using System.Collections.Generic;
using GameProcess.Cards;
using GameProcess.Directors.Functions;
using UnityEngine;

namespace GameProcess.Directors
{
  public class DirectorCore : MonoBehaviour
  {
    public static List<GameObject> SpawnedObjects = new List<GameObject>();

    public static DirectorCore instance { get; private set; }

    private void OnEnable()
    {
      if (!instance)
        instance = this;
      else
        Debug.Log("Error");
    }

    private void OnDisable()
    {
      if (!(instance == this))
        return;
      instance = null;
    }

    public GameObject TrySpawnObject(DirectorSpawnRequest directorSpawnRequest)
    {
      SpawnCard spawnCard = directorSpawnRequest.SpawnCard;
      
      GameObject gameObject = null;
      
      Quaternion quaternion = Quaternion.Euler(0f, 0f, 0f);
      
      gameObject = spawnCard.DoSpawn(this.gameObject.transform.position, quaternion, directorSpawnRequest).SpawnedInstance;
          
      return gameObject;
    }

    public static void GetMonsterSpawnDistance(MonsterSpawnDistance input, out float minimumDistance, out float maximumDistance)
    {
      minimumDistance = 0f;
      maximumDistance = 0f;
      switch (input)
      {
        case MonsterSpawnDistance.Standard:
          minimumDistance = 25f;
          maximumDistance = 40f;
          break;
        case MonsterSpawnDistance.Close:
          minimumDistance = 8f;
          maximumDistance = 20f;
          break;
        case MonsterSpawnDistance.Far:
          minimumDistance = 70f;
          maximumDistance = 120f;
          break;
      }
    }

    public enum MonsterSpawnDistance
    {
      Standard,
      Close,
      Far,
    }
  }
}
