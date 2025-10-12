using UnityEngine;

/// <summary>
/// Quick fix tool for wheels that rotate incorrectly
/// Attach to Car object temporarily to diagnose and fix wheel rotation issues
/// </summary>
public class WheelRotationFixTool : MonoBehaviour
{
    [Header("Wheel References")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    [Header("Fix Options")]
    [Tooltip("Rotate all wheel CHILDREN by this amount to align them correctly")]
    public Vector3 wheelChildRotationOffset = Vector3.zero;

    [Tooltip("Apply the rotation fix to all wheels")]
    public bool applyRotationFix = false;

    [Header("Test Rotation")]
    [Tooltip("Test spin all wheels to see which axis is correct")]
    [Range(-360, 360)]
    public float testRotationX = 0f;
    [Range(-360, 360)]
    public float testRotationY = 0f;
    [Range(-360, 360)]
    public float testRotationZ = 0f;

    [Header("Diagnosis")]
    [SerializeField] private bool showWheelAxes = true;

    void Update()
    {
        if (applyRotationFix)
        {
            ApplyRotationFix();
            applyRotationFix = false;
        }

        // Apply test rotation
        ApplyTestRotation(frontLeftWheel);
        ApplyTestRotation(frontRightWheel);
        ApplyTestRotation(rearLeftWheel);
        ApplyTestRotation(rearRightWheel);
    }

    private void ApplyTestRotation(Transform wheel)
    {
        if (wheel == null) return;

        // Apply test rotation directly to the wheel parent
        wheel.localRotation = Quaternion.Euler(testRotationX, testRotationY, testRotationZ);
    }

    private void ApplyRotationFix()
    {
        Debug.Log("Applying rotation fix to wheel children...");

        FixWheelChild(frontLeftWheel, "Front Left");
        FixWheelChild(frontRightWheel, "Front Right");
        FixWheelChild(rearLeftWheel, "Rear Left");
        FixWheelChild(rearRightWheel, "Rear Right");

        Debug.Log("Rotation fix applied! You can now remove this component.");
    }

    private void FixWheelChild(Transform wheelParent, string name)
    {
        if (wheelParent == null) return;

        // Find the actual wheel mesh (first child)
        if (wheelParent.childCount > 0)
        {
            Transform wheelMesh = wheelParent.GetChild(0);
            wheelMesh.localRotation = Quaternion.Euler(wheelChildRotationOffset);
            Debug.Log($"{name} wheel mesh rotated by {wheelChildRotationOffset}");
        }
        else
        {
            Debug.LogWarning($"{name} wheel has no children to rotate!");
        }
    }

    void OnDrawGizmos()
    {
        if (!showWheelAxes) return;

        DrawWheelAxes(frontLeftWheel, "FL");
        DrawWheelAxes(frontRightWheel, "FR");
        DrawWheelAxes(rearLeftWheel, "RL");
        DrawWheelAxes(rearRightWheel, "RR");
    }

    private void DrawWheelAxes(Transform wheel, string label)
    {
        if (wheel == null) return;

        float axisLength = 0.5f;

        // Draw X axis (RED) - usually the spin axis
        Gizmos.color = Color.red;
        Gizmos.DrawLine(wheel.position, wheel.position + wheel.right * axisLength);

        // Draw Y axis (GREEN) - usually the steer axis
        Gizmos.color = Color.green;
        Gizmos.DrawLine(wheel.position, wheel.position + wheel.up * axisLength);

        // Draw Z axis (BLUE) - usually forward
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(wheel.position, wheel.position + wheel.forward * axisLength);

        // Draw label
#if UNITY_EDITOR
        UnityEditor.Handles.Label(wheel.position + Vector3.up * 0.5f, label);
#endif
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(WheelRotationFixTool))]
public class WheelRotationFixToolEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WheelRotationFixTool tool = (WheelRotationFixTool)target;

        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.HelpBox(
            "HOW TO FIX WHEEL ROTATION:\n\n" +
            "1. Look at Scene view - see colored axes on wheels\n" +
            "   RED = X, GREEN = Y, BLUE = Z\n\n" +
            "2. Use Test Rotation sliders to find which axis spins correctly\n" +
            "   - Usually X-axis (red) should spin the wheel\n\n" +
            "3. If wheels are sideways/upside down:\n" +
            "   - Set 'Wheel Child Rotation Offset' (try 0, 90, 180, or -90 on different axes)\n" +
            "   - Click 'Apply Rotation Fix'\n\n" +
            "4. Update VisualCarController settings:\n" +
            "   - Set 'Wheel Rotation Axis' to match test results\n" +
            "   - Check 'Invert Rotation' if wheels spin backward",
            UnityEditor.MessageType.Info
        );

        UnityEditor.EditorGUILayout.Space();

        if (UnityEditor.EditorGUILayout.Toggle("Apply Rotation Fix", false))
        {
            tool.applyRotationFix = true;
        }
    }
}
#endif