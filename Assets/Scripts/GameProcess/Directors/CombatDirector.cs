using System.Collections;
using System.Collections.Generic;
using GameProcess.Directors;
using GameProcess.Directors.Functions;
using UnityEngine;

public class CombatDirector : MonoBehaviour
{
    [Header("Core Director Values")]
    public string customName;
    public float monsterCredit;
    [Tooltip("Monster credit that's been refunded from culling non-elite enemies. Can only be used to buy non-elite enemies.")]
    public float refundedMonsterCredit;
    public float expRewardCoefficient = 0.2f;
    public float goldRewardCoefficient = 1f;
    public float minSeriesSpawnInterval = 0.1f;
    public float maxSeriesSpawnInterval = 1f;
    public float minRerollSpawnInterval = 2.33333325f;
    public float maxRerollSpawnInterval = 4.33333349f;
    public RangeFloat[] moneyWaveIntervals;
    [Tooltip("How much to multiply money wave yield by.")]
    [Header("Optional Behaviors")]
    public float creditMultiplier = 1f;
    [Tooltip("The coefficient to multiply spawn distances. Used for combat shrines, to keep spawns nearby.")]
    public float spawnDistanceMultiplier = 1f;
    [Tooltip("The maximum distance at which enemies will spawn.")]
    public float maxSpawnDistance = float.PositiveInfinity;
    [Tooltip("Ensure that the minimum spawn distance is at least this many units away from the maxSpawnDistance")]
    public float minSpawnRange;
    public bool shouldSpawnOneWave;
    public bool targetPlayers = true;
    public bool skipSpawnIfTooCheap = true;
    [Tooltip("If skipSpawnIfTooCheap is true, we'll behave as though it's not set after this many consecutive skips")]
    public int maxConsecutiveCheapSkips = int.MaxValue;
    public bool resetMonsterCardIfFailed = true;
    public int maximumNumberToSpawnBeforeSkipping = 6;
    public float eliteBias = 1f;
    public CombatDirector.OnSpawnedServer onSpawnedServer;
    [Tooltip("A special effect for when a monster appears will be instantiated at its position. Used for combat shrine.")]
    public GameObject spawnEffectPrefab;
    public bool ignoreTeamSizeLimit;
    [SerializeField]
    private DirectorCardCategorySelection _monsterCards;
    public bool fallBackToStageMonsterCards = true;
    public static readonly List<CombatDirector> instancesList = new List<CombatDirector>();
    private bool hasStartedWave;
    private DirectorCard currentMonsterCard;
    private int currentMonsterCardCost;
    private WeightedSelection<DirectorCard> monsterCardsSelection;
    private int consecutiveCheapSkips;
    public GameObject currentSpawnTarget;
    private float playerRetargetTimer;
    private int spawnCountInCurrentWave;
    private CombatDirector.DirectorMoneyWave[] moneyWaves;
    private bool isHalcyonShrineSpawn;
    private int shrineHalcyoniteDifficultyLevel;

    public float monsterSpawnTimer { get; set; }

    public DirectorCard lastAttemptedMonsterCard { get; set; }

    public float totalCreditsSpent { get; private set; }

    private WeightedSelection<DirectorCard> finalMonsterCardsSelection
    {
      get
      {
        WeightedSelection<DirectorCard> monsterCardsSelection = this.monsterCardsSelection;
        if (monsterCardsSelection != null)
          return monsterCardsSelection;
        return ClassicStageInfo.instance?.monsterSelection;
      }
    }

    private DirectorCardCategorySelection monsterCards
    {
      get => this._monsterCards;
      set
      {
        if (!((UnityEngine.Object) this._monsterCards != (UnityEngine.Object) value))
          return;
        this._monsterCards = value;
        this.monsterCardsSelection = this._monsterCards?.GenerateDirectorCardWeightedSelection();
      }
    }

    private void Awake()
    {
      if (!NetworkServer.active)
        return;
      this.rng = new Xoroshiro128Plus((ulong) Run.instance.stageRng.nextUint);
      this.moneyWaves = new CombatDirector.DirectorMoneyWave[this.moneyWaveIntervals.Length];
      for (int index = 0; index < this.moneyWaveIntervals.Length; ++index)
        this.moneyWaves[index] = new CombatDirector.DirectorMoneyWave()
        {
          interval = this.rng.RangeFloat(this.moneyWaveIntervals[index].min, this.moneyWaveIntervals[index].max),
          multiplier = this.creditMultiplier
        };
      this.monsterCardsSelection = this.monsterCards?.GenerateDirectorCardWeightedSelection();
    }

    private void OnEnable() => CombatDirector.instancesList.Add(this);

    private void OnDisable()
    {
      CombatDirector.instancesList.Remove(this);
      if (!NetworkServer.active || CombatDirector.instancesList.Count <= 0)
        return;
      float num = 0.4f;
      CombatDirector combatDirector = this.rng.NextElementUniform<CombatDirector>(CombatDirector.instancesList);
      this.monsterCredit *= num;
      combatDirector.monsterCredit += this.monsterCredit;
      Debug.LogFormat("Transfered {0} monster credits from {1} to {2}", (object) this.monsterCredit, (object) this.gameObject, (object) combatDirector.gameObject);
      this.monsterCredit = 0.0f;
    }

    private void GenerateAmbush(Vector3 victimPosition)
    {
      NodeGraph groundNodes = SceneInfo.instance.groundNodes;
      NodeGraph.NodeIndex closestNode = groundNodes.FindClosestNode(victimPosition, HullClassification.Human);
      NodeGraphSpider nodeGraphSpider = new NodeGraphSpider(groundNodes, HullMask.Human);
      nodeGraphSpider.AddNodeForNextStep(closestNode);
      List<NodeGraphSpider.StepInfo> stepInfoList = new List<NodeGraphSpider.StepInfo>();
      int num = 0;
      List<NodeGraphSpider.StepInfo> collectedSteps = nodeGraphSpider.collectedSteps;
      while (nodeGraphSpider.PerformStep() && num < 8)
      {
        ++num;
        for (int index = 0; index < collectedSteps.Count; ++index)
        {
          if (CombatDirector.IsAcceptableAmbushSpiderStep(groundNodes, closestNode, collectedSteps[index]))
            stepInfoList.Add(collectedSteps[index]);
        }
        collectedSteps.Clear();
      }
      for (int index = 0; index < stepInfoList.Count; ++index)
      {
        Vector3 position;
        groundNodes.GetNodePosition(stepInfoList[index].node, out position);
        LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/scLemurian").DoSpawn(position, Quaternion.identity, (DirectorSpawnRequest) null);
      }
    }

    private static bool IsAcceptableAmbushSpiderStep(
      NodeGraph nodeGraph,
      NodeGraph.NodeIndex startNode,
      NodeGraphSpider.StepInfo stepInfo)
    {
      int num = 0;
      while (stepInfo.previousStep != null && !nodeGraph.TestNodeLineOfSight(startNode, stepInfo.node))
      {
        stepInfo = stepInfo.previousStep;
        ++num;
        if (num > 2)
          return true;
      }
      return false;
    }

    public void OverrideCurrentMonsterCard(DirectorCard overrideMonsterCard) => this.PrepareNewMonsterWave(overrideMonsterCard);

    public void SetNextSpawnAsBoss()
    {
      WeightedSelection<DirectorCard> weightedSelection = new WeightedSelection<DirectorCard>();
      int i = 0;
      for (int count = this.finalMonsterCardsSelection.Count; i < count; ++i)
      {
        WeightedSelection<DirectorCard>.ChoiceInfo choice = this.finalMonsterCardsSelection.GetChoice(i);
        SpawnCard spawnCard = choice.value.spawnCard;
        bool isChampion = spawnCard.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<CharacterBody>().isChampion;
        bool flag = spawnCard is CharacterSpawnCard characterSpawnCard && characterSpawnCard.forbiddenAsBoss;
        if (isChampion && !flag && choice.value.IsAvailable())
          weightedSelection.AddChoice(choice);
      }
      if (weightedSelection.Count > 0)
        this.PrepareNewMonsterWave(weightedSelection.Evaluate(this.rng.nextNormalizedFloat));
      this.monsterSpawnTimer = -600f;
    }

    private void PickPlayerAsSpawnTarget()
    {
      ReadOnlyCollection<PlayerCharacterMasterController> instances = PlayerCharacterMasterController.instances;
      List<PlayerCharacterMasterController> list = new List<PlayerCharacterMasterController>();
      foreach (PlayerCharacterMasterController masterController in instances)
      {
        if (masterController.master.hasBody)
          list.Add(masterController);
      }
      if (list.Count <= 0)
        return;
      this.currentSpawnTarget = this.rng.NextElementUniform<PlayerCharacterMasterController>(list).master.GetBodyObject();
    }

    public void SpendAllCreditsOnMapSpawns(Transform mapSpawnTarget)
    {
      int num1 = 0;
      int num2 = 10;
      while ((double) this.monsterCredit > 0.0)
      {
        this.PrepareNewMonsterWave(this.finalMonsterCardsSelection.Evaluate(this.rng.nextNormalizedFloat));
        if (!(bool) (UnityEngine.Object) mapSpawnTarget ? this.AttemptSpawnOnTarget((Transform) null, (bool) (UnityEngine.Object) SceneInfo.instance.approximateMapBoundMesh ? DirectorPlacementRule.PlacementMode.RandomNormalized : DirectorPlacementRule.PlacementMode.Random) : this.AttemptSpawnOnTarget(mapSpawnTarget))
        {
          num1 = 0;
        }
        else
        {
          ++num1;
          if (num1 >= num2)
            break;
        }
      }
    }

    public void ToggleAllCombatDirectorsOnThisObject(bool newValue)
    {
      foreach (Behaviour component in this.GetComponents<CombatDirector>())
        component.enabled = newValue;
    }

    private void Simulate(float deltaTime)
    {
      if (this.targetPlayers)
      {
        this.playerRetargetTimer -= deltaTime;
        if ((double) this.playerRetargetTimer <= 0.0)
        {
          this.playerRetargetTimer = this.rng.RangeFloat(1f, 10f);
          this.PickPlayerAsSpawnTarget();
        }
      }
      this.monsterSpawnTimer -= deltaTime;
      if ((double) this.monsterSpawnTimer > 0.0)
        return;
      if (this.AttemptSpawnOnTarget((bool) (UnityEngine.Object) this.currentSpawnTarget ? this.currentSpawnTarget.transform : (Transform) null))
      {
        if (this.shouldSpawnOneWave)
          this.hasStartedWave = true;
        this.monsterSpawnTimer += this.rng.RangeFloat(this.minSeriesSpawnInterval, this.maxSeriesSpawnInterval);
      }
      else
      {
        this.monsterSpawnTimer += this.rng.RangeFloat(this.minRerollSpawnInterval, this.maxRerollSpawnInterval);
        if (this.resetMonsterCardIfFailed)
          this.currentMonsterCard = (DirectorCard) null;
        if (!this.shouldSpawnOneWave || !this.hasStartedWave)
          return;
        this.enabled = false;
      }
    }

    public static bool IsEliteOnlyArtifactActive() => RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.eliteOnlyArtifactDef);

    private static bool NotEliteOnlyArtifactActive() => !CombatDirector.IsEliteOnlyArtifactActive();

    [SystemInitializer(new System.Type[] {typeof (EliteCatalog)})]
    private static void Init() => CombatDirector.eliteTiers = new CombatDirector.EliteTierDef[6]
    {
      new CombatDirector.EliteTierDef()
      {
        costMultiplier = 1f,
        eliteTypes = new EliteDef[1],
        isAvailable = (Func<SpawnCard.EliteRules, bool>) (rules => CombatDirector.NotEliteOnlyArtifactActive()),
        canSelectWithoutAvailableEliteDef = true
      },
      new CombatDirector.EliteTierDef()
      {
        costMultiplier = CombatDirector.baseEliteCostMultiplier,
        eliteTypes = new EliteDef[4]
        {
          RoR2Content.Elites.Lightning,
          RoR2Content.Elites.Ice,
          RoR2Content.Elites.Fire,
          DLC1Content.Elites.Earth
        },
        isAvailable = (Func<SpawnCard.EliteRules, bool>) (rules => CombatDirector.NotEliteOnlyArtifactActive() && rules == SpawnCard.EliteRules.Default),
        canSelectWithoutAvailableEliteDef = false
      },
      new CombatDirector.EliteTierDef()
      {
        costMultiplier = Mathf.LerpUnclamped(1f, CombatDirector.baseEliteCostMultiplier, 0.5f),
        eliteTypes = new EliteDef[4]
        {
          RoR2Content.Elites.LightningHonor,
          RoR2Content.Elites.IceHonor,
          RoR2Content.Elites.FireHonor,
          DLC1Content.Elites.EarthHonor
        },
        isAvailable = (Func<SpawnCard.EliteRules, bool>) (rules => CombatDirector.IsEliteOnlyArtifactActive()),
        canSelectWithoutAvailableEliteDef = false
      },
      new CombatDirector.EliteTierDef()
      {
        costMultiplier = Mathf.LerpUnclamped(1f, CombatDirector.baseEliteCostMultiplier, 0.5f),
        eliteTypes = new EliteDef[5]
        {
          RoR2Content.Elites.Lightning,
          RoR2Content.Elites.Ice,
          RoR2Content.Elites.Fire,
          DLC1Content.Elites.Earth,
          DLC2Content.Elites.Aurelionite
        },
        isAvailable = (Func<SpawnCard.EliteRules, bool>) (rules => CombatDirector.NotEliteOnlyArtifactActive() && rules == SpawnCard.EliteRules.Default && Run.instance.stageClearCount >= 2),
        canSelectWithoutAvailableEliteDef = false
      },
      new CombatDirector.EliteTierDef()
      {
        costMultiplier = CombatDirector.baseEliteCostMultiplier * 6f,
        eliteTypes = new EliteDef[3]
        {
          RoR2Content.Elites.Poison,
          RoR2Content.Elites.Haunted,
          DLC2Content.Elites.Bead
        },
        isAvailable = (Func<SpawnCard.EliteRules, bool>) (rules => Run.instance.loopClearCount > 0 && rules == SpawnCard.EliteRules.Default),
        canSelectWithoutAvailableEliteDef = false
      },
      new CombatDirector.EliteTierDef()
      {
        costMultiplier = CombatDirector.baseEliteCostMultiplier,
        eliteTypes = new EliteDef[1]
        {
          RoR2Content.Elites.Lunar
        },
        isAvailable = (Func<SpawnCard.EliteRules, bool>) (rules => rules == SpawnCard.EliteRules.Lunar),
        canSelectWithoutAvailableEliteDef = false
      }
    };

    public static float CalcHighestEliteCostMultiplier(SpawnCard.EliteRules eliteRules)
    {
      float a = 1f;
      for (int index = 1; index < CombatDirector.eliteTiers.Length; ++index)
      {
        if (CombatDirector.eliteTiers[index].CanSelect(eliteRules))
          a = Mathf.Max(a, CombatDirector.eliteTiers[index].costMultiplier);
      }
      return a;
    }

    public static float lowestEliteCostMultiplier => CombatDirector.eliteTiers[1].costMultiplier;

    private int mostExpensiveMonsterCostInDeck
    {
      get
      {
        int a = 0;
        for (int i = 0; i < this.finalMonsterCardsSelection.Count; ++i)
        {
          DirectorCard directorCard = this.finalMonsterCardsSelection.GetChoice(i).value;
          int b = directorCard.cost;
          if (!(directorCard.spawnCard as CharacterSpawnCard).noElites)
            b = (int) ((double) b * (double) CombatDirector.CalcHighestEliteCostMultiplier(directorCard.spawnCard.eliteRules));
          a = Mathf.Max(a, b);
        }
        return a;
      }
    }

    private void ResetEliteType()
    {
      this.currentActiveEliteTier = CombatDirector.eliteTiers[0];
      for (int index = 0; index < CombatDirector.eliteTiers.Length; ++index)
      {
        if (CombatDirector.eliteTiers[index].CanSelect(this.currentMonsterCard.spawnCard.eliteRules))
        {
          this.currentActiveEliteTier = CombatDirector.eliteTiers[index];
          break;
        }
      }
      this.currentActiveEliteDef = this.currentActiveEliteTier.GetRandomAvailableEliteDef(this.rng);
    }

    private void PrepareNewMonsterWave(DirectorCard monsterCard)
    {
      if (CombatDirector.cvDirectorCombatEnableInternalLogs.value)
        Debug.LogFormat("Preparing monster wave {0}", (object) monsterCard.spawnCard);
      this.currentMonsterCard = monsterCard;
      this.ResetEliteType();
      if (!(this.currentMonsterCard.spawnCard as CharacterSpawnCard).noElites)
      {
        for (int index = 1; index < CombatDirector.eliteTiers.Length; ++index)
        {
          CombatDirector.EliteTierDef eliteTier = CombatDirector.eliteTiers[index];
          if (!eliteTier.CanSelect(this.currentMonsterCard.spawnCard.eliteRules))
          {
            if (CombatDirector.cvDirectorCombatEnableInternalLogs.value)
              Debug.LogFormat("Elite tier index {0} is unavailable", (object) index);
          }
          else
          {
            float num = (float) this.currentMonsterCard.cost * eliteTier.costMultiplier * this.eliteBias;
            if ((double) num <= (double) this.monsterCredit)
            {
              this.currentActiveEliteTier = eliteTier;
              if (CombatDirector.cvDirectorCombatEnableInternalLogs.value)
                Debug.LogFormat("Found valid elite tier index {0}", (object) index);
            }
            else if (CombatDirector.cvDirectorCombatEnableInternalLogs.value)
              Debug.LogFormat("Elite tier index {0} is too expensive ({1}/{2})", (object) index, (object) num, (object) this.monsterCredit);
          }
        }
      }
      else if (CombatDirector.cvDirectorCombatEnableInternalLogs.value)
        Debug.LogFormat("Card {0} cannot be elite. Skipping elite procedure.", (object) this.currentMonsterCard.spawnCard);
      this.currentActiveEliteDef = this.currentActiveEliteTier.GetRandomAvailableEliteDef(this.rng);
      if (CombatDirector.cvDirectorCombatEnableInternalLogs.value)
        Debug.LogFormat("Assigned elite index {0}", (object) this.currentActiveEliteDef);
      this.lastAttemptedMonsterCard = this.currentMonsterCard;
      this.spawnCountInCurrentWave = 0;
    }

    private bool AttemptSpawnOnTarget(
      Transform spawnTarget,
      DirectorPlacementRule.PlacementMode placementMode = DirectorPlacementRule.PlacementMode.Approximate)
    {
      if (this.currentMonsterCard == null)
      {
        int num = CombatDirector.cvDirectorCombatEnableInternalLogs.value ? 1 : 0;
        if (this.finalMonsterCardsSelection == null)
          return false;
        this.PrepareNewMonsterWave(this.finalMonsterCardsSelection.Evaluate(this.rng.nextNormalizedFloat));
      }
      if (this.spawnCountInCurrentWave >= this.maximumNumberToSpawnBeforeSkipping)
      {
        this.spawnCountInCurrentWave = 0;
        if (CombatDirector.cvDirectorCombatEnableInternalLogs.value)
          Debug.LogFormat("Spawn count has hit the max ({0}/{1}). Aborting spawn.", (object) this.spawnCountInCurrentWave, (object) this.maximumNumberToSpawnBeforeSkipping);
        return false;
      }
      int num1 = this.currentMonsterCard.cost;
      int cost = this.currentMonsterCard.cost;
      float num2 = 1f;
      EliteDef currentActiveEliteDef = this.currentActiveEliteDef;
      int num3 = (int) ((double) num1 * (double) this.currentActiveEliteTier.costMultiplier);
      if ((double) num3 <= (double) this.monsterCredit)
      {
        num1 = num3;
        num2 = this.currentActiveEliteTier.costMultiplier;
      }
      else
      {
        this.ResetEliteType();
        currentActiveEliteDef = this.currentActiveEliteDef;
      }
      if (!this.currentMonsterCard.IsAvailable())
      {
        if (CombatDirector.cvDirectorCombatEnableInternalLogs.value)
          Debug.LogFormat("Spawn card {0} is invalid, aborting spawn.", (object) this.currentMonsterCard.spawnCard);
        return false;
      }
      if ((double) this.monsterCredit < (double) num1)
      {
        if (CombatDirector.cvDirectorCombatEnableInternalLogs.value)
          Debug.LogFormat("Spawn card {0} is too expensive, aborting spawn.", (object) this.currentMonsterCard.spawnCard);
        return false;
      }
      if (this.skipSpawnIfTooCheap && this.consecutiveCheapSkips < this.maxConsecutiveCheapSkips && (double) (num3 * this.maximumNumberToSpawnBeforeSkipping) < (double) this.monsterCredit)
      {
        if (CombatDirector.cvDirectorCombatEnableInternalLogs.value)
          Debug.LogFormat("Card {0} seems too cheap ({1}/{2}). Comparing against most expensive possible ({3})", (object) this.currentMonsterCard.spawnCard, (object) (num1 * this.maximumNumberToSpawnBeforeSkipping), (object) this.monsterCredit, (object) this.mostExpensiveMonsterCostInDeck);
        if (this.mostExpensiveMonsterCostInDeck > num1)
        {
          ++this.consecutiveCheapSkips;
          if (CombatDirector.cvDirectorCombatEnableInternalLogs.value)
            Debug.LogFormat("Spawn card {0} is too cheap, aborting spawn.", (object) this.currentMonsterCard.spawnCard);
          return false;
        }
      }
      SpawnCard spawnCard = this.currentMonsterCard.spawnCard;
      EliteDef eliteDef = currentActiveEliteDef;
      Transform spawnTarget1 = spawnTarget;
      float num4 = num2;
      DirectorPlacementRule.PlacementMode placementMode1 = placementMode;
      bool preventOverhead = this.currentMonsterCard.preventOverhead;
      int spawnDistance = (int) this.currentMonsterCard.spawnDistance;
      int num5 = preventOverhead ? 1 : 0;
      double valueMultiplier = (double) num4;
      int num6 = (int) placementMode1;
      if (!this.Spawn(spawnCard, eliteDef, spawnTarget1, (DirectorCore.MonsterSpawnDistance) spawnDistance, num5 != 0, (float) valueMultiplier, (DirectorPlacementRule.PlacementMode) num6))
        return false;
      this.monsterCredit -= (float) num1;
      this.totalCreditsSpent += (float) num1;
      ++this.spawnCountInCurrentWave;
      this.consecutiveCheapSkips = 0;
      return true;
    }

    public bool Spawn(
      SpawnCard spawnCard,
      EliteDef eliteDef,
      Transform spawnTarget,
      DirectorCore.MonsterSpawnDistance spawnDistance,
      bool preventOverhead,
      float valueMultiplier = 1f,
      DirectorPlacementRule.PlacementMode placementMode = DirectorPlacementRule.PlacementMode.Approximate)
    {
      DirectorPlacementRule placementRule = new DirectorPlacementRule()
      {
        placementMode = placementMode,
        spawnOnTarget = spawnTarget,
        preventOverhead = preventOverhead
      };
      DirectorCore.GetMonsterSpawnDistance(spawnDistance, out placementRule.minDistance, out placementRule.maxDistance);
      placementRule.maxDistance = Mathf.Min(this.maxSpawnDistance, placementRule.maxDistance * this.spawnDistanceMultiplier);
      placementRule.minDistance = Mathf.Max(0.0f, Mathf.Min(placementRule.maxDistance - this.minSpawnRange, placementRule.minDistance * this.spawnDistanceMultiplier));
      if ((bool) (UnityEngine.Object) DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, this.rng)
      {
        ignoreTeamMemberLimit = this.ignoreTeamSizeLimit,
        teamIndexOverride = new TeamIndex?(this.teamIndex),
        onSpawnedServer = new Action<SpawnCard.SpawnResult>(OnCardSpawned)
      }))
        return true;
      Debug.LogFormat("Spawn card {0} failed to spawn. Aborting cost procedures.", (object) spawnCard);
      return false;

      void OnCardSpawned(SpawnCard.SpawnResult result)
      {
        if (!result.success)
          return;
        CharacterMaster component1 = result.spawnedInstance.GetComponent<CharacterMaster>();
        GameObject bodyObject = component1.GetBodyObject();
        CharacterBody component2 = bodyObject.GetComponent<CharacterBody>();
        if ((bool) (UnityEngine.Object) component2)
          component2.cost = valueMultiplier * (float) spawnCard.directorCreditCost;
        if ((bool) (UnityEngine.Object) this.combatSquad)
          this.combatSquad.AddMember(component1);
        EliteDef eliteDef1 = eliteDef;
        float num1 = eliteDef1 != null ? eliteDef1.healthBoostCoefficient : 1f;
        EliteDef eliteDef2 = eliteDef;
        float num2 = eliteDef2 != null ? eliteDef2.damageBoostCoefficient : 1f;
        if (this.isHalcyonShrineSpawn)
        {
          if (this.shrineHalcyoniteDifficultyLevel > 20)
            this.shrineHalcyoniteDifficultyLevel = 20 + this.shrineHalcyoniteDifficultyLevel / 100;
          num1 += (float) this.shrineHalcyoniteDifficultyLevel * 0.6f;
          num2 += (float) this.shrineHalcyoniteDifficultyLevel * 0.25f;
          eliteDef = DLC2Content.Elites.Aurelionite;
        }
        EquipmentIndex newEquipmentIndex = (EquipmentIndex) ((int) eliteDef?.eliteEquipmentDef?.equipmentIndex ?? -1);
        if (newEquipmentIndex != EquipmentIndex.None)
          component1.inventory.SetEquipmentIndex(newEquipmentIndex);
        if ((bool) (UnityEngine.Object) this.combatSquad && this.combatSquad.grantBonusHealthInMultiplayer)
        {
          int livingPlayerCount = Run.instance.livingPlayerCount;
          num1 *= Mathf.Pow((float) livingPlayerCount, 1f);
        }
        component1.inventory.GiveItem(RoR2Content.Items.BoostHp, Mathf.RoundToInt((float) (((double) num1 - 1.0) * 10.0)));
        component1.inventory.GiveItem(RoR2Content.Items.BoostDamage, Mathf.RoundToInt((float) (((double) num2 - 1.0) * 10.0)));
        DeathRewards component3 = bodyObject.GetComponent<DeathRewards>();
        if ((bool) (UnityEngine.Object) component3)
        {
          float b = (float) spawnCard.directorCreditCost * valueMultiplier * this.expRewardCoefficient;
          component3.spawnValue = (int) Mathf.Max(1f, b);
          if ((double) b > (double) Mathf.Epsilon)
          {
            component3.expReward = (uint) Mathf.Max(1f, b * Run.instance.compensatedDifficultyCoefficient);
            component3.goldReward = (uint) Mathf.Max(1f, (float) ((double) b * (double) this.goldRewardCoefficient * 2.0) * Run.instance.compensatedDifficultyCoefficient);
          }
          else
          {
            component3.expReward = 0U;
            component3.goldReward = 0U;
          }
        }
        if ((bool) (UnityEngine.Object) this.spawnEffectPrefab && NetworkServer.active)
        {
          Vector3 vector3 = result.position;
          CharacterBody characterBody = component2;
          if ((bool) (UnityEngine.Object) characterBody)
            vector3 = characterBody.corePosition;
          EffectManager.SpawnEffect(this.spawnEffectPrefab, new EffectData()
          {
            origin = vector3
          }, true);
        }
        this.onSpawnedServer?.Invoke(result.spawnedInstance);
      }
    }

    private void FixedUpdate()
    {
      if (CombatDirector.cvDirectorCombatDisable.value || !NetworkServer.active || !(bool) (UnityEngine.Object) Run.instance)
        return;
      float difficultyCoefficient = Run.instance.compensatedDifficultyCoefficient;
      for (int index = 0; index < this.moneyWaves.Length; ++index)
        this.monsterCredit += this.moneyWaves[index].Update(Time.fixedDeltaTime, difficultyCoefficient);
      this.Simulate(Time.fixedDeltaTime);
    }

    public void CombatShrineActivation(
      Interactor interactor,
      float monsterCredit,
      DirectorCard chosenDirectorCard)
    {
      this.enabled = true;
      this.monsterCredit += monsterCredit;
      this.OverrideCurrentMonsterCard(chosenDirectorCard);
      this.monsterSpawnTimer = 0.0f;
      CharacterMaster component1 = chosenDirectorCard.spawnCard.prefab.GetComponent<CharacterMaster>();
      if (!(bool) (UnityEngine.Object) component1)
        return;
      CharacterBody component2 = component1.bodyPrefab.GetComponent<CharacterBody>();
      if (!(bool) (UnityEngine.Object) component2)
        return;
      Chat.SubjectFormatChatMessage message = new Chat.SubjectFormatChatMessage();
      message.subjectAsCharacterBody = interactor.GetComponent<CharacterBody>();
      message.baseToken = "SHRINE_COMBAT_USE_MESSAGE";
      message.paramTokens = new string[1]
      {
        component2.baseNameToken
      };
      Chat.SendBroadcastChat((ChatMessageBase) message);
    }

    public DirectorCard SelectMonsterCardForCombatShrine(float monsterCredit)
    {
      WeightedSelection<DirectorCard> directorCardSpawnList = Util.CreateReasonableDirectorCardSpawnList(monsterCredit, this.maximumNumberToSpawnBeforeSkipping, 1);
      return directorCardSpawnList.Count == 0 ? (DirectorCard) null : directorCardSpawnList.Evaluate(this.rng.nextNormalizedFloat);
    }

    public void HalcyoniteShrineActivation(
      float monsterCredit,
      DirectorCard chosenDirectorCard,
      int difficultyLevel,
      Transform shrineTransform)
    {
      this.enabled = true;
      this.monsterCredit += monsterCredit;
      this.isHalcyonShrineSpawn = true;
      this.shrineHalcyoniteDifficultyLevel = difficultyLevel;
      this.OverrideCurrentMonsterCard(chosenDirectorCard);
      this.monsterSpawnTimer = 0.0f;
      int num = (int) Util.PlaySound("Play_halcyonite_spawn", shrineTransform.gameObject);
    }

    public void SetMonsterCredit(float newMonsterCredit) => this.monsterCredit = newMonsterCredit;

    [Serializable]
    public class OnSpawnedServer : UnityEvent<GameObject>
    {
    }

    public class EliteTierDef
    {
      public float costMultiplier;
      public EliteDef[] eliteTypes;
      public Func<SpawnCard.EliteRules, bool> isAvailable = (Func<SpawnCard.EliteRules, bool>) (rules => true);
      public bool canSelectWithoutAvailableEliteDef;
      private List<EliteDef> availableDefs = new List<EliteDef>();

      public bool CanSelect(SpawnCard.EliteRules rules)
      {
        if (!this.isAvailable(rules))
          return false;
        return this.canSelectWithoutAvailableEliteDef || this.HasAnyAvailableEliteDefs();
      }

      public bool HasAnyAvailableEliteDefs()
      {
        foreach (EliteDef eliteType in this.eliteTypes)
        {
          if ((bool) (UnityEngine.Object) eliteType && eliteType.IsAvailable())
            return true;
        }
        return false;
      }

      public EliteDef GetRandomAvailableEliteDef(Xoroshiro128Plus rng)
      {
        this.availableDefs.Clear();
        foreach (EliteDef eliteType in this.eliteTypes)
        {
          if ((bool) (UnityEngine.Object) eliteType && eliteType.IsAvailable())
            this.availableDefs.Add(eliteType);
        }
        return this.availableDefs.Count > 0 ? rng.NextElementUniform<EliteDef>(this.availableDefs) : (EliteDef) null;
      }
    }

    private class DirectorMoneyWave
    {
      public float interval;
      public float timer;
      public float multiplier;
      private float accumulatedAward;

      public float Update(float deltaTime, float difficultyCoefficient)
      {
        this.timer += deltaTime;
        if ((double) this.timer > (double) this.interval)
        {
          float num = (float) (0.5 + (double) Run.instance.participatingPlayerCount * 0.5);
          this.timer -= this.interval;
          this.accumulatedAward += (float) ((double) this.interval * (double) this.multiplier * (1.0 + 0.40000000596046448 * (double) difficultyCoefficient)) * num;
        }
        float num1 = (float) Mathf.FloorToInt(this.accumulatedAward);
        this.accumulatedAward -= num1;
        return num1;
      }
    }
}
