using GameProcess.Cards;
using GameProcess.Directors.Functions;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameProcess.Directors
{
  public class CombatDirector : MonoBehaviour
  {
    [SerializeField] private float radius;
    [SerializeField] private WeightSelection category;
    [SerializeField] private float monsterCredit;
    [SerializeField] private RangeFloat[] moneyWaveIntervals;
    
    private const float MinSeriesSpawnInterval = 0.1f;
    private const float MaxSeriesSpawnInterval = 1f;
    private const float MinRerollSpawnInterval = 22.5f;
    private const float MaxRerollSpawnInterval = 30f;
    private const int MaxConsecutiveCheapSkips = int.MaxValue;
    private const int MaximumNumberToSpawnBeforeSkipping = 6;
    
    private bool _shouldSpawnOneWave;
    private bool _hasStartedWave;
    private Transform _player;
    private DirectorCard _currentMonsterCard;
    private int _consecutiveCheapSkips;
    private int _spawnCountInCurrentWave;
    private DirectorMoneyWave[] _moneyWaves;

    private float monsterSpawnTimer { get; set; }

    public DirectorCard lastAttemptedMonsterCard { get; set; }
    
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
      _moneyWaves = new DirectorMoneyWave[moneyWaveIntervals.Length];
      for (int index = 0; index < moneyWaveIntervals.Length; ++index)
        _moneyWaves[index] = new DirectorMoneyWave
        {
          Interval = Random.Range(moneyWaveIntervals[index].Min, moneyWaveIntervals[index].Max),
        };
    }
    
    private void PrepareNewMonsterWave(DirectorCard monsterCard)
    {
      _currentMonsterCard = monsterCard;
      lastAttemptedMonsterCard = _currentMonsterCard;
      _spawnCountInCurrentWave = 0;
    }

    private bool AttemptSpawnOnTarget()
    {
      if (!_currentMonsterCard)
      {
        PrepareNewMonsterWave(category.Evaluate(Random.Range(1, 10)));
      }
      
      int num1 = _currentMonsterCard.Cost;
      
      if (_spawnCountInCurrentWave >= MaximumNumberToSpawnBeforeSkipping)
      {
        _spawnCountInCurrentWave = 0;
        return false;
      }
      
      if (_consecutiveCheapSkips < MaxConsecutiveCheapSkips)
      {
        if (mostExpensiveMonsterCostInDeck > num1)
        {
          ++_consecutiveCheapSkips;
        }
      }
      SpawnCard spawnCard = _currentMonsterCard.spawnCard;

      if (num1 > monsterCredit)
      {
        return false;
      }
      
      Vector3 spawnTarget1 = TakeRandomPositionToSpawn();
      
      if (!Spawn(spawnCard, spawnTarget1))
        return false;
      monsterCredit -= num1;
      ++_spawnCountInCurrentWave;
      _consecutiveCheapSkips = 0;
      return true;
    }

    public bool Spawn(SpawnCard spawnCard, Vector3 spawnTarget)
    {
      if (DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard), spawnTarget))
      {
        return true;
      }
      return false;
    }
    
    private void FixedUpdate()
    {
      float difficultyCoefficient = GameManager.Instance.GameDifficulty;
      for (int index = 0; index < _moneyWaves.Length; ++index)
        monsterCredit += _moneyWaves[index].Update(Time.fixedDeltaTime, difficultyCoefficient);
      Simulate(Time.deltaTime);
    }
    
    private void Simulate(float deltaTime)
    {
      monsterSpawnTimer -= deltaTime;
      if (monsterSpawnTimer > 0.0)
        return;
      if (AttemptSpawnOnTarget())
      {
        if (_shouldSpawnOneWave) _hasStartedWave = true;
        monsterSpawnTimer += Random.Range(MinSeriesSpawnInterval, MaxSeriesSpawnInterval);
      }
      else
      {
        monsterSpawnTimer += Random.Range(MinRerollSpawnInterval, MaxRerollSpawnInterval);
        _currentMonsterCard = null;
        if (!_shouldSpawnOneWave || !_hasStartedWave)
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

    public void ResetGame()
    {
      monsterCredit = 0;
      _consecutiveCheapSkips = 0;
      _spawnCountInCurrentWave = 0;
      _hasStartedWave = false;
    }
    
    private class DirectorMoneyWave
    {
      public float Interval;
      private float _timer;
      private float _accumulatedAward;

      public float Update(float deltaTime, float difficultyCoefficient)
      {
        _timer += deltaTime;
        if (_timer > Interval)
        {
          float num = 0.5f;
          _timer -= Interval;
          _accumulatedAward += Interval * (1.0f + 0.4f * difficultyCoefficient) * num;
        }
        float num1 = Mathf.FloorToInt(_accumulatedAward);
        _accumulatedAward -= num1;
        return num1;
      }
    }
  }
}
