using System;
using GameProcess.Cards;
using UnityEngine;

namespace GameProcess.Directors.Functions
{
    public class DirectorSpawnRequest
    {
        public SpawnCard spawnCard;

        public DirectorSpawnRequest(SpawnCard spawnCard)
        {
            this.spawnCard = spawnCard;
        }
    }
}
