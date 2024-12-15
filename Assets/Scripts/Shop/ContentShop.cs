using System;
using System.Collections.Generic;
using Saving;
using UnityEngine;

namespace Shop
{
    public class ContentShop : MonoBehaviour
    {
        [SerializeField] private List<ShopItem> items;

        private void Update()
        {
            CheckForAvailability();
        }

        private void CheckForAvailability()
        {
            foreach (var item in items)
            {
                if (item.IsSold)
                {
                    return;
                }
                if (item.ItemCost <= SaveSystem.Instance.Credits)
                {
                    item.CanAfford = true;
                }
                else
                {
                    item.CanAfford = false;
                }
            }
        }

        public void BuyItem(ShopItem item)
        {
            if (item.ItemCost <= SaveSystem.Instance.Credits)
            {
                item.ItemSold();
            }
        }
    }
}
