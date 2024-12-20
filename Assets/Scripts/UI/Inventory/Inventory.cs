using System;
using System.Collections.Generic;
using Saving;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Inventory
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private GameObject inventoryVR;
    
        [SerializeField] private GameObject anchor;

        [SerializeField] private InputActionProperty inventoryButton;

        [SerializeField] private List<Slot> slots;
        
        [SerializeField] private GameObject racket;
        
        [SerializeField] private GameObject blaster;
    
        private bool _uiActive;

        private void Start()
        {
            if (SaveSystem.Instance.IsGunBought)
            {
                slots[1].InsertItem(blaster);
            }
            else
            {
                Destroy(blaster);
            }
        }

        private void Update()
        {
            if (inventoryButton.action.triggered)
            {
                _uiActive = !_uiActive;
                inventoryVR.SetActive(_uiActive);
            }
        }

        public void InsertITems()
        {
            slots[0].InsertItem(racket);
        }

        public Slot CheckEmpty()
        {
            foreach (var slot in slots)
            {
                if (slot.ItemInSlot == null)
                {
                    return slot;
                }
            }

            return null;
        }
    }
}
