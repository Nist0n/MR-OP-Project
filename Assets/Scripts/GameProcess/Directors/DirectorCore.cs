using System.Collections.Generic;
using GameProcess.Cards;
using GameProcess.Directors.Functions;
using UnityEngine;

namespace GameProcess.Directors
{
  public class DirectorCore : MonoBehaviour
  {
    public static DirectorCore instance { get; private set; }
    
    private const float ExpRewardCoefficient = 0.2f;
    private const float GoldRewardCoefficient = 1f;

    private void OnEnable()
    {
      if (!instance) instance = this;
    }

    private void OnDisable()
    {
      if (!(instance == this)) return;
      instance = null;
    }

    public GameObject TrySpawnObject(DirectorSpawnRequest directorSpawnRequest, Vector3 spawnTarget)
    {
      SpawnCard spawnCard = directorSpawnRequest.spawnCard;
      
      GameObject spawnCardObject = null;
      
      Quaternion quaternion = Quaternion.Euler(0f, 0f, 0f);
      
      SpawnCard.SpawnResult result = spawnCard.DoSpawn(spawnTarget, quaternion, directorSpawnRequest);

      spawnCardObject = result.SpawnedInstance;

      if (spawnCardObject)
      {
        OnCardSpawned(result, spawnCard);
      }
          
      return spawnCardObject;
    }
    
    private void OnCardSpawned(SpawnCard.SpawnResult result, SpawnCard spawn)
    {
      SpawnCard spawnCard = spawn;
      GameObject bodyObject = result.SpawnedInstance;
      DeathRewards component3 = bodyObject.GetComponentInChildren<DeathRewards>();
      if (component3)
      {
        float b = spawnCard.DirectorCreditCost * ExpRewardCoefficient;
        component3.spawnValue = (int) Mathf.Max(1f, b);
        if (b > Mathf.Epsilon)
        {
          component3.expReward = Mathf.Max(1f, b * GameManager.Instance.GameDifficulty);
          component3.goldReward = Mathf.Max(1f, b * GoldRewardCoefficient * 2.0f * GameManager.Instance.GameDifficulty);
        }
        else
        {
          component3.expReward = 0;
          component3.goldReward = 0;
        }
      }
    }
  }
}
