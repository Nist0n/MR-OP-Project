using System;
using UnityEngine;

namespace GameProcess
{
    public class EventHandler : MonoBehaviour
    {
        public static Action<float, float> EnemyDied;
        
        public static Action GameLost;

        public static void OnEnemyDied(float gold, float exp)
        {
            EnemyDied?.Invoke(gold, exp);
        }
        
        public static void OnGameLost()
        {
            GameLost?.Invoke();
        }
    }
}
