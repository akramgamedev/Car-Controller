using System.Collections.Generic;
using UnityEngine;

public class SkidMarkManager : MonoBehaviour
{
    [Header("Skid Mark Settings")]
    public Material skidMarkMaterial;
    public float markWidth = 0.3f;
    public float minDistance = 0.1f;
    public int maxMarks = 1024;
    public float groundOffset = 0.02f;
    public Color skidColor = new Color(0.1f, 0.1f, 0.1f, 1f);

    [Header("Fade Settings")]
    public bool fadeMarks = true;
    public float fadeDelay = 5f;
    public float fadeDuration = 2f;

    private struct MarkSection
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector4 tangent;
        public float intensity;
        public int lastIndex;
        public float timestamp;
    }

    private List<MarkSection> marks = new List<MarkSection>();
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private int markIndex = 0;
    private bool updated = false;


    void Awake()
    {
        GameObject skidObject = new GameObject("SkidMarks");
        skidObject.transform.SetParent(transform);
        skidObject.transform.localPosition = Vector3.zero;

        meshFilter = skidObject.AddComponent<MeshFilter>();
        meshRenderer = skidObject.AddComponent<MeshRenderer>();

        mesh = new Mesh();
        mesh.MarkDynamic();
        meshFilter.mesh = mesh;

        if (skidMarkMaterial != null)
        {
            meshRenderer.material = skidMarkMaterial;
        }
        else
        {
            // Create default material if none assigned
            skidMarkMaterial = new Material(Shader.Find("Unlit/Transparent"));
            skidMarkMaterial.color = skidColor;
            meshRenderer.material = skidMarkMaterial;
        }

        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }
    void LateUpdate()
    {
        if (updated && fadeMarks)
        {
            bool needsRebuild = false;
            float currentTime = Time.time;
            
            for (int i = marks.Count - 1; i >= 0; i--)
            {
                MarkSection mark = marks[i];
                float age = currentTime - mark.timestamp;
                
                if (age > fadeDelay + fadeDuration)
                {
                    marks.RemoveAt(i);
                    needsRebuild = true;
                }
                else if (age > fadeDelay)
                {
                    float fade = 1f - ((age - fadeDelay) / fadeDuration);
                    mark.intensity = fade;
                    marks[i] = mark;
                    needsRebuild = true;
                }
            }
            
            if (needsRebuild)
            {
                UpdateMesh();
            }
        }
        updated = false;
    }

    public int AddSkidMark(Vector3 position, Vector3 normal, float intensity, int lastIndex)
    {
        if (intensity <= 0) return -1;

        // Check minimum distance from last mark
        if (lastIndex > 0 && lastIndex < marks.Count)
        {
            float dist = Vector3.Distance(position, marks[lastIndex].position);
            if (dist < minDistance) return lastIndex;
        }

        MarkSection mark = new MarkSection
        {
            position = position + normal * groundOffset,
            normal = normal,
            intensity = Mathf.Clamp01(intensity),
            lastIndex = lastIndex,
            timestamp = Time.time
        };

        // Calculate tangent for mesh generation
        if (lastIndex >= 0 && lastIndex < marks.Count)
        {
            Vector3 dir = (mark.position - marks[lastIndex].position).normalized;
            mark.tangent = new Vector4(dir.x, dir.y, dir.z, 1);
        }
        else
        {
            mark.tangent = new Vector4(1, 0, 0, 1);
        }

        marks.Add(mark);
        markIndex = marks.Count - 1;

        // Remove oldest marks if exceeded max
        if (marks.Count > maxMarks)
        {
            marks.RemoveRange(0, marks.Count - maxMarks);
        }

        updated = true;
        UpdateMesh();
        
        return markIndex;
    }

    void UpdateMesh()
    {
        if (marks.Count < 2)
        {
            mesh.Clear();
            return;
        }

        int segmentCount = 0;
        for (int i = 0; i < marks.Count; i++)
        {
            if (marks[i].lastIndex != -1 && marks[i].lastIndex < marks.Count)
            {
                segmentCount++;
            }
        }

        if (segmentCount == 0)
        {
            mesh.Clear();
            return;
        }

        Vector3[] vertices = new Vector3[segmentCount * 4];
        Vector3[] normals = new Vector3[segmentCount * 4];
        Vector4[] tangents = new Vector4[segmentCount * 4];
        Color[] colors = new Color[segmentCount * 4];
        Vector2[] uvs = new Vector2[segmentCount * 4];
        int[] triangles = new int[segmentCount * 6];

        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < marks.Count; i++)
        {
            MarkSection curr = marks[i];
            
            if (curr.lastIndex == -1 || curr.lastIndex >= marks.Count) continue;
            
            MarkSection last = marks[curr.lastIndex];

            Vector3 dir = (curr.position - last.position).normalized;
            Vector3 right = Vector3.Cross(dir, curr.normal).normalized;

            // Create quad
            vertices[vertIndex] = last.position - right * markWidth * 0.5f;
            vertices[vertIndex + 1] = last.position + right * markWidth * 0.5f;
            vertices[vertIndex + 2] = curr.position - right * markWidth * 0.5f;
            vertices[vertIndex + 3] = curr.position + right * markWidth * 0.5f;

            // Normals
            normals[vertIndex] = last.normal;
            normals[vertIndex + 1] = last.normal;
            normals[vertIndex + 2] = curr.normal;
            normals[vertIndex + 3] = curr.normal;

            // Tangents
            tangents[vertIndex] = last.tangent;
            tangents[vertIndex + 1] = last.tangent;
            tangents[vertIndex + 2] = curr.tangent;
            tangents[vertIndex + 3] = curr.tangent;

            // Colors (for fading)
            Color c = new Color(1, 1, 1, curr.intensity);
            colors[vertIndex] = c;
            colors[vertIndex + 1] = c;
            colors[vertIndex + 2] = c;
            colors[vertIndex + 3] = c;

            // UVs
            uvs[vertIndex] = new Vector2(0, 0);
            uvs[vertIndex + 1] = new Vector2(1, 0);
            uvs[vertIndex + 2] = new Vector2(0, 1);
            uvs[vertIndex + 3] = new Vector2(1, 1);

            // Triangles
            triangles[triIndex] = vertIndex;
            triangles[triIndex + 1] = vertIndex + 2;
            triangles[triIndex + 2] = vertIndex + 1;
            triangles[triIndex + 3] = vertIndex + 2;
            triangles[triIndex + 4] = vertIndex + 3;
            triangles[triIndex + 5] = vertIndex + 1;

            vertIndex += 4;
            triIndex += 6;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.tangents = tangents;
        mesh.colors = colors;
        mesh.uv = uvs;
        mesh.triangles = triangles;
    }

    public void ClearMarks()
    {
        marks.Clear();
        markIndex = 0;
        mesh.Clear();
    }
}

