using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class RoadPathLine : MonoBehaviour
{
    [Header("References")]
    public Transform car;
    public List<Transform> roadPoints;

    [Header("Settings")]
    public float yOffset = 0.05f;
    public float eraseDistance = 3f;
    public int curveResolution = 20;

    [Header("Line Appearance")]
    public float lineWidth = 0.25f;
    public Color lineColor = new Color(0.1f, 0.1f, 0.6f);

    [Header("Real-time Editing")]
    public bool updateInPlayMode = true;

    private LineRenderer line;
    private List<Vector3> fullPath = new List<Vector3>();
    private float totalPathLength;
    private List<Vector3> lastWaypointPositions = new List<Vector3>();
    private float lastLineWidth;
    private Color lastLineColor;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    void Start()
    {
        if (roadPoints.Count < 2)
        {
            LogHelper.LogError("Please assign at least 2 roadPoints in the inspector!");
            return;
        }

        RegeneratePath();
        CacheWaypointPositions();
        lastLineWidth = lineWidth;
        lastLineColor = lineColor;
    }

    void Update()
    {
        if (updateInPlayMode && HasWaypointsChanged())
        {
            RegeneratePath();
            CacheWaypointPositions();
        }

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

        if (car == null || fullPath.Count == 0) return;

        int closestIndex = GetClosestPointIndex(car.position);

        int visibleCount = Mathf.Max(0, fullPath.Count - closestIndex);
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

    private void SetupLineRenderer()
    {
        line.useWorldSpace = true;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        var mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = lineColor;
        line.material = mat;
    }

    private void RegeneratePath()
    {
        fullPath = GenerateSmoothPath(roadPoints, curveResolution);

        line.positionCount = fullPath.Count;
        line.SetPositions(fullPath.ToArray());

        totalPathLength = CalculatePathLength(fullPath);
    }

    private void CacheWaypointPositions()
    {
        lastWaypointPositions.Clear();
        foreach (var point in roadPoints)
        {
            if (point != null)
                lastWaypointPositions.Add(point.position);
        }
    }

    private bool HasWaypointsChanged()
    {
        if (roadPoints.Count != lastWaypointPositions.Count)
            return true;

        for (int i = 0; i < roadPoints.Count; i++)
        {
            if (roadPoints[i] == null) continue;
            if (Vector3.Distance(roadPoints[i].position, lastWaypointPositions[i]) > 0.01f)
                return true;
        }
        return false;
    }

    private List<Vector3> GenerateSmoothPath(List<Transform> points, int resolution)
    {
        List<Vector3> path = new List<Vector3>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 p0 = i == 0 ? points[i].position : points[i - 1].position;
            Vector3 p1 = points[i].position;
            Vector3 p2 = points[i + 1].position;
            Vector3 p3 = (i + 2 < points.Count) ? points[i + 2].position : p2;

            for (int j = 0; j < resolution; j++)
            {
                float t = j / (float)resolution;
                Vector3 pos = CatmullRom(p0, p1, p2, p3, t) + Vector3.up * yOffset;
                path.Add(pos);
            }
        }

        path.Add(points[points.Count - 1].position + Vector3.up * yOffset);
        return path;
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            (2 * p1) +
            (-p0 + p2) * t +
            (2 * p0 - 5 * p1 + 4 * p2 - p3) * (t * t) +
            (-p0 + 3 * p1 - 3 * p2 + p3) * (t * t * t)
        );
    }

    private int GetClosestPointIndex(Vector3 carPos)
    {
        float minDist = float.MaxValue;
        int index = 0;
        for (int i = 0; i < fullPath.Count; i++)
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

    private float CalculatePathLength(List<Vector3> pts)
    {
        float length = 0;
        for (int i = 1; i < pts.Count; i++)
            length += Vector3.Distance(pts[i - 1], pts[i]);
        return length;
    }
}

//********** working code **********//

// using UnityEngine;
// using System.Collections.Generic;

// [RequireComponent(typeof(LineRenderer))]
// public class RoadPathLine : MonoBehaviour
// {
//     [Header("References")]
//     public Transform car; // Drag your car here
//     public List<Transform> roadPoints; // Assign road waypoints here in order

//     [Header("Settings")]
//     public float yOffset = 0.05f;  // Slightly above road
//     public float eraseDistance = 3f; // How far behind car the line disappears
//     public int curveResolution = 20; // Smoothness per segment

//     private LineRenderer line;
//     private List<Vector3> fullPath = new List<Vector3>();
//     private float totalPathLength;

//     void Awake()
//     {
//         line = GetComponent<LineRenderer>();
//         SetupLineRenderer();
//     }

//     void Start()
//     {
//         if (roadPoints.Count < 2)
//         {
//             Debug.LogError("❌ Please assign at least 2 roadPoints in the inspector!");
//             return;
//         }

//         // Generate full smooth path using Catmull-Rom spline
//         fullPath = GenerateSmoothPath(roadPoints, curveResolution);

//         line.positionCount = fullPath.Count;
//         line.SetPositions(fullPath.ToArray());

//         // Calculate total path length for fade reference
//         totalPathLength = CalculatePathLength(fullPath);
//     }

//     void Update()
//     {
//         if (car == null || fullPath.Count == 0) return;

//         // Find nearest point index to car
//         int closestIndex = GetClosestPointIndex(car.position);

//         // Keep only points ahead of car (erase behind)
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
//         line.startWidth = 0.25f;
//         line.endWidth = 0.25f;

//         var mat = new Material(Shader.Find("Unlit/Color"));
//         mat.color = new Color(0.1f, 0.1f, 0.6f); // navy blue
//         line.material = mat;
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

//         // Add final point
//         path.Add(points[points.Count - 1].position + Vector3.up * yOffset);
//         return path;
//     }

//     private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
//     {
//         // Catmull-Rom spline interpolation
//         return 0.5f * (
//             (2 * p1) +
//             (-p0 + p2) * t +
//             (2*p0 - 5*p1 + 4*p2 - p3) * (t * t) +
//             (-p0 + 3*p1 - 3*p2 + p3) * (t * t * t)
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

//********** working code ends here **********//






// using UnityEngine;
// using System.Collections.Generic;

// [RequireComponent(typeof(LineRenderer))]
// public class PathLineRenderer : MonoBehaviour
// {
//     [Header("References")]
//     public Transform car;                 // Drag your car here

//     [Header("Settings")]
//     public float minDistance = 0.2f;      // Distance before adding new point
//     public float visibleLength = 10f;     // How much of the trail stays visible
//     public int smoothness = 10;           // For curve smoothness
//     public float yOffset = 0.05f;         // Slightly above road

//     private LineRenderer line;
//     private List<Vector3> points = new List<Vector3>();

//     void Awake()
//     {
//         line = GetComponent<LineRenderer>();
//         SetupLineRenderer();
//     }

//     void Start()
//     {
//         // 🔹 Test visibility: draw static line first
//         line.positionCount = 2;
//         line.SetPosition(0, new Vector3(0, 0.1f, 0));
//         line.SetPosition(1, new Vector3(0, 0.1f, 10));
//         Debug.Log("✅ LineRenderer visible test: you should see a blue line from (0,0,0) to (0,0,10)");
//     }

//     void Update()
//     {
//         if (car == null) return;

//         Vector3 currentPos = car.position + Vector3.up * yOffset;

//         // Add point when car moves
//         if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], currentPos) > minDistance)
//         {
//             points.Add(currentPos);
//         }

//         // Trim old points
//         float totalDistance = 0f;
//         for (int i = points.Count - 1; i > 0; i--)
//         {
//             totalDistance += Vector3.Distance(points[i], points[i - 1]);
//             if (totalDistance > visibleLength)
//             {
//                 points.RemoveRange(0, Mathf.Max(0, i - 1));
//                 break;
//             }
//         }

//         if (points.Count < 2) return;

//         Vector3[] smoothed = SmoothLine(points, smoothness);
//         line.positionCount = smoothed.Length;
//         line.SetPositions(smoothed);
//     }

//     private void SetupLineRenderer()
//     {
//         line.useWorldSpace = true;
//         line.startWidth = 0.25f;
//         line.endWidth = 0.25f;

//         // Force a simple visible material
//         var mat = new Material(Shader.Find("Unlit/Color"));
//         mat.color = Color.blue;
//         line.material = mat;

//         // Optional fade gradient
//         Gradient g = new Gradient();
//         g.SetKeys(
//             new GradientColorKey[] {
//                 new GradientColorKey(Color.blue, 0f),
//                 new GradientColorKey(Color.blue, 1f)
//             },
//             new GradientAlphaKey[] {
//                 new GradientAlphaKey(1f, 0f),
//                 new GradientAlphaKey(0f, 1f)
//             }
//         );
//         line.colorGradient = g;
//     }

//     private Vector3[] SmoothLine(List<Vector3> input, int subdivisions)
//     {
//         if (input.Count < 2) return input.ToArray();
//         List<Vector3> result = new List<Vector3>();
//         for (int i = 0; i < input.Count - 1; i++)
//         {
//             Vector3 p0 = input[i];
//             Vector3 p1 = input[i + 1];
//             for (int j = 0; j < subdivisions; j++)
//             {
//                 float t = j / (float)subdivisions;
//                 result.Add(Vector3.Lerp(p0, p1, t));
//             }
//         }
//         result.Add(input[input.Count - 1]);
//         return result.ToArray();
//     }
// }
