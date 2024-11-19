using GameProcess.Cards;
using UnityEngine;

namespace GameProcess.Directors.Functions
{
    public abstract class DirectorSpawnRequest
    {
        public SpawnCard SpawnCard;
        public DirectorPlacementRule PlacementRule;
        public GameObject SummonerBodyObject;

        public DirectorSpawnRequest(SpawnCard spawnCard, DirectorPlacementRule placementRule)
        {
            SpawnCard = spawnCard;
            this.PlacementRule = placementRule;
        }
    }
}
