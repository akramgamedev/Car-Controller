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
    private Transform carCache;
    private Vector3[] remainingPathCache;
    private int lastVisibleCount = 0;

    private int lastClosestIndex = 0;
    private int framesSinceLastSearch = 0;

    private Vector3 lastCarPosition;
    private const float MIN_CAR_MOVEMENT = 0.1f;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    void Start()
    {
        lastLineWidth = lineWidth;
        lastLineColor = lineColor;
        carCache = car;
        remainingPathCache = new Vector3[2048];
    }

    void Update()
    {
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


        if (carCache == null || fullPath == null || fullPath.Length == 0) return;

        float carMovementSqr = (carCache.position - lastCarPosition).sqrMagnitude;
        if (carMovementSqr < MIN_CAR_MOVEMENT * MIN_CAR_MOVEMENT)
            return;
        lastCarPosition = carCache.position;

        int closestIndex = GetClosestPointIndex(carCache.position);

        int visibleCount = Mathf.Max(0, fullPath.Length - closestIndex);
        if (visibleCount <= 1)
        {
            if (line.positionCount != 0)
                line.positionCount = 0;
            return;
        }

        // if (remainingPathCache.Length < visibleCount)
        // {
        //     remainingPathCache = new Vector3[visibleCount];
        // }

        // //  Vector3[] remaining = new Vector3[visibleCount];
        // for (int i = 0; i < visibleCount; i++)
        //     remainingPathCache[i] = fullPath[closestIndex + i];

        // line.positionCount = visibleCount;
        // line.SetPositions(remainingPathCache);

        if (Mathf.Abs(visibleCount - lastVisibleCount) > 5 || lastVisibleCount == 0)
        {
            if (remainingPathCache.Length < visibleCount)
            {
                remainingPathCache = new Vector3[Mathf.NextPowerOfTwo(visibleCount)];
            }

            // for (int i = 0; i < visibleCount; i++)
            // remainingPathCache[i] = fullPath[closestIndex + i];
            System.Array.Copy(fullPath, closestIndex, remainingPathCache, 0, visibleCount);

            line.positionCount = visibleCount;
            line.SetPositions(remainingPathCache);
            lastVisibleCount = visibleCount;
        }
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
    }

    private void RegeneratePath()
    {
        if (currentSpline == null || currentSpline.Spline == null)
        {
            LogHelper.LogError("Cannot regenerate path - no spline assigned!");
            return;
        }

        totalPathLength = currentSpline.Spline.GetLength();

        int sampleCount = Mathf.Max(minSamples, Mathf.RoundToInt(totalPathLength * samplesPerUnit));

        fullPath = new Vector3[sampleCount];

        // for (int i = 0; i < sampleCount; i++)
        // {
        //     float t = i / (float)(sampleCount - 1);

        //     Vector3 localPos = currentSpline.Spline.EvaluatePosition(t);

        //     Vector3 worldPos = currentSpline.transform.TransformPoint(localPos);

        //     fullPath[i] = worldPos + Vector3.up * yOffset;
        // }
        Transform splineTransform = currentSpline.transform;
        Vector3 yOffsetVector = Vector3.up * yOffset; // Cache calculation
        float invSampleCount = 1f / (sampleCount - 1); // Cache division

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i * invSampleCount; // Use multiplication instead of division

            Vector3 localPos = currentSpline.Spline.EvaluatePosition(t);
            Vector3 worldPos = splineTransform.TransformPoint(localPos);

            fullPath[i] = worldPos + yOffsetVector;
        }

        line.positionCount = fullPath.Length;
        line.SetPositions(fullPath);

        LogHelper.Log($"SplinePathLine: Generated path with {sampleCount} samples (length: {totalPathLength:F2})");
    }

    // private int GetClosestPointIndex(Vector3 carPos)
    // {
    //     if (fullPath == null || fullPath.Length == 0)
    //         return 0;

    //     float minDistSqr = float.MaxValue;
    //     int index = 0;

    //     int step = Mathf.Max(1, fullPath.Length / 200);

    //     for (int i = 0; i < fullPath.Length; i += step)
    //     {
    //         float distSqr = (carPos - fullPath[i]).sqrMagnitude;
    //         if (distSqr < minDistSqr)
    //         {
    //             minDistSqr = distSqr;
    //             index = i;
    //         }
    //     }

    //     int searchRange = step * 2;
    //     int startSearch = Mathf.Max(0, index - searchRange);
    //     int endSearch = Mathf.Min(fullPath.Length - 1, index + searchRange);

    //     for (int i = startSearch; i <= endSearch; i++)
    //     {
    //         float distSqr = (carPos - fullPath[i]).sqrMagnitude;
    //         if (distSqr < minDistSqr)
    //         {
    //             minDistSqr = distSqr;
    //             index = i;
    //         }
    //     }

    //     return index;
    // }

    private int GetClosestPointIndex(Vector3 carPos)
    {
        if (fullPath == null || fullPath.Length == 0)
            return 0;

        framesSinceLastSearch++;

        // Only do full search every 3 frames, otherwise search near last position
        if (framesSinceLastSearch < 3)
        {
            // Search only forward from last position (car moves forward)
            int localSearchRange = 20;
            int localStartSearch = Mathf.Max(0, lastClosestIndex - 5);
            int localEndSearch = Mathf.Min(fullPath.Length - 1, lastClosestIndex + localSearchRange);

            float localMinDistSqr = float.MaxValue;
            int localIndex = lastClosestIndex;

            for (int i = localStartSearch; i <= localEndSearch; i++)
            {
                float distSqr = (carPos - fullPath[i]).sqrMagnitude;
                if (distSqr < localMinDistSqr)
                {
                    localMinDistSqr = distSqr;
                    localIndex = i;
                }
            }

            lastClosestIndex = localIndex;
            return localIndex;
        }

        // Full search every 3rd frame
        framesSinceLastSearch = 0;

        float minDistSqr = float.MaxValue;
        int index = 0;

        int step = Mathf.Max(1, fullPath.Length / 200);

        for (int i = 0; i < fullPath.Length; i += step)
        {
            float distSqr = (carPos - fullPath[i]).sqrMagnitude;
            if (distSqr < minDistSqr)
            {
                minDistSqr = distSqr;
                index = i;
            }
        }

        int searchRange = step * 2;
        int startSearch = Mathf.Max(0, index - searchRange);
        int endSearch = Mathf.Min(fullPath.Length - 1, index + searchRange);

        for (int i = startSearch; i <= endSearch; i++)
        {
            float distSqr = (carPos - fullPath[i]).sqrMagnitude;
            if (distSqr < minDistSqr)
            {
                minDistSqr = distSqr;
                index = i;
            }
        }

        lastClosestIndex = index;
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
        lastClosestIndex = 0;
        framesSinceLastSearch = 0;
        lastVisibleCount = 0;
    }

    public float GetPathProgress(Vector3 position)
    {
        if (fullPath == null || fullPath.Length == 0)
            return 0f;

        int closestIndex = GetClosestPointIndex(position);
        return closestIndex / (float)fullPath.Length;
    }
}