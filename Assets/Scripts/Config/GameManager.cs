using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        AudioManager.instance.PlayMusic("BG_Music");
    }
}
