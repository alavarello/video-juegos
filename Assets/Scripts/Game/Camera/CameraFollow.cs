﻿using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Transform target;            // The position that that camera will be following.
    public float smoothing = 2f;        // The speed with which the camera will be following.

    Vector3 offset;                     // The initial offset from the target.
    
    void FixedUpdate ()
    {
        if (target == null) return;

        if (offset == Vector3.zero)
        {       
            offset = transform.position - target.position;

        }

        // Create a postion the camera is aiming for based on the offset from the target.
        Vector3 targetCamPos = target.position + offset;

        // Smoothly interpolate between the camera's current position and it's target position.
        transform.position = Vector3.Lerp (transform.position, targetCamPos, smoothing * Time.deltaTime);
    }
}