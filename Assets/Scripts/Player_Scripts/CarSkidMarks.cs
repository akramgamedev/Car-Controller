using System;
using UnityEngine;
using UnityEngine.Rendering;

public class CarSkidMarks : MonoBehaviour
{
    [Header("Trail Color Settings")]
    public Color trailColor = Color.black;
    [Range(0f, 1f)] public float trailAlpha = 0.9f;

    [Header("Drift Trail Settings")]
    public TrailRenderer leftTrail;
    public TrailRenderer rightTrail;
    public float wheelDistance = 1.5f;
    public float rearWheelOffset = 1.2f;
    public float trailStartAngle = 12f;
    public float trailWidth = 0.3f;
    public float trailLifetime = 3f;

    [Header("Brake Mark Settings")]
    public float minBrakeSpeed = 12f;
    public float minHighSpeedDuration = 1.5f;

    private GameObject leftTrailObject;
    private GameObject rightTrailObject;
    private Transform carChild;

    

    void Start()
    {
        // Automatically find the active car with tag "Car"
        carChild = FindActiveCarChild();

        // Continue only if we found one
        if (carChild != null)
        {
            SetupTrailRenderers();
        }
        else
        {
            LogHelper.LogWarning("CarSkidMarks: No active child with 'Car' tag found under " + gameObject.name);
        }
    }

    // 🔍 Finds the first active child with the "Car" tag
    Transform FindActiveCarChild()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy && child.CompareTag("Car"))
                return child;
        }
        return null;
    }

    void SetupTrailRenderers()
    {
        // destroy existing trail objects (if any) to avoid reusing wrong parents
        if (leftTrailObject != null) Destroy(leftTrailObject);
        if (rightTrailObject != null) Destroy(rightTrailObject);

        leftTrailObject = new GameObject("LeftDriftTrail");
        rightTrailObject = new GameObject("RightDriftTrail");

        // parent to carChild (if available) so local pos/rot make sense
        if (carChild != null)
        {
            leftTrailObject.transform.SetParent(carChild, false);
            rightTrailObject.transform.SetParent(carChild, false);
        }
        else
        {
            leftTrailObject.transform.SetParent(transform, false);
            rightTrailObject.transform.SetParent(transform, false);
        }

        // reset transforms so local positions equal local wheel offsets
        leftTrailObject.transform.localPosition = Vector3.zero;
        leftTrailObject.transform.localRotation = Quaternion.identity;
        leftTrailObject.transform.localScale = Vector3.one;

        rightTrailObject.transform.localPosition = Vector3.zero;
        rightTrailObject.transform.localRotation = Quaternion.identity;
        rightTrailObject.transform.localScale = Vector3.one;

        leftTrail = leftTrailObject.AddComponent<TrailRenderer>();
        rightTrail = rightTrailObject.AddComponent<TrailRenderer>();

        ConfigureTrail(leftTrail);
        ConfigureTrail(rightTrail);

        leftTrail.emitting = false;
        rightTrail.emitting = false;
    }

    void ConfigureTrail(TrailRenderer trail)
    {
        trail.time = trailLifetime;
        trail.autodestruct = false;

        trail.startWidth = trailWidth;
        trail.endWidth = trailWidth;

        // color gradient
        Gradient gradient = new Gradient();
        gradient.mode = GradientMode.Blend;
        gradient.SetKeys(
            new GradientColorKey[] {
            new GradientColorKey(trailColor, 0f),
            new GradientColorKey(trailColor, 1f)
            },
            new GradientAlphaKey[] {
            new GradientAlphaKey(trailAlpha, 0.0f),
            new GradientAlphaKey(trailAlpha * 0.85f, 0.3f),
            new GradientAlphaKey(trailAlpha * 0.65f, 0.5f),
            new GradientAlphaKey(trailAlpha * 0.45f, 0.7f),
            new GradientAlphaKey(trailAlpha * 0.25f, 0.85f),
            new GradientAlphaKey(0f, 1.0f)
            }
        );
        trail.colorGradient = gradient;

        // simple width curve
        AnimationCurve widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
        trail.widthCurve = widthCurve;

        // material
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.material.color = trailColor;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;

        // IMPORTANT: keep the trail transform rotation neutral.
        // If you need the trail to face up, use the trail's localPosition/parent instead of rotating it.
        trail.alignment = LineAlignment.View; // safer: aligns with camera view; avoids transform Z issues
        trail.transform.localRotation = Quaternion.identity;
        trail.textureMode = LineTextureMode.Stretch;
        trail.minVertexDistance = 0.05f; // smaller so trails are continuous for small wheel motion
        trail.generateLightingData = false;
    }

    public void Initialize(Transform car)
    {
        // assign new car
        carChild = car;
        AutoDetectWheelsPosition();

        // recreate trails parented to the new car and reset transforms
        SetupTrailRenderers();
    }

    public void HandleDriftTrails(bool isTouching, float currentSpeed, float currentDriftAngle, ref float previousSpeed, ref float highSpeedTimer)
    {
        if (leftTrail == null || rightTrail == null || carChild == null || leftTrailObject == null || rightTrailObject == null)
            return;

        // --- Local wheel offsets relative to the visual car mesh (carChild) ---
        Vector3 leftWheelLocal = new Vector3(-wheelDistance * 0.5f, 0.05f, -rearWheelOffset);
        Vector3 rightWheelLocal = new Vector3(wheelDistance * 0.5f, 0.05f, -rearWheelOffset);

        // Convert local offsets to world space for raycasts
        Vector3 leftWheelWorld = carChild.TransformPoint(leftWheelLocal);
        Vector3 rightWheelWorld = carChild.TransformPoint(rightWheelLocal);

        RaycastHit hit;
        float raycastDistance = 2f;

        // Initialize with world positions
        Vector3 leftFinalWorld = leftWheelWorld;
        Vector3 rightFinalWorld = rightWheelWorld;

        // --- Ground detection (raycasts) ---
        if (Physics.Raycast(leftWheelWorld + Vector3.up * 0.5f, Vector3.down, out hit, raycastDistance))
            leftFinalWorld = hit.point + Vector3.up * 0.02f;
        else
            leftFinalWorld.y = carChild.position.y + 0.02f;

        if (Physics.Raycast(rightWheelWorld + Vector3.up * 0.5f, Vector3.down, out hit, raycastDistance))
            rightFinalWorld = hit.point + Vector3.up * 0.02f;
        else
            rightFinalWorld.y = carChild.position.y + 0.02f;

        // --- FIXED: Set positions correctly depending on parent space ---
        if (leftTrailObject.transform.parent == carChild)
        {
            leftTrailObject.transform.localPosition = carChild.InverseTransformPoint(leftFinalWorld);
            rightTrailObject.transform.localPosition = carChild.InverseTransformPoint(rightFinalWorld);
        }
        else
        {
            leftTrailObject.transform.position = leftFinalWorld;
            rightTrailObject.transform.position = rightFinalWorld;
        }

        // --- Drift & brake mark logic ---
        if (isTouching && currentSpeed > minBrakeSpeed)
            highSpeedTimer += Time.deltaTime;
        else if (currentSpeed < minBrakeSpeed * 0.7f)
            highSpeedTimer = 0f;

        bool isDrifting = Mathf.Abs(currentDriftAngle) > trailStartAngle;
        bool showDriftMarks = isDrifting;

        float decelAmount = (previousSpeed - currentSpeed) / Mathf.Max(Time.deltaTime, 0.01f);
        bool isBrakingNow = !isTouching && currentSpeed < previousSpeed;

        bool cameFromSpeed = previousSpeed > 10f;
        bool brakingStrong = decelAmount > 3.5f;
        bool stillRolling = currentSpeed > 2.5f;

        bool showBrakeMarks =
            cameFromSpeed &&
            brakingStrong &&
            stillRolling &&
            isBrakingNow &&
            highSpeedTimer > 0.3f;

        if (currentSpeed < 2f)
            showBrakeMarks = false;

        bool shouldShowTrails = showDriftMarks || showBrakeMarks;

        leftTrail.emitting = shouldShowTrails;
        rightTrail.emitting = shouldShowTrails;

        leftTrail.startWidth = rightTrail.startWidth = rightTrail.endWidth = leftTrail.endWidth = trailWidth;

        previousSpeed = currentSpeed;

#if UNITY_EDITOR
        // Optional: visualize wheel positions for debugging
        Debug.DrawLine(leftFinalWorld, leftFinalWorld + Vector3.up * 0.3f, Color.red);
        Debug.DrawLine(rightFinalWorld, rightFinalWorld + Vector3.up * 0.3f, Color.green);
#endif
    }

    void AutoDetectWheelsPosition()
    {
        if (carChild == null) return;

        Renderer carRenderer = carChild.GetComponent<Renderer>();
        if (carRenderer != null)
        {
            Bounds bounds = carRenderer.bounds;
            Vector3 localBoundsSize = carChild.InverseTransformVector(bounds.size);

            wheelDistance = Mathf.Abs(localBoundsSize.x) * 0.8f; // 80% of car width
            rearWheelOffset = Mathf.Abs(localBoundsSize.z) * 0.3f;

            LogHelper.Log("Auto-detected wheelDistance: " + wheelDistance + ", rearWheelOffset: " + rearWheelOffset);

        }

    }
    
    public void DisableTrails()
    {
        if (leftTrail != null) leftTrail.emitting = false;
        if (rightTrail != null) rightTrail.emitting = false;
    }
}