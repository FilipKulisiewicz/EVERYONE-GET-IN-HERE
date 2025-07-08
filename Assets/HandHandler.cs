using System;
using UnityEngine;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.HandLandmarker;

public class HandHandler : MonoBehaviour
{
    public event Action<GameObject, GameObject> TwoObjectInteraction;

    public enum HandSelection
    {
        Left,
        Right
    }

    public enum FingerType { Thumb, Index, Middle, Ring, Pinky }

    public List<FingerType> fingersToDisplay = new List<FingerType>
    {
        FingerType.Thumb,
        FingerType.Index,
        FingerType.Middle,
        FingerType.Ring,
        FingerType.Pinky
    };

    private Dictionary<FingerType, int[]> fingerConnections = new Dictionary<FingerType, int[]>
    {
        { FingerType.Thumb,  new[] {1,2,3,4} },
        { FingerType.Index,  new[] {5,6,7,8} },
        { FingerType.Middle, new[] {9,10,11,12} },
        { FingerType.Ring,   new[] {13,14,15,16} },
        { FingerType.Pinky,  new[] {17,18,19,20} }
    };

    public GameObject jointPrefab;
    public Material lineMaterial;
    public HandSelection handToUse = HandSelection.Right; // exposed in Inspector
    public float depth = 0.3f;

    private List<GameObject> jointObjects = new List<GameObject>();
    private List<LineRenderer> bones = new List<LineRenderer>();

    private List<NormalizedLandmark> landmarks = null;
    private List<Landmark> landmarks3D = null;
    private List<NormalizedLandmark> latestLandmarks = null;
    private List<Landmark> latestLandmarks3D = null;
    private readonly object _lock = new object();
    private Camera arCamera;

    private float noHandTimer = 0f;
    public float handTimeoutSeconds = 0.5f; // adjustable
    private bool handVisible = false;

    private bool isPinched = false;
    private bool isPinchedPrevState = false;
    public float pinchDistanceEnterThreshold = 0.035f;  // Enter pinch when closer than this
    public float pinchDistanceExitThreshold = 0.050f;   // Exit pinch when farther than this

    private GameObject pinchedObject = null, potentialUnpinchedObject = null;
    private readonly string[] validTags = {"Card", "Hero", "Special"};

    private void Awake()
    {
        arCamera = Camera.main;
        transform.SetParent(arCamera.transform);
        transform.localPosition = new Vector3(0, 0, 10000.0f); // Keep offscreen
        transform.localRotation = Quaternion.identity;
    }

    void Start()
    {
        for (int i = 0; i < 21; i++)
        {
            var joint = Instantiate(jointPrefab, transform);
            joint.name = $"Joint_{i}";
            jointObjects.Add(joint);
        }

        for (int i = 0; i < 20; i++)
        {
            var go = new GameObject($"Bone_{i}");
            go.transform.SetParent(transform);
            var line = go.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.material = lineMaterial;
            line.widthMultiplier = 0.005f;
            bones.Add(line);
        }
        HideHand();
    }


    public void ScheduleHandUpdate(HandLandmarkerResult result)
    {
        if (result.Equals(default) || result.handLandmarks == null || result.handLandmarks.Count == 0)
        {
            landmarks = null;
            landmarks3D = null;
            lock (_lock)
            {
                latestLandmarks = null;
                latestLandmarks3D = null;
            }
            return;
        }

        // Try to find the hand index based on handedness string
        string targetHand = handToUse.ToString(); // "Right" or "Left"
        int selectedIndex = -1;

        for (int i = 0; i < result.handedness.Count; i++)
        {
            var categories = result.handedness[i].categories;
            if (categories != null && categories.Count > 0)
            {
                string categoryName = categories[0].categoryName;
                if (categoryName.Equals(targetHand, StringComparison.OrdinalIgnoreCase))
                {
                    selectedIndex = i;
                    break;
                }
            }
        }

        if (selectedIndex == -1)
        {
            // Selected hand not found
            landmarks = null;
            landmarks3D = null;
            lock (_lock)
            {
                latestLandmarks = null;
                latestLandmarks3D = null;
            }
            return;
        }

        // Use landmarks for the matched hand
        landmarks = result.handLandmarks[selectedIndex].landmarks;
        landmarks3D = result.handWorldLandmarks[selectedIndex].landmarks;

        lock (_lock)
        {
            latestLandmarks = new List<NormalizedLandmark>(landmarks);
            latestLandmarks3D = new List<Landmark>(landmarks3D);
        }
    }

    void Update()
    {
        List<NormalizedLandmark> landmarksToUpdate = null;
        List<Landmark> landmarks3DToUpdate = null;

        lock (_lock)
        {
            if (latestLandmarks != null)
            {
                landmarksToUpdate = latestLandmarks;
                latestLandmarks = null;
                landmarks3DToUpdate = latestLandmarks3D;
                latestLandmarks3D = null;
            }
        }

        if (landmarksToUpdate != null && landmarksToUpdate.Count >= 21)
        {
            noHandTimer = 0f; // reset timer
            handVisible = true;

            DrawHand(landmarksToUpdate);
            HandlePinchingSelection(landmarksToUpdate, landmarks3DToUpdate);  
        }
        else
        {
            noHandTimer += Time.deltaTime;

            if (handVisible && noHandTimer >= handTimeoutSeconds)
            {
                HideHand();
                handVisible = false;
            }
        }
    }

    private void HandlePinchingSelection(List<NormalizedLandmark> landmarks2D, List<Landmark> landmarks3D)
    {
        var isPinchedCurrState = CheckIfPinched(landmarks3D);
        if(isPinchedPrevState != isPinchedCurrState);
        {
            if(isPinchedCurrState == true)
            {
                var obj = CheckIfTouchingCard(landmarks2D);
                if (obj)
                {
                    if (pinchedObject == null) //set as pinched obj
                    {
                        if(HasAnyOfTags(obj, validTags)){
                            pinchedObject = obj;
                            CardVisualHelper.SetGlow(pinchedObject, true, "yellow");    
                            Debug.Log("pinchedObject: " + pinchedObject.name);
                        }
                    }
                }
            }
            else if (isPinchedCurrState == false) //unpinched
            {
                var unpinchedObject = CheckIfTouchingCard(landmarks2D); //check for unpinched obj

                if (unpinchedObject)
                {
                    if (pinchedObject != null)                     
                    {
                        Debug.Log("PinchedObject: " + GetCardSpriteName(pinchedObject) + " interacts with  Un-pinchedObject: " +  GetCardSpriteName(unpinchedObject));
                        TwoObjectInteraction?.Invoke(pinchedObject, unpinchedObject);
                    }
                }
                //clear pinched obj
                if (pinchedObject != null)
                {
                    CardVisualHelper.SetGlow(pinchedObject, false);
                    pinchedObject = null;
                }
                if (unpinchedObject != null)
                {
                    CardVisualHelper.SetGlow(unpinchedObject, false);
                    unpinchedObject = null;
                }
                if (potentialUnpinchedObject != null)
                {
                    CardVisualHelper.SetGlow(potentialUnpinchedObject, false);
                    potentialUnpinchedObject = null;
                }
            }
        }
        //handled piched marking
        if(isPinchedCurrState  == true){
            var obj = CheckIfTouchingCard(landmarks2D);
            if(obj != null && obj != pinchedObject && obj != potentialUnpinchedObject)
            {
                if(potentialUnpinchedObject != null){
                    CardVisualHelper.SetGlow(potentialUnpinchedObject, false); //prev potentialUnpinchedObject
                }
                potentialUnpinchedObject = obj;
                if(HasAnyOfTags(potentialUnpinchedObject, validTags)){ 
                    CardVisualHelper.SetGlow(potentialUnpinchedObject, true, "purple");    
                }
            }
            else if(obj == null){
                if(potentialUnpinchedObject != null){
                    CardVisualHelper.SetGlow(potentialUnpinchedObject, false);
                    potentialUnpinchedObject = null;
                }
            }
        }
    }

    private void HideHand()
    {
        foreach (var joint in jointObjects)
        {
            if (joint != null) joint.SetActive(false);
        }

        foreach (var bone in bones)
        {
            if (bone != null) bone.gameObject.SetActive(false);
        }
    }

    public void DrawHand(List<NormalizedLandmark> landmarks)
    {
        if (landmarks == null || landmarks.Count < 21) return;

        for (int i = 0; i < landmarks.Count; i++)
        {
            NormalizedLandmark lm = landmarks[i];

            float screenX = lm.x * Screen.width;
            float screenY = (1f - lm.y) * Screen.height;
            float z = depth;
            if (lm.z < 0) z += Mathf.Abs(lm.z) * 0.2f;

            Vector3 screenPoint = new Vector3(screenX, screenY, z);
            Vector3 worldPoint = arCamera.ScreenToWorldPoint(screenPoint);

            jointObjects[i].transform.position = Vector3.Lerp(jointObjects[i].transform.position, worldPoint, 0.5f);
            jointObjects[i].SetActive(false); // Hide by default
        }

        int boneIndex = 0;

        // Hide all joints and bones initially
        foreach (var joint in jointObjects)
            joint.SetActive(false);
        foreach (var bone in bones)
            bone.gameObject.SetActive(false);

        // Only draw selected fingers
        foreach (var finger in fingersToDisplay)
        {
            if (!fingerConnections.ContainsKey(finger))
                continue;

            var indices = fingerConnections[finger];

            for (int i = 0; i < indices.Length - 1; i++)
            {
                int start = indices[i];
                int end = indices[i + 1];

                jointObjects[start].SetActive(true);
                jointObjects[end].SetActive(true);

                var line = bones[boneIndex++];
                line.useWorldSpace = true;
                line.startColor = Color.red;
                line.endColor = Color.red;
                line.SetPosition(0, jointObjects[start].transform.position);
                line.SetPosition(1, jointObjects[end].transform.position);
                line.gameObject.SetActive(true);
            }
        }

        // Explicitly hide the palm root (index 0)
        jointObjects[0].SetActive(false);
    }

    public bool CheckIfPinched(List<Landmark> landmarks3D)
    {
        if (landmarks3D == null || landmarks3D.Count < 9)
            return isPinched;

        var thumbTip = landmarks3D[4];
        var indexTip = landmarks3D[8];

        float dx = indexTip.x - thumbTip.x;
        float dy = indexTip.y - thumbTip.y;
        float dz = indexTip.z - thumbTip.z;

        float distance = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);

        // Apply Schmitt Trigger logic
        if (!isPinched && distance < pinchDistanceEnterThreshold)
        {
            isPinched = true;
            // Debug.Log("Pinch Detected!");
        }
        else if (isPinched && distance > pinchDistanceExitThreshold)
        {
            isPinched = false;
            // Debug.Log("Pinch Released");
        }

        int[] highlightedJoints = {4, 8};
        Color pinchColor = isPinched ? Color.yellow : Color.red;

        foreach (int i in highlightedJoints)
        {
            var renderer = jointObjects[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = pinchColor;
            }
        }
        return isPinched;
    }

    private GameObject CheckIfTouchingCard(List<NormalizedLandmark> landmarks)
    {
        if (landmarks == null || landmarks.Count < 9) return null;

        // Get thumb tip and index tip (screen space)
        Vector3 thumbScreen = new Vector3(landmarks[4].x * Screen.width, (1f - landmarks[4].y) * Screen.height, depth);
        Vector3 indexScreen = new Vector3(landmarks[8].x * Screen.width, (1f - landmarks[8].y) * Screen.height, depth);

        // Midpoint in screen space
        Vector3 midScreen = (thumbScreen + indexScreen) / 2f;

        // Convert to world point
        Vector3 midWorld = arCamera.ScreenToWorldPoint(midScreen);

        // Direction from camera to mid point
        Vector3 dir = (midWorld - arCamera.transform.position).normalized;

        Ray ray = new Ray(arCamera.transform.position, dir);
        return RaycastForTaggedCards(ray, validTags);
    }

    public GameObject RaycastForTaggedCards(Ray ray, string[] validTags)
    {
        int cardLayer = LayerMask.NameToLayer("Card");
        int layerMask = 1 << cardLayer;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, layerMask))
        {
            if (HasAnyOfTags(hit.collider.gameObject,validTags))
            {
                return hit.collider.gameObject;
            }
        }
        return null;
    }

    private string GetCardSpriteName(GameObject obj){ // it should not be solved this way
        if (obj != null)
        {
            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                return spriteRenderer.sprite.name;
            }
        }
        return "err";
    } 

    private static bool HasAnyOfTags(GameObject obj, params string[] tags)
    {
        foreach (string tag in tags)
        {
            if (obj.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }
}
