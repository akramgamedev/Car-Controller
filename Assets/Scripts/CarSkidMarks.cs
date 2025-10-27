using UnityEngine;

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
            Debug.LogWarning("CarSkidMarks: No active child with 'Car' tag found under " + gameObject.name);
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
        if (leftTrail == null)
        {
            leftTrailObject = new GameObject("LeftDriftTrail");
            leftTrailObject.transform.SetParent(transform);
            leftTrail = leftTrailObject.AddComponent<TrailRenderer>();
        }

        if (rightTrail == null)
        {
            rightTrailObject = new GameObject("RightDriftTrail");
            rightTrailObject.transform.SetParent(transform);
            rightTrail = rightTrailObject.AddComponent<TrailRenderer>();
        }

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

        Gradient gradient = new Gradient();
        gradient.mode = GradientMode.Blend;
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(trailColor, 0.0f),
                new GradientColorKey(trailColor, 1.0f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(trailAlpha, 0.0f),
                new GradientAlphaKey(trailAlpha * 0.85f, 0.3f),
                new GradientAlphaKey(trailAlpha * 0.65f, 0.5f),
                new GradientAlphaKey(trailAlpha * 0.45f, 0.7f),
                new GradientAlphaKey(trailAlpha * 0.25f, 0.85f),
                new GradientAlphaKey(0f, 1.0f)
            }
        );
        trail.colorGradient = gradient;

        AnimationCurve widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
        trail.widthCurve = widthCurve;

        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.material.color = trailColor;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;
        trail.alignment = LineAlignment.TransformZ;
        trail.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        trail.textureMode = LineTextureMode.Stretch;
        trail.minVertexDistance = 0.5f;
        trail.generateLightingData = false;
    }

    public void Initialize(Transform car)
    {
        carChild = car;
        SetupTrailRenderers();
    }

    public void HandleDriftTrails(bool isTouching, float currentSpeed, float currentDriftAngle, ref float previousSpeed, ref float highSpeedTimer)
    {
        if (leftTrail == null || rightTrail == null || carChild == null) return;

        Vector3 leftWheelLocal = new Vector3(-wheelDistance * 0.5f, 0.05f, -rearWheelOffset);
        Vector3 rightWheelLocal = new Vector3(wheelDistance * 0.5f, 0.05f, -rearWheelOffset);

        Vector3 leftWheelWorld = carChild.TransformPoint(leftWheelLocal);
        Vector3 rightWheelWorld = carChild.TransformPoint(rightWheelLocal);

        RaycastHit hit;
        float raycastDistance = 2f;

        if (Physics.Raycast(leftWheelWorld + Vector3.up * 0.5f, Vector3.down, out hit, raycastDistance))
            leftWheelWorld = hit.point + Vector3.up * 0.02f;
        else
            leftWheelWorld.y = transform.position.y + 0.02f;

        if (Physics.Raycast(rightWheelWorld + Vector3.up * 0.5f, Vector3.down, out hit, raycastDistance))
            rightWheelWorld = hit.point + Vector3.up * 0.02f;
        else
            rightWheelWorld.y = transform.position.y + 0.02f;

        leftTrailObject.transform.position = leftWheelWorld;
        rightTrailObject.transform.position = rightWheelWorld;

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
    }
}



// using UnityEngine;

// public class CarSkidMarks : MonoBehaviour
// {
//     [Header("Trail Color Settings")]
//     public Color trailColor = Color.black;
//     [Range(0f, 1f)] public float trailAlpha = 0.9f;

//     [Header("Drift Trail Settings")]
//     public TrailRenderer leftTrail;
//     public TrailRenderer rightTrail;
//     public float wheelDistance = 1.5f;
//     public float rearWheelOffset = 1.2f;
//     public float trailStartAngle = 12f;
//     public float trailWidth = 0.3f;
//     public float trailLifetime = 3f;

//     [Header("Brake Mark Settings")]
//     public float minBrakeSpeed = 12f;
//     public float minHighSpeedDuration = 1.5f;

//     private GameObject leftTrailObject;
//     private GameObject rightTrailObject;
//     private Transform carChild;

//     public void Initialize(Transform car)
//     {
//         carChild = car;
//         SetupTrailRenderers();
//     }

//     void SetupTrailRenderers()
//     {
//         if (leftTrail == null)
//         {
//             leftTrailObject = new GameObject("LeftDriftTrail");
//             leftTrailObject.transform.SetParent(transform);
//             leftTrail = leftTrailObject.AddComponent<TrailRenderer>();
//         }

//         if (rightTrail == null)
//         {
//             rightTrailObject = new GameObject("RightDriftTrail");
//             rightTrailObject.transform.SetParent(transform);
//             rightTrail = rightTrailObject.AddComponent<TrailRenderer>();
//         }

//         ConfigureTrail(leftTrail);
//         ConfigureTrail(rightTrail);

//         leftTrail.emitting = false;
//         rightTrail.emitting = false;
//     }

//     void ConfigureTrail(TrailRenderer trail)
//     {
//         trail.time = trailLifetime;
//         trail.autodestruct = false;

//         trail.startWidth = trailWidth;
//         trail.endWidth = trailWidth;

//         Gradient gradient = new Gradient();
//         gradient.mode = GradientMode.Blend;
//         gradient.SetKeys(
//             new GradientColorKey[]
//             {
//                 new GradientColorKey(trailColor, 0.0f),
//                 new GradientColorKey(trailColor, 1.0f)
//             },
//             new GradientAlphaKey[]
//             {
//                 new GradientAlphaKey(trailAlpha, 0.0f),
//                 new GradientAlphaKey(trailAlpha * 0.85f, 0.3f),
//                 new GradientAlphaKey(trailAlpha * 0.65f, 0.5f),
//                 new GradientAlphaKey(trailAlpha * 0.45f, 0.7f),
//                 new GradientAlphaKey(trailAlpha * 0.25f, 0.85f),
//                 new GradientAlphaKey(0f, 1.0f)
//             }
//         );
//         trail.colorGradient = gradient;

//         AnimationCurve widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
//         trail.widthCurve = widthCurve;

//         trail.material = new Material(Shader.Find("Sprites/Default"));
//         trail.material.color = trailColor;
//         trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
//         trail.receiveShadows = false;
//         trail.alignment = LineAlignment.TransformZ;
//         trail.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
//         trail.textureMode = LineTextureMode.Stretch;
//         trail.minVertexDistance = 0.5f;
//         trail.generateLightingData = false;
//     }

//     public void HandleDriftTrails(bool isTouching, float currentSpeed, float currentDriftAngle, ref float previousSpeed, ref float highSpeedTimer)
//     {
//         if (leftTrail == null || rightTrail == null || carChild == null) return;

//         Vector3 leftWheelLocal = new Vector3(-wheelDistance * 0.5f, 0.05f, -rearWheelOffset);
//         Vector3 rightWheelLocal = new Vector3(wheelDistance * 0.5f, 0.05f, -rearWheelOffset);

//         Vector3 leftWheelWorld = carChild.TransformPoint(leftWheelLocal);
//         Vector3 rightWheelWorld = carChild.TransformPoint(rightWheelLocal);

//         RaycastHit hit;
//         float raycastDistance = 2f;

//         if (Physics.Raycast(leftWheelWorld + Vector3.up * 0.5f, Vector3.down, out hit, raycastDistance))
//             leftWheelWorld = hit.point + Vector3.up * 0.02f;
//         else
//             leftWheelWorld.y = transform.position.y + 0.02f;

//         if (Physics.Raycast(rightWheelWorld + Vector3.up * 0.5f, Vector3.down, out hit, raycastDistance))
//             rightWheelWorld = hit.point + Vector3.up * 0.02f;
//         else
//             rightWheelWorld.y = transform.position.y + 0.02f;

//         leftTrailObject.transform.position = leftWheelWorld;
//         rightTrailObject.transform.position = rightWheelWorld;

//         if (isTouching && currentSpeed > minBrakeSpeed)
//             highSpeedTimer += Time.deltaTime;
//         else if (currentSpeed < minBrakeSpeed * 0.7f)
//             highSpeedTimer = 0f;

//         bool isDrifting = Mathf.Abs(currentDriftAngle) > trailStartAngle;
//         bool showDriftMarks = isDrifting;

//         float decelAmount = (previousSpeed - currentSpeed) / Mathf.Max(Time.deltaTime, 0.01f);
//         bool isBrakingNow = !isTouching && currentSpeed < previousSpeed;

//         bool cameFromSpeed = previousSpeed > 10f;
//         bool brakingStrong = decelAmount > 3.5f;
//         bool stillRolling = currentSpeed > 2.5f;

//         bool showBrakeMarks =
//             cameFromSpeed &&
//             brakingStrong &&
//             stillRolling &&
//             isBrakingNow &&
//             highSpeedTimer > 0.3f;

//         if (currentSpeed < 2f)
//             showBrakeMarks = false;

//         bool shouldShowTrails = showDriftMarks || showBrakeMarks;
//         leftTrail.emitting = shouldShowTrails;
//         rightTrail.emitting = shouldShowTrails;

//         leftTrail.startWidth = rightTrail.startWidth = rightTrail.endWidth = leftTrail.endWidth = trailWidth;

//         previousSpeed = currentSpeed;
//     }
// }
