using System;
using GameProcess.Cards;
using UnityEngine;

namespace GameProcess.Directors
{
    [Serializable]
    public class DirectorCard
    {
        public SpawnCard spawnCard;
        public int SelectionWeight;
        public DirectorCore.MonsterSpawnDistance SpawnDistance;

        public int Cost => spawnCard.DirectorCreditCost;
    }
}
