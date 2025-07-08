using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateModel : MonoBehaviour
{
    private GameObject parentObj;

    void Start()
    {
        parentObj = transform.parent?.gameObject;
    }

    void Update()
    {
        if (parentObj != null)
        {
            // Step 1: Get your original local rotation
            Vector3 originalLocalEuler = transform.localEulerAngles;

            // Step 2: Build a quaternion with the desired Y rotation (e.g. fixed, or zeroed)
            float canceledY = -parentObj.transform.eulerAngles.y;

            // Step 3: Construct and assign new local rotation (safe, no accumulation)
            transform.localRotation = Quaternion.Euler(originalLocalEuler.x, canceledY, originalLocalEuler.z);

            Debug.Log("Corrected rotation: " + transform.rotation.eulerAngles);
        }
    }
}

