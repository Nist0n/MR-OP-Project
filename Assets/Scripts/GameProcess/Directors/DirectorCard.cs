using System;
using GameProcess.Cards;
using UnityEngine;

namespace GameProcess.Directors
{
    [CreateAssetMenu]
    public class DirectorCard : ScriptableObject
    {
        public SpawnCard spawnCard;
        
        public int SelectionWeight;

        public int Cost => spawnCard.DirectorCreditCost;
    }
}
