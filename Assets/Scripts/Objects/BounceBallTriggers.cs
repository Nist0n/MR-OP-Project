using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class BounceBallTriggers : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        AudioManager.instance.PlaySFX("BounceSound");
    }
}
