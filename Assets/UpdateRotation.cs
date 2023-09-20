using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateRotation : MonoBehaviour
{
    public Camera mainCamera;

private void Start()
{
    // Find the main camera in the scene
    mainCamera = Camera.main;
}

private void Update()
{
    // Check if the main camera is available
    if (mainCamera != null)
    {
            // Calculate the rotation needed to face the camera while keeping the Y rotation fixed
            Vector3 toCamera = mainCamera.transform.position - transform.position;
            toCamera.y = 0; // Keep the Y component fixed
            Quaternion lookRotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);

            // Apply the rotation to the cube
            transform.rotation = lookRotation;
        }
}
}
