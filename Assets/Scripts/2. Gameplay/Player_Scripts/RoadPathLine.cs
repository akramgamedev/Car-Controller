using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(LineRenderer))]
public class RoadPathLine : MonoBehaviour
{
    [Header("References")]
    public Transform car;
    private SplineContainer currentSpline;

    [Header("Settings")]
    public float yOffset = 0.05f;
    public float eraseDistance = 3f;

    [Header("Spline Sampling")]
    [Tooltip("Higher = smoother curves but more performance cost")]
    public int samplesPerUnit = 10;
    [Tooltip("Minimum number of samples for the entire path")]
    public int minSamples = 100;

    [Header("Line Appearance")]
    public float lineWidth = 0.25f;
    public Color lineColor = new Color(0.1f, 0.1f, 0.6f);

    private LineRenderer line;
    private Vector3[] fullPath;
    private float totalPathLength;
    private float lastLineWidth;
    private Color lastLineColor;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    void Start()
    {
        lastLineWidth = lineWidth;
        lastLineColor = lineColor;
    }

    void Update()
    {
        // Update line appearance if changed
        if (lineWidth != lastLineWidth)
        {
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            lastLineWidth = lineWidth;
        }

        if (lineColor != lastLineColor)
        {
            line.material.color = lineColor;
            lastLineColor = lineColor;
        }

        if (car == null || fullPath == null || fullPath.Length == 0) return;

        // Find closest point and hide passed sections
        int closestIndex = GetClosestPointIndex(car.position);

        int visibleCount = Mathf.Max(0, fullPath.Length - closestIndex);
        if (visibleCount <= 1)
        {
            line.positionCount = 0;
            return;
        }

        Vector3[] remaining = new Vector3[visibleCount];
        for (int i = 0; i < visibleCount; i++)
            remaining[i] = fullPath[closestIndex + i];

        line.positionCount = remaining.Length;
        line.SetPositions(remaining);
    }

    public void SetSpline(SplineContainer splineContainer)
    {
        currentSpline = splineContainer;

        if (currentSpline != null && currentSpline.Spline != null)
        {
            RegeneratePath();
        }
        else
        {
            LogHelper.LogError("SplinePathLine: Invalid spline container provided!");
        }
    }

    private void SetupLineRenderer()
    {
        line.useWorldSpace = true;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        var mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = lineColor;
        line.material = mat;
    }

    // private void RegeneratePath()
    // {
    //     if (currentSpline == null || currentSpline.Spline == null)
    //     {
    //         LogHelper.LogError("Cannot regenerate path - no spline assigned!");
    //         return;
    //     }

    //     totalPathLength = currentSpline.Spline.GetLength();

    //     List<Vector3> adaptivePath = new List<Vector3>();

    //     int baseSegments = Mathf.Max(minSamples, Mathf.RoundToInt(totalPathLength * samplesPerUnit));

    //     for (int i = 0; i < baseSegments; i++)
    //     {
    //         float t = i / (float)(baseSegments - 1);
    //         Vector3 localPos = currentSpline.Spline.EvaluatePosition(t);
    //         Vector3 worldPos = currentSpline.transform.TransformPoint(localPos);
    //         adaptivePath.Add(worldPos + Vector3.up * yOffset);

    //         if (i < baseSegments - 1)
    //         {
    //             float nextT = (i + 1) / (float)(baseSegments - 1);
    //             Vector3 nextLocalPos = currentSpline.Spline.EvaluatePosition(nextT);
    //             Vector3 nextWorldPos = currentSpline.transform.TransformPoint(nextLocalPos);

    //             if (i > 0)
    //             {
    //                 Vector3 dir1 = (worldPos - adaptivePath[adaptivePath.Count - 2]).normalized;
    //                 Vector3 dir2 = (nextWorldPos - worldPos).normalized;
    //                 float angle = Vector3.Angle(dir1, dir2);

    //                 if (angle > 15f)
    //                 {
    //                     int extraSamples = Mathf.CeilToInt(angle / 10f);
    //                     for (int j = 1; j <= extraSamples; j++)
    //                     {
    //                         float midT = Mathf.Lerp(t, nextT, j / (float)(extraSamples + 1));
    //                         Vector3 midLocal = currentSpline.Spline.EvaluatePosition(midT);
    //                         Vector3 midWorld = currentSpline.transform.TransformPoint(midLocal);
    //                         adaptivePath.Add(midWorld + Vector3.up * yOffset);
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //     fullPath = SmoothPath(adaptivePath.ToArray(), 2);

    //     line.positionCount = fullPath.Length;
    //     line.SetPositions(fullPath);

    //     LogHelper.Log($"SplinePathLine: Generated smooth path with {fullPath.Length} samples");

    // }

    // private Vector3[] SmoothPath(Vector3[] path, int smoothPasses)
    // {
    //     if (path.Length < 3) return path;

    //     Vector3[] smoothed = new Vector3[path.Length];
    //     System.Array.Copy(path, smoothed, path.Length);

    //     for (int pass = 0; pass < smoothPasses; pass++)
    //     {
    //         Vector3[] temp = new Vector3[smoothed.Length];

    //         temp[0] = smoothed[0];
    //         temp[temp.Length - 1] = smoothed[smoothed.Length - 1];

    //         for (int i = 1; i < smoothed.Length - 1; i++)
    //         {
    //             Vector3 prev = smoothed[i - 1];
    //             Vector3 curr = smoothed[i];
    //             Vector3 next = smoothed[i + 1];

    //             temp[i] = (prev + curr + next) * 0.25f;

    //         }
    //         smoothed = temp;
    //     }
    //     return smoothed;
    // }

    private void RegeneratePath()
    {
        if (currentSpline == null || currentSpline.Spline == null)
        {
            LogHelper.LogError("Cannot regenerate path - no spline assigned!");
            return;
        }

        // Calculate spline length
        totalPathLength = currentSpline.Spline.GetLength();

        // Calculate number of samples based on length
        int sampleCount = Mathf.Max(minSamples, Mathf.RoundToInt(totalPathLength * samplesPerUnit));

        fullPath = new Vector3[sampleCount];

        // Sample points evenly along the spline
        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)(sampleCount - 1); // Normalize to 0-1

            // Evaluate position on spline
            Vector3 localPos = currentSpline.Spline.EvaluatePosition(t);

            // Transform to world space
            Vector3 worldPos = currentSpline.transform.TransformPoint(localPos);

            // Add offset
            fullPath[i] = worldPos + Vector3.up * yOffset;
        }

        // Set initial line renderer positions
        line.positionCount = fullPath.Length;
        line.SetPositions(fullPath);

        LogHelper.Log($"SplinePathLine: Generated path with {sampleCount} samples (length: {totalPathLength:F2})");
    }

    private int GetClosestPointIndex(Vector3 carPos)
    {
        if (fullPath == null || fullPath.Length == 0)
            return 0;

        float minDist = float.MaxValue;
        int index = 0;

        // Optimize by only checking every few points for large paths
        int step = Mathf.Max(1, fullPath.Length / 200);

        for (int i = 0; i < fullPath.Length; i += step)
        {
            float dist = Vector3.Distance(carPos, fullPath[i]);
            if (dist < minDist)
            {
                minDist = dist;
                index = i;
            }
        }

        // Fine-tune around the approximate closest point
        int searchRange = step * 2;
        int startSearch = Mathf.Max(0, index - searchRange);
        int endSearch = Mathf.Min(fullPath.Length - 1, index + searchRange);

        for (int i = startSearch; i <= endSearch; i++)
        {
            float dist = Vector3.Distance(carPos, fullPath[i]);
            if (dist < minDist)
            {
                minDist = dist;
                index = i;
            }
        }

        return index;
    }

    public void ClearPath()
    {
        if (line != null)
        {
            line.positionCount = 0;
        }
        fullPath = null;
        currentSpline = null;
    }

    // Helper method to get progress along the path
    public float GetPathProgress(Vector3 position)
    {
        if (fullPath == null || fullPath.Length == 0)
            return 0f;

        int closestIndex = GetClosestPointIndex(position);
        return closestIndex / (float)fullPath.Length;
    }
}


// using UnityEngine;
// using System.Collections.Generic;

// [RequireComponent(typeof(LineRenderer))]
// public class RoadPathLine : MonoBehaviour
// {
//     [Header("References")]
//     public Transform car;
//     public List<Transform> roadPoints;

//     [Header("Settings")]
//     public float yOffset = 0.05f;
//     public float eraseDistance = 3f;
//     public int curveResolution = 20;

//     [Header("Line Appearance")]
//     public float lineWidth = 0.25f;
//     public Color lineColor = new Color(0.1f, 0.1f, 0.6f);

//     [Header("Real-time Editing")]
//     public bool updateInPlayMode = true;

//     private LineRenderer line;
//     private List<Vector3> fullPath = new List<Vector3>();
//     private float totalPathLength;
//     private List<Vector3> lastWaypointPositions = new List<Vector3>();
//     private float lastLineWidth;
//     private Color lastLineColor;

//     void Awake()
//     {
//         line = GetComponent<LineRenderer>();
//         SetupLineRenderer();
//     }

//     void Start()
//     {
//         if (roadPoints.Count < 2)
//         {
//             LogHelper.LogError("Please assign at least 2 roadPoints in the inspector!");
//             return;
//         }

//         RegeneratePath();
//         CacheWaypointPositions();
//         lastLineWidth = lineWidth;
//         lastLineColor = lineColor;
//     }

//     void Update()
//     {
//         if (updateInPlayMode && HasWaypointsChanged())
//         {
//             RegeneratePath();
//             CacheWaypointPositions();
//         }

//         if (lineWidth != lastLineWidth)
//         {
//             line.startWidth = lineWidth;
//             line.endWidth = lineWidth;
//             lastLineWidth = lineWidth;
//         }

//         if (lineColor != lastLineColor)
//         {
//             line.material.color = lineColor;
//             lastLineColor = lineColor;
//         }

//         if (car == null || fullPath.Count == 0) return;

//         int closestIndex = GetClosestPointIndex(car.position);

//         int visibleCount = Mathf.Max(0, fullPath.Count - closestIndex);
//         if (visibleCount <= 1)
//         {
//             line.positionCount = 0;
//             return;
//         }

//         Vector3[] remaining = new Vector3[visibleCount];
//         for (int i = 0; i < visibleCount; i++)
//             remaining[i] = fullPath[closestIndex + i];

//         line.positionCount = remaining.Length;
//         line.SetPositions(remaining);
//     }

//     private void SetupLineRenderer()
//     {
//         line.useWorldSpace = true;
//         line.startWidth = lineWidth;
//         line.endWidth = lineWidth;

//         var mat = new Material(Shader.Find("Unlit/Color"));
//         mat.color = lineColor;
//         line.material = mat;
//     }

//     private void RegeneratePath()
//     {
//         fullPath = GenerateSmoothPath(roadPoints, curveResolution);

//         line.positionCount = fullPath.Count;
//         line.SetPositions(fullPath.ToArray());

//         totalPathLength = CalculatePathLength(fullPath);
//     }

//     private void CacheWaypointPositions()
//     {
//         lastWaypointPositions.Clear();
//         foreach (var point in roadPoints)
//         {
//             if (point != null)
//                 lastWaypointPositions.Add(point.position);
//         }
//     }

//     private bool HasWaypointsChanged()
//     {
//         if (roadPoints.Count != lastWaypointPositions.Count)
//             return true;

//         for (int i = 0; i < roadPoints.Count; i++)
//         {
//             if (roadPoints[i] == null) continue;
//             if (Vector3.Distance(roadPoints[i].position, lastWaypointPositions[i]) > 0.01f)
//                 return true;
//         }
//         return false;
//     }

//     private List<Vector3> GenerateSmoothPath(List<Transform> points, int resolution)
//     {
//         List<Vector3> path = new List<Vector3>();

//         for (int i = 0; i < points.Count - 1; i++)
//         {
//             Vector3 p0 = i == 0 ? points[i].position : points[i - 1].position;
//             Vector3 p1 = points[i].position;
//             Vector3 p2 = points[i + 1].position;
//             Vector3 p3 = (i + 2 < points.Count) ? points[i + 2].position : p2;

//             for (int j = 0; j < resolution; j++)
//             {
//                 float t = j / (float)resolution;
//                 Vector3 pos = CatmullRom(p0, p1, p2, p3, t) + Vector3.up * yOffset;
//                 path.Add(pos);
//             }
//         }

//         path.Add(points[points.Count - 1].position + Vector3.up * yOffset);
//         return path;
//     }

//     private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
//     {
//         return 0.5f * (
//             (2 * p1) +
//             (-p0 + p2) * t +
//             (2 * p0 - 5 * p1 + 4 * p2 - p3) * (t * t) +
//             (-p0 + 3 * p1 - 3 * p2 + p3) * (t * t * t)
//         );
//     }

//     private int GetClosestPointIndex(Vector3 carPos)
//     {
//         float minDist = float.MaxValue;
//         int index = 0;
//         for (int i = 0; i < fullPath.Count; i++)
//         {
//             float dist = Vector3.Distance(carPos, fullPath[i]);
//             if (dist < minDist)
//             {
//                 minDist = dist;
//                 index = i;
//             }
//         }
//         return index;
//     }

//     private float CalculatePathLength(List<Vector3> pts)
//     {
//         float length = 0;
//         for (int i = 1; i < pts.Count; i++)
//             length += Vector3.Distance(pts[i - 1], pts[i]);
//         return length;
//     }
// }