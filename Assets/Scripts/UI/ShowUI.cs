using System;
using Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class ShowUI : MonoBehaviour
    {
        private static Vector3 _posUI;
        
        public static void CreateUI(GameObject ui)
        {
            ui.SetActive(true);
            ui.transform.position = _posUI;
        }

        private void Awake()
        {
            _posUI = FindObjectOfType<PlayerConfig>().UIPos.transform.position;
        }
    }
}
