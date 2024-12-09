using System.Collections.Generic;
using GameProcess.Cards;
using GameProcess.Directors.Functions;
using UnityEngine;

namespace GameProcess.Directors
{
  public class DirectorCore : MonoBehaviour
  {
    public static DirectorCore instance { get; private set; }

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
      
      spawnCardObject = spawnCard.DoSpawn(spawnTarget, quaternion, directorSpawnRequest).SpawnedInstance;
          
      return spawnCardObject;
    }
  }
}
