using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GameProcess.Cards;
using GameProcess.Directors;
using GameProcess.Directors.Functions;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

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
    [Tooltip("A special effect for when a monster appears will be instantiated at its position. Used for combat shrine.")]
    public GameObject spawnEffectPrefab;
    public bool ignoreTeamSizeLimit;
    [SerializeField]
    private DirectorCardCategorySelection _monsterCards;
    public bool fallBackToStageMonsterCards = true;
    public static readonly List<CombatDirector> instancesList = new List<CombatDirector>();
    private bool hasStartedWave;

    private DirectorCard currentMonsterCard;

    [SerializeField] private WeightedSelection<DirectorCard> monster;

    private int currentMonsterCardCost;
    private WeightedSelection<DirectorCard> monsterCardsSelection;
    private int consecutiveCheapSkips;
    public GameObject currentSpawnTarget;
    private float playerRetargetTimer;
    private static readonly float baseEliteCostMultiplier = 6f;

    private int spawnCountInCurrentWave;
    private DirectorMoneyWave[] moneyWaves;
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
        //return ClassicStageInfo.instance?.monsterSelection;
        return monster;
      }
    }
    
    private int mostExpensiveMonsterCostInDeck
    {
      get
      {
        int a = 0;
        for (int i = 0; i < this.finalMonsterCardsSelection.Count; ++i)
        {
          DirectorCard directorCard = this.finalMonsterCardsSelection.GetChoice(i).value;
          int b = directorCard.Cost;
          a = Mathf.Max(a, b);
        }
        return a;
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
          float num = 0.5f;
          this.timer -= this.interval;
          this.accumulatedAward += (float) ((double) this.interval * (double) this.multiplier * (1.0 + 0.40000000596046448 * (double) difficultyCoefficient)) * num;
        }
        float num1 = (float) Mathf.FloorToInt(this.accumulatedAward);
        this.accumulatedAward -= num1;
        return num1;
      }
    }
    
    private void PrepareNewMonsterWave(DirectorCard monsterCard)
    {
      this.currentMonsterCard = monsterCard;
      this.lastAttemptedMonsterCard = this.currentMonsterCard;
      this.spawnCountInCurrentWave = 0;
    }

    private bool AttemptSpawnOnTarget(Transform spawnTarget)
    {
      if (this.currentMonsterCard == null)
      {
        if (this.finalMonsterCardsSelection == null)
          return false;
        PrepareNewMonsterWave(this.finalMonsterCardsSelection.Evaluate(0));
      }
      if (this.spawnCountInCurrentWave >= this.maximumNumberToSpawnBeforeSkipping)
      {
        this.spawnCountInCurrentWave = 0;
        return false;
      }
      int num1 = currentMonsterCard.Cost;
      int cost = currentMonsterCard.Cost;
      float num2 = 1f;
      if (skipSpawnIfTooCheap && consecutiveCheapSkips < maxConsecutiveCheapSkips)
      {
        if (mostExpensiveMonsterCostInDeck > num1)
        {
          ++consecutiveCheapSkips;
        }
      }
      SpawnCard spawnCard = currentMonsterCard.spawnCard;
      Transform spawnTarget1 = spawnTarget;
      if (!Spawn(spawnCard, spawnTarget1))
        return false;
      monsterCredit -= num1;
      totalCreditsSpent += num1;
      ++spawnCountInCurrentWave;
      consecutiveCheapSkips = 0;
      return true;
    }

    public bool Spawn(SpawnCard spawnCard, Transform spawnTarget)
    {
      if (DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard)))
      {
        Action<SpawnCard.SpawnResult> onSpawnedServer = new Action<SpawnCard.SpawnResult>(OnCardSpawned);
        return true;
      }
      return false;
    }

    private void OnCardSpawned(SpawnCard.SpawnResult result)
      {
        SpawnCard spawnCard = result.SpawnedInstance.GetComponent<SpawnCard>();
        GameObject bodyObject = result.SpawnedInstance;
        DeathRewards component3 = bodyObject.GetComponent<DeathRewards>();
        if ((UnityEngine.Object) component3)
        {
          float b = spawnCard.DirectorCreditCost * expRewardCoefficient;
          component3.spawnValue = (int) Mathf.Max(1f, b);
          if (b > Mathf.Epsilon)
          {
            //component3.expReward = Mathf.Max(1f, b * Run.instance.compensatedDifficultyCoefficient);
            //component3.goldReward = Mathf.Max(1f, (b * goldRewardCoefficient * 2.0) * Run.instance.compensatedDifficultyCoefficient);
          }
          else
          {
            component3.expReward = 0U;
            component3.goldReward = 0U;
          }
        }
      }
    
    private void FixedUpdate()
    {
      //float difficultyCoefficient = Run.instance.compensatedDifficultyCoefficient;
      for (int index = 0; index < moneyWaves.Length; ++index)
        //monsterCredit += moneyWaves[index].Update(Time.fixedDeltaTime, difficultyCoefficient);
      Simulate(Time.fixedDeltaTime);
    }
    
    private void Simulate(float deltaTime)
    {
      monsterSpawnTimer -= deltaTime;
      if (monsterSpawnTimer > 0.0)
        return;
      if (AttemptSpawnOnTarget((bool) currentSpawnTarget ? currentSpawnTarget.transform : null))
      {
        if (shouldSpawnOneWave)
          hasStartedWave = true;
        monsterSpawnTimer += Random.Range(minSeriesSpawnInterval, maxSeriesSpawnInterval);
      }
      else
      {
        monsterSpawnTimer += Random.Range(minRerollSpawnInterval, maxRerollSpawnInterval);
        if (resetMonsterCardIfFailed)
          currentMonsterCard = null;
        if (!shouldSpawnOneWave || !hasStartedWave)
          return;
        enabled = false;
      }
    }
}
