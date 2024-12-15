using System.Collections.Generic;
using Achievements;
using UnityEngine;

namespace Saving
{
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance;

        private const string Key = "mainSave";
        
        public List<Achievement> Achievements;
        
        public int KilledMobs;

        public int MaxGottenLevel;

        public bool IsGunBought;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Save()
        {
            DataBase.Save(Key, GetSaveSnapshot());
        }
        
        public void SaveFirstTime()
        {
            DataBase.Save(Key, GetSaveSnapshotFirstTime());
        }

        private GameData GetSaveSnapshot()
        {
            var data = new GameData
            {
                Achievements = this.Achievements,
                KilledMobs = this.KilledMobs,
                IsGunBought = this.IsGunBought,
                MaxGottenLevel = this.MaxGottenLevel
            };

            return data;
        }
        
        private GameData GetSaveSnapshotFirstTime()
        {
            PlayerPrefs.SetInt("FirstTime", 1);
            var data = new GameData
            {
                Achievements = this.Achievements,
                KilledMobs = this.KilledMobs,
                IsGunBought = this.IsGunBought,
                MaxGottenLevel = this.MaxGottenLevel
            };

            return data;
        }
    
        public void Load()
        {
            var data = DataBase.Load<GameData>(Key);

            if (data == null) return;
            
            Debug.Log(data.KilledMobs);
            
            Achievements = data.Achievements;
            KilledMobs = data.KilledMobs;
            MaxGottenLevel = data.MaxGottenLevel;
            IsGunBought = data.IsGunBought;
        }
    }
}
