using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class BounceBallTriggers : MonoBehaviour
{
    [SerializeField] private PlaneClassification targetPlaneClassification;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent(out ARPlane arPlane) && (arPlane.classification & targetPlaneClassification) != 0)
        {
            AudioManager.instance.PlaySFX("BounceSound");
        }
    }
}
