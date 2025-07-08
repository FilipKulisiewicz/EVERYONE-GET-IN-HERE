using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateModelManager : MonoBehaviour
{
    void Update()
    {
        // Find all objects with the "Card" tag (or use a specific component if needed)
        GameObject[] cardObjects = GameObject.FindGameObjectsWithTag("Card");

        foreach (var card in cardObjects)
        {
            if (card.transform.parent == null) continue;

            Transform cardTransform = card.transform;
            Transform parentTransform = cardTransform.parent;

            // Step 1: Get original local rotation
            Vector3 originalLocalEuler = cardTransform.localEulerAngles;

            // Step 2: Cancel out parent's Y rotation
            float canceledY = -parentTransform.eulerAngles.y;

            // Step 3: Apply corrected rotation
            cardTransform.localRotation = Quaternion.Euler(originalLocalEuler.x, canceledY, originalLocalEuler.z);
        }
    }
}

