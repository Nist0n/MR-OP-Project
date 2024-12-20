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
            ui.transform.position = FindObjectOfType<PlayerConfig>().UIPos.transform.position;
            ui.transform.LookAt(FindObjectOfType<PlayerConfig>().transform);
            ui.transform.eulerAngles -= new Vector3(0, 180, 0);
        }
    }
}
