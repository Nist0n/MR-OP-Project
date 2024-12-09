using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GameProcess;
using GameProcess.Cards;
using GameProcess.Directors;
using GameProcess.Directors.Functions;
using Player;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class CombatDirector : MonoBehaviour
{
  [SerializeField] private float radius;
  
  public WeightSelection category;
    public float monsterCredit;
    [Tooltip("Monster credit that's been refunded from culling non-elite enemies. Can only be used to buy non-elite enemies.")]
    public float expRewardCoefficient = 0.2f;
    public float goldRewardCoefficient = 1f;
    public float minSeriesSpawnInterval = 0.1f;
    public float maxSeriesSpawnInterval = 1f;
    public float minRerollSpawnInterval;
    public float maxRerollSpawnInterval;
    [Tooltip("Ensure that the minimum spawn distance is at least this many units away from the maxSpawnDistance")]
    public bool shouldSpawnOneWave;
    public bool skipSpawnIfTooCheap = true;
    [Tooltip("If skipSpawnIfTooCheap is true, we'll behave as though it's not set after this many consecutive skips")]
    public int maxConsecutiveCheapSkips = int.MaxValue;
    public bool resetMonsterCardIfFailed = true;
    public int maximumNumberToSpawnBeforeSkipping = 6;
    private bool hasStartedWave;

    private Transform _player;
    
    public RangeFloat[] moneyWaveIntervals;

    private DirectorCard currentMonsterCard;

    private int currentMonsterCardCost;
    private int consecutiveCheapSkips;
    private float playerRetargetTimer;

    private int spawnCountInCurrentWave;
    private DirectorMoneyWave[] moneyWaves;
    private bool isHalcyonShrineSpawn;
    private int shrineHalcyoniteDifficultyLevel;

    public float monsterSpawnTimer { get; set; }

    public DirectorCard lastAttemptedMonsterCard { get; set; }

    public float totalCreditsSpent { get; private set; }
    
    private int mostExpensiveMonsterCostInDeck
    {
      get
      {
        int a = 0;
        for (int i = 0; i < category.cards.Length; ++i)
        {
          DirectorCard directorCard = category.GetChoice(i);
          int b = directorCard.Cost;
          a = Mathf.Max(a, b);
        }
        return a;
      }
    }
    
    private void Awake()
    {
      _player = FindObjectOfType<PlayerConfig>().transform;
      moneyWaves = new DirectorMoneyWave[moneyWaveIntervals.Length];
      for (int index = 0; index < moneyWaveIntervals.Length; ++index)
        moneyWaves[index] = new DirectorMoneyWave
        {
          interval = Random.Range(moneyWaveIntervals[index].min, moneyWaveIntervals[index].max),
        };
    }
    
    private class DirectorMoneyWave
    {
      public float interval;
      public float timer;
      public float multiplier;
      private float accumulatedAward;

      public float Update(float deltaTime, float difficultyCoefficient)
      {
        timer += deltaTime;
        if (timer > interval)
        {
          float num = 0.5f;
          timer -= interval;
          accumulatedAward += interval * (1.0f + 0.4f * difficultyCoefficient) * num;
        }
        float num1 = Mathf.FloorToInt(accumulatedAward);
        accumulatedAward -= num1;
        return num1;
      }
    }
    
    private void PrepareNewMonsterWave(DirectorCard monsterCard)
    {
      currentMonsterCard = monsterCard;
      lastAttemptedMonsterCard = currentMonsterCard;
      spawnCountInCurrentWave = 0;
    }

    private bool AttemptSpawnOnTarget()
    {
      if (!currentMonsterCard)
      {
        PrepareNewMonsterWave(category.Evaluate(Random.Range(1, 10)));
      }
      
      int num1 = currentMonsterCard.Cost;
      
      if (spawnCountInCurrentWave >= maximumNumberToSpawnBeforeSkipping)
      {
        spawnCountInCurrentWave = 0;
        return false;
      }
      
      if (skipSpawnIfTooCheap && consecutiveCheapSkips < maxConsecutiveCheapSkips)
      {
        if (mostExpensiveMonsterCostInDeck > num1)
        {
          ++consecutiveCheapSkips;
        }
      }
      SpawnCard spawnCard = currentMonsterCard.spawnCard;

      if (num1 > monsterCredit)
      {
        return false;
      }
      
      Vector3 spawnTarget1 = TakeRandomPositionToSpawn();
      
      if (!Spawn(spawnCard, spawnTarget1))
        return false;
      monsterCredit -= num1;
      totalCreditsSpent += num1;
      ++spawnCountInCurrentWave;
      consecutiveCheapSkips = 0;
      return true;
    }

    public bool Spawn(SpawnCard spawnCard, Vector3 spawnTarget)
    {
      if (DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard), spawnTarget))
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
        if (component3)
        {
          float b = spawnCard.DirectorCreditCost * expRewardCoefficient;
          component3.spawnValue = (int) Mathf.Max(1f, b);
          if (b > Mathf.Epsilon)
          {
            component3.expReward = Mathf.Max(1f, b * GameManager.instance.GameDifficulty);
            component3.goldReward = Mathf.Max(1f, b * goldRewardCoefficient * 2.0f * GameManager.instance.GameDifficulty);
          }
          else
          {
            component3.expReward = 0;
            component3.goldReward = 0;
          }
        }
      }
    
    private void FixedUpdate()
    {
      float difficultyCoefficient = GameManager.instance.GameDifficulty;
      for (int index = 0; index < moneyWaves.Length; ++index)
        monsterCredit += moneyWaves[index].Update(Time.fixedDeltaTime, difficultyCoefficient);
      Simulate(Time.fixedDeltaTime);
    }
    
    private void Simulate(float deltaTime)
    {
      monsterSpawnTimer -= deltaTime;
      if (monsterSpawnTimer > 0.0)
        return;
      if (AttemptSpawnOnTarget())
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

    private Vector3 TakeRandomPositionToSpawn()
    {
      Vector3 targetPos = _player.position;
      Vector3 randomPos = targetPos + Random.insideUnitSphere * radius;
      if (randomPos.y < targetPos.y)
      {
        return TakeRandomPositionToSpawn();
      }

      return randomPos;
    }
}
