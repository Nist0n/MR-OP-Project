using System;
using System.Collections.Generic;
using Saving;
using UnityEngine;
using EventHandler = GameProcess.EventHandler;

namespace Achievements
{
    public class AchievementConditions : MonoBehaviour
    {
        public List<Achievement> Achievements;

        [SerializeField] private List<GameObject> achievementButtons;
        
        [SerializeField] private SaveSystem saveSystem;

        private void Awake()
        {
            if (PlayerPrefs.GetInt("FirstTime") != 1)
            {
                for (int i = 0; i < Achievements.Count; i++)
                {
                    saveSystem.Achievements.Add(Achievements[i]);
                }

                saveSystem.SaveFirstTime();
                saveSystem.Load();
            }
        }

        private void Start()
        {
            EventHandler.EnemyDied += OnEnemyDied;
            EventHandler.PlayerLevelUp += OnPlayerLevelUp;
            EventHandler.GunBuy += OnGunBuy;
            CheckForCompletion();
        }

        private void OnEnemyDied(float num1, float num2)
        {
            SaveSystem.Instance.KilledMobs++;
            if (SaveSystem.Instance.KilledMobs >= 1)
            {
                SaveSystem.Instance.Achievements.Find(x => x.name.Contains("EnemyKilled")).CompleteAchievement();
            }
            SaveSystem.Instance.Save();
            CheckForCompletion();
        }

        private void OnPlayerLevelUp(int level)
        {
            SaveSystem.Instance.MaxLevel = Mathf.Max(level, SaveSystem.Instance.MaxLevel);
            if (SaveSystem.Instance.MaxLevel >= 10)
            {
                SaveSystem.Instance.Achievements.Find(x => x.name.Contains("MaxLevel")).CompleteAchievement();
            }
            SaveSystem.Instance.Save();
            CheckForCompletion();
        }
        
        private void OnGunBuy()
        {
            SaveSystem.Instance.IsGunBought = true;
            SaveSystem.Instance.Achievements.Find(x => x.name.Contains("GunBought")).CompleteAchievement();
            SaveSystem.Instance.Save();
            CheckForCompletion();
        }

        private void CheckForCompletion()
        {
            foreach (var achieve in SaveSystem.Instance.Achievements)
            {
                if (achieve.Condition)
                {
                    achievementButtons.Find(x => x.name == achieve.name).SetActive(true);
                }
            }
        }
    }
}
