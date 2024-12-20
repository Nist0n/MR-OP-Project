using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class Slot : MonoBehaviour
    {
        public GameObject ItemInSlot;

        public Image SlotImage;

        private readonly Color _originalColor = Color.white;
    
        [SerializeField] private InputActionProperty putItem;
        
        private void Start()
        {
            SlotImage = GetComponentInChildren<Image>();
        }

        private void OnTriggerStay(Collider other)
        {
            if (ItemInSlot != null) return;

            if (other.GetComponent<Item>().CurrentSlot != null) return;
            
            Debug.Log(other.name);

            GameObject obj = other.gameObject;

            if (!IsItem(obj))
            {
                return;
            }
            
            SlotImage.color = Color.green;
            
            if (!putItem.action.IsPressed() && obj.GetComponent<Item>().IsHanded)
            {
                InsertItem(obj);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            ResetColor();
        }

        private bool IsItem(GameObject obj)
        {
            if (obj.GetComponent<Item>())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void InsertItem(GameObject obj)
        {
            obj.transform.localScale /= 2;
            obj.GetComponent<Rigidbody>().isKinematic = true;
            obj.transform.SetParent(gameObject.transform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localEulerAngles = obj.GetComponent<Item>().SlotRotation;
            obj.GetComponent<Item>().InSlot = true;
            obj.GetComponent<Item>().CurrentSlot = this;
            ItemInSlot = obj;
            SlotImage.color = Color.gray;
        }

        public void ResetColor()
        {
            SlotImage.color = _originalColor;
        }
    }
}
