using Saving;
using UnityEngine;

namespace Achievements
{
    [CreateAssetMenu]
    public class Achievement : ScriptableObject
    {
        public bool Condition;

        public void CompleteAchievement()
        {
            Condition = true;
        }
    }
}
