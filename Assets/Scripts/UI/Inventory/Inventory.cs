using System.Collections.Generic;
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
    
        private bool _uiActive;

        private void Update()
        {
            if (inventoryButton.action.triggered)
            {
                _uiActive = !_uiActive;
                inventoryVR.SetActive(_uiActive);
            }
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
