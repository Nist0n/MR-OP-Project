using System;
using GameProcess.Directors.Functions;
using UnityEngine;

namespace GameProcess.Cards
{
    [CreateAssetMenu]
    public class SpawnCard : ScriptableObject
    {
        public int DirectorCreditCost;

        public GameObject Prefab;

        private void Spawn(Vector3 position, Quaternion rotation, DirectorSpawnRequest spawnRequest, ref SpawnResult spawnResult)
        {
            GameObject gameObject = Instantiate(Prefab, position, rotation);
            gameObject.name = "Mosquito";
            spawnResult.SpawnedInstance = gameObject;
            spawnResult.Success = true;
            GameManager.Instance.Enemies.Add(spawnResult.SpawnedInstance);
        }
        
        public SpawnResult DoSpawn(Vector3 position, Quaternion rotation, DirectorSpawnRequest spawnRequest)
        {
            SpawnResult spawnResult = new SpawnResult()
            {
                SpawnRequest = spawnRequest,
                Position = position,
                Rotation = rotation
            };
            Spawn(position, rotation, spawnRequest, ref spawnResult);
            return spawnResult;
        }
        
        public struct SpawnResult
        {
            public GameObject SpawnedInstance;
            public DirectorSpawnRequest SpawnRequest;
            public Vector3 Position;
            public Quaternion Rotation;
            public bool Success;
        }
    }
}
