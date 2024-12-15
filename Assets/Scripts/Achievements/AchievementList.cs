using System.Collections.Generic;
using UnityEngine;

namespace Achievements
{
    public class AchievementList : MonoBehaviour
    {
        public List<Achievement> Achievements;

        public void CheckForCompletion()
        {
            foreach (var achieve in Achievements)
            {
                if (achieve.Condition)
                {
                    //
                }
            }
        }
    }
}
