using GameProcess.Cards;
using UnityEngine;

namespace GameProcess.Directors.Functions
{
    public abstract class DirectorSpawnRequest
    {
        public SpawnCard SpawnCard;
        public GameObject SummonerBodyObject;

        public DirectorSpawnRequest(SpawnCard spawnCard)
        {
            SpawnCard = spawnCard;
        }
    }
}
