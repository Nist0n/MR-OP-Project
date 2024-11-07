using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;

public class SpawnCubes : MonoBehaviour
{
    [SerializeField] private InputActionReference spawnAction;
    [SerializeField] private InputActionReference switchAction;
    [SerializeField] private XRRayInteractor xrRayInteractor;
    [SerializeField] private PlaneClassification targetPlaneClassification;
    [SerializeField] private GameObject objectPrefab;
    
    private void OnEnable()
    {
        spawnAction.action.Enable();
        spawnAction.action.performed += OnSpawn;
    }

    private void OnDisable()
    {
        spawnAction.action.Disable();
        spawnAction.action.performed -= OnSpawn;
    }
    
    private void OnSpawn(InputAction.CallbackContext context)
    {
        if (xrRayInteractor.enabled && xrRayInteractor.TryGetCurrent3DRaycastHit(out var raycastHit, out _))
        {
            if (raycastHit.transform.TryGetComponent(out ARPlane arPlane) && (arPlane.classification & targetPlaneClassification) != 0)
            {
                var hitPose = new Pose(raycastHit.point, Quaternion.LookRotation(raycastHit.normal));

                var instantiate = Instantiate(objectPrefab, hitPose.position, hitPose.rotation);
                instantiate.name = "SpawnedObject";

                instantiate.GetComponent<Renderer>().material.color = Random.ColorHSV();
                instantiate.AddComponent<ARAnchor>();

                return;
            }

            if (raycastHit.transform.name == "SpawnedObject")
            {
                Destroy(raycastHit.transform.gameObject);

                return;
            }
        }
    }
}
