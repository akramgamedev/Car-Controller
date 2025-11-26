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

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)(sampleCount - 1); // Normalize to 0-1

            Vector3 localPos = currentSpline.Spline.EvaluatePosition(t);

            Vector3 worldPos = currentSpline.transform.TransformPoint(localPos);

            fullPath[i] = worldPos + Vector3.up * yOffset;
        }

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

    public float GetPathProgress(Vector3 position)
    {
        if (fullPath == null || fullPath.Length == 0)
            return 0f;

        int closestIndex = GetClosestPointIndex(position);
        return closestIndex / (float)fullPath.Length;
    }
}