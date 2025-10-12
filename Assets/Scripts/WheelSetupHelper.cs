using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper component to quickly setup wheels for your car
/// Attach this temporarily to measure and configure wheels
/// Remove after setup is complete
/// </summary>
public class WheelSetupHelper : MonoBehaviour
{
    [Header("Quick Setup")]
    [Tooltip("Drag your 4 wheel GameObjects here")]
    public Transform frontLeft;
    public Transform frontRight;
    public Transform rearLeft;
    public Transform rearRight;

    [Header("Measurement Tools")]
    [SerializeField] private bool showWheelRadius = true;
    [SerializeField] private bool showWheelCenters = true;
    [SerializeField] private float measuredRadius = 0.35f;

    [Header("Auto Setup")]
    [Tooltip("Click this button to auto-assign wheels to VisualCarController")]
    public bool autoAssignToController = false;

    void OnDrawGizmos()
    {
        if (!showWheelCenters && !showWheelRadius) return;

        DrawWheelGizmo(frontLeft, Color.red, "FL");
        DrawWheelGizmo(frontRight, Color.green, "FR");
        DrawWheelGizmo(rearLeft, Color.blue, "RL");
        DrawWheelGizmo(rearRight, Color.yellow, "RR");
    }

    private void DrawWheelGizmo(Transform wheel, Color color, string label)
    {
        if (wheel == null) return;

        Gizmos.color = color;

        // Draw wheel center
        if (showWheelCenters)
        {
            Gizmos.DrawWireSphere(wheel.position, 0.1f);
        }

        // Draw wheel radius
        if (showWheelRadius)
        {
            // Draw circle representing wheel
            DrawCircle(wheel.position, wheel.right, measuredRadius, color);

            // Draw radius line
            Gizmos.DrawLine(wheel.position, wheel.position + wheel.up * measuredRadius);
        }

#if UNITY_EDITOR
        // Draw label
        GUIStyle style = new GUIStyle();
        style.normal.textColor = color;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;
        Handles.Label(wheel.position + Vector3.up * 0.5f, label, style);
#endif
    }

    private void DrawCircle(Vector3 center, Vector3 normal, float radius, Color color)
    {
        Gizmos.color = color;
        Vector3 forward = Vector3.Slerp(normal, -normal, 0.5f);
        Vector3 right = Vector3.Cross(normal, forward).normalized;

        Vector3 previousPoint = center + right * radius;

        for (int i = 1; i <= 32; i++)
        {
            float angle = (float)i / 32f * Mathf.PI * 2f;
            Vector3 newPoint = center + (right * Mathf.Cos(angle) + forward * Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(previousPoint, newPoint);
            previousPoint = newPoint;
        }
    }

    /// <summary>
    /// Automatically measure wheel radius from the wheel's scale
    /// </summary>
    public void MeasureWheelRadius()
    {
        if (frontLeft != null)
        {
            // Try to get renderer bounds
            Renderer renderer = frontLeft.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                measuredRadius = renderer.bounds.extents.y;
                Debug.Log($"Measured wheel radius: {measuredRadius:F3}");
            }
            else
            {
                // Fallback: use scale
                measuredRadius = frontLeft.localScale.y * 0.5f;
                Debug.Log($"Estimated wheel radius from scale: {measuredRadius:F3}");
            }
        }
    }

    /// <summary>
    /// Assign wheels to VisualCarController component
    /// </summary>
    public void AssignWheelsToController()
    {
        VisualCarController controller = GetComponent<VisualCarController>();

        if (controller == null)
        {
            Debug.LogError("No VisualCarController found on this GameObject!");
            return;
        }

        controller.ManuallyAssignWheels(frontLeft, frontRight, rearLeft, rearRight);
        Debug.Log("Wheels assigned to VisualCarController!");
    }

    /// <summary>
    /// Create empty parent objects for wheels at their current position
    /// Useful if wheels are part of the car mesh
    /// </summary>
    public void CreateWheelParents()
    {
        CreateWheelParent(ref frontLeft, "Wheel_FL");
        CreateWheelParent(ref frontRight, "Wheel_FR");
        CreateWheelParent(ref rearLeft, "Wheel_RL");
        CreateWheelParent(ref rearRight, "Wheel_RR");

        Debug.Log("Wheel parent objects created!");
    }

    private void CreateWheelParent(ref Transform wheel, string name)
    {
        if (wheel == null) return;

        // Create empty parent
        GameObject parent = new GameObject(name);
        parent.transform.position = wheel.position;
        parent.transform.rotation = wheel.rotation;
        parent.transform.SetParent(transform);

        // Make wheel a child of parent
        Transform originalParent = wheel.parent;
        wheel.SetParent(parent.transform);

        // Update reference
        wheel = parent.transform;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WheelSetupHelper))]
public class WheelSetupHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WheelSetupHelper helper = (WheelSetupHelper)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Measure Wheel Radius", GUILayout.Height(30)))
        {
            helper.MeasureWheelRadius();
        }

        if (GUILayout.Button("Assign to VisualCarController", GUILayout.Height(30)))
        {
            helper.AssignWheelsToController();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "1. Drag your wheel GameObjects to the slots above\n" +
            "2. Click 'Measure Wheel Radius' to auto-calculate size\n" +
            "3. Click 'Assign to VisualCarController' to complete setup\n" +
            "4. Remove this component when done!",
            MessageType.Info
        );
    }
}
#endif