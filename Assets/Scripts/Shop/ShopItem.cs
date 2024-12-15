using System;
using Saving;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Shop
{
    public class ShopItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI itemText;

        [SerializeField] private ShopItemType itemType;
        
        [SerializeField] private GameObject soldImage;
        
        [SerializeField] private Button itemButton;

        public int ItemCost;

        public bool CanAfford;

        public bool IsSold;

        private void Start()
        {
            itemText.text = ItemCost.ToString();
        }

        private void Update()
        {
            itemButton.enabled = CanAfford;
        }

        public void ItemSold()
        {
            GetComponent<Button>().enabled = false;
            soldImage.SetActive(true);
            IsSold = true;
            SaveSystem.Instance.Credits -= ItemCost;
            switch (itemType)
            {
                case ShopItemType.Blaster:
                    SaveSystem.Instance.IsGunBought = true;
                    break;
                case ShopItemType.MagicalOrb:
                    SaveSystem.Instance.IsMagicalOrbBought = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            SaveSystem.Instance.Save();
        }
    }

    public enum ShopItemType
    {
        MagicalOrb,
        Blaster
    }
}
