using System;
using Saving;
using TMPro;
using UnityEngine;

namespace Achievements
{
    public class TextImporter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI maxLevel;
        [SerializeField] private TextMeshProUGUI killedMobs;
        [SerializeField] private TextMeshProUGUI gunBought;

        private void Update()
        {
            maxLevel.text = $"Достичь десятого уровня {SaveSystem.Instance.MaxLevel}/10";
            killedMobs.text = $"Убить 150 насекомых {SaveSystem.Instance.KilledMobs}/150";
            gunBought.text = "Купить в магазине бластер";
        }
    }
}
