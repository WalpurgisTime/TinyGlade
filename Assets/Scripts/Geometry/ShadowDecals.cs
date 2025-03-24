using System.Collections.Generic;
using UnityEngine;

public class ShadowDecal : MonoBehaviour
{
    public const float OFFSET_FROM_GROUND = 0.001f;
    public const float SHADOW_WIDTH = 0.5f;
    public const int SHADOW_CAP_STEPS = 10;

    public static GameObject New(Curve curve, MeshLibrary meshAssets, ShaderLibrary shaderAssets)
    {
        var mesh = new Mesh();
        bool success = Update(curve, mesh);

        if (!success)
        {
            Debug.LogError("Shadow Decal: couldn't update shadow mesh");
            return null;
        }

        var shadowGO = new GameObject("ShadowDecal");
        var meshFilter = shadowGO.AddComponent<MeshFilter>();
        var meshRenderer = shadowGO.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;
        Shader shader = shaderAssets.GetShaderByName("shadow_shader");

        if (shader == null)
        {
            Debug.LogError("[ShadowDecal] ? Shader 'shadow_shader' not found in ShaderLibrary. Make sure it's assigned.");
            return null;
        }

        meshRenderer.material = new Material(shader);


        // Pour marquer comme "Transient"
        shadowGO.AddComponent<TransientMesh>();

        return shadowGO;
    }

    public static bool Update(Curve curve, Mesh mesh)
    {
        var curvePts = curve.points;
        if (curvePts == null || curvePts.Count < 2)
            return false;

        List<Vector3> offsetPts = new List<Vector3>();
        for (int i = 0; i < curvePts.Count; i++)
        {
            Vector3 thisPt = curvePts[i];
            Vector3 nextPt = (i + 1 < curvePts.Count) ? curvePts[i + 1] : curvePts[i - 1];
            Vector3 tangent = (nextPt - thisPt).normalized;
            Vector3 offset = Vector3.Cross(tangent, Vector3.up) * SHADOW_WIDTH;
            offsetPts.Add(offset);
        }

        List<int> indices = new List<int>();
        List<Vector3> positions = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        for (int quad = 1; quad < curvePts.Count - 2; quad++)
        {
            Vector3 start = curvePts[quad];
            Vector3 end = curvePts[quad + 1];
            Vector3 l_start = start - offsetPts[quad];
            Vector3 l_end = end - offsetPts[quad + 1];
            Vector3 r_start = start + offsetPts[quad];
            Vector3 r_end = end + offsetPts[quad + 1];

            int baseIndex = positions.Count;

            positions.Add(start + Vector3.up * OFFSET_FROM_GROUND); // 0
            positions.Add(end + Vector3.up * OFFSET_FROM_GROUND);   // 1
            positions.Add(l_start + Vector3.up * OFFSET_FROM_GROUND); // 2
            positions.Add(l_end + Vector3.up * OFFSET_FROM_GROUND);   // 3
            positions.Add(r_start + Vector3.up * OFFSET_FROM_GROUND); // 4
            positions.Add(r_end + Vector3.up * OFFSET_FROM_GROUND);   // 5

            uvs.AddRange(new[] {
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 1), new Vector2(1, 1),
            });

            indices.AddRange(new int[] {
                baseIndex + 0, baseIndex + 1, baseIndex + 2,
                baseIndex + 1, baseIndex + 3, baseIndex + 2,
                baseIndex + 0, baseIndex + 4, baseIndex + 1,
                baseIndex + 4, baseIndex + 5, baseIndex + 1,
            });
        }

        // Shadow caps
        List<(Vector3, Vector3)> caps = new();
        if (curvePts.Count == 2)
        {
            Vector3 mid = (curvePts[0] + curvePts[1]) / 2f;
            Vector3 t1 = (curvePts[0] - curvePts[1]).normalized;
            Vector3 t2 = (curvePts[1] - curvePts[0]).normalized;
            caps.Add((mid, t1));
            caps.Add((mid, t2));
        }
        else
        {
            int startIndex = 1;
            int endIndex = curvePts.Count - 2;
            Vector3 t_start = (curvePts[startIndex + 1] - curvePts[startIndex]).normalized;
            Vector3 t_end = -(curvePts[endIndex + 1] - curvePts[endIndex]).normalized;
            caps.Add((curvePts[startIndex], t_start));
            caps.Add((curvePts[endIndex], t_end));
        }

        foreach (var (position, tangent) in caps)
        {
            AddCap(position, tangent, positions, uvs, indices);
        }

        // Fill mesh
        mesh.Clear();
        mesh.SetVertices(positions);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateNormals();

        return true;
    }

    private static void AddCap(
        Vector3 position,
        Vector3 tangent,
        List<Vector3> positions,
        List<Vector2> uvs,
        List<int> indices)
    {
        Vector3 offsetDir = Vector3.Cross(tangent, Vector3.up) * SHADOW_WIDTH;
        List<Vector3> capPoints = new();
        List<Vector2> capUvs = new();

        for (int s = 0; s < SHADOW_CAP_STEPS; s++)
        {
            float t = s / (float)(SHADOW_CAP_STEPS - 1);
            float angle = -Mathf.PI * t;
            Quaternion rot = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);
            Vector3 p = position + rot * offsetDir;
            capPoints.Add(p + Vector3.up * OFFSET_FROM_GROUND);
            capUvs.Add(new Vector2(t, 1.0f));
        }

        int startIndex = positions.Count;
        positions.Add(position + Vector3.up * OFFSET_FROM_GROUND);
        uvs.Add(Vector2.zero);

        positions.AddRange(capPoints);
        uvs.AddRange(capUvs);

        for (int i = 1; i < SHADOW_CAP_STEPS; i++)
        {
            indices.Add(startIndex + 0);
            indices.Add(startIndex + i);
            indices.Add(startIndex + i + 1);
        }
    }
}

/*
using System;
using System.Collections.Generic;
using UnityEngine;

public class ShadowDecal
{
    private const float OffsetFromGround = 0.001f;
    private const float ShadowWidth = 0.5f;
    private const int ShadowCapSteps = 10;

    public static Mesh CreateShadowMesh(Curve curve)
    {
        Mesh mesh = new Mesh();
        UpdateShadowMesh(curve, mesh);
        return mesh;
    }

    public static void UpdateShadowMesh(Curve curve, Mesh mesh)
    {
        List<Vector3> curvePoints = curve.points;
        List<Vector3> offsetPoints = new List<Vector3>();

        for (int i = 0; i < curvePoints.Count; i++)
        {
            Vector3 thisPoint = curvePoints[i];
            Vector3 nextPoint = (i + 1 < curvePoints.Count) ? curvePoints[i + 1] : curvePoints[i - 1];
            Vector3 tangent = (nextPoint - thisPoint).normalized;
            offsetPoints.Add(Vector3.Cross(tangent, Vector3.up) * ShadowWidth);
        }

        List<int> indices = new List<int>();
        List<Vector3> positions = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 1; i < curvePoints.Count - 2; i++)
        {
            Vector3 start = curvePoints[i];
            Vector3 lStart = start - offsetPoints[i];
            Vector3 rStart = start + offsetPoints[i];
            Vector3 end = curvePoints[i + 1];
            Vector3 lEnd = end - offsetPoints[i + 1];
            Vector3 rEnd = end + offsetPoints[i + 1];

            int baseIndex = positions.Count;
            indices.AddRange(new int[] { 0, 1, 2, 1, 3, 2, 0, 4, 1, 4, 5, 1 });

            positions.AddRange(new Vector3[]
            {
                new Vector3(start.x, start.y + OffsetFromGround, start.z),
                new Vector3(end.x, end.y + OffsetFromGround, end.z),
                new Vector3(lStart.x, lStart.y + OffsetFromGround, lStart.z),
                new Vector3(lEnd.x, lEnd.y + OffsetFromGround, lEnd.z),
                new Vector3(rStart.x, rStart.y + OffsetFromGround, rStart.z),
                new Vector3(rEnd.x, rEnd.y + OffsetFromGround, rEnd.z),
            });

            uvs.AddRange(new Vector2[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
            });

            for (int j = 0; j < indices.Count; j++)
            {
                indices[j] += baseIndex;
            }
        }

        /*
        mesh.vertices = positions.ToArray();
        mesh.normals = new Vector3[positions.Count];
        mesh.uv = uvs.ToArray();
        mesh.triangles = indices.ToArray();
        
    }

    private static void AddCap(Vector3 position, Vector3 tangent, List<int> indices, List<Vector3> positions, List<Vector2> uvs)
    {
        Vector3 offsetDir = Vector3.Cross(tangent, Vector3.up) * ShadowWidth;
        List<Vector3> capPositions = new List<Vector3>();

        for (int s = 0; s < ShadowCapSteps; s++)
        {
            float t = s / (float)(ShadowCapSteps - 1);
            Quaternion rot = Quaternion.AngleAxis(-180f * t, Vector3.up);
            capPositions.Add(position + rot * offsetDir);
        }

        List<int> newIndices = new List<int>();
        for (int s = 0; s < ShadowCapSteps - 1; s++)
        {
            newIndices.Add(s + 1);
            newIndices.Add(0);
            newIndices.Add(s + 2);
        }

        int baseIndex = positions.Count;
        for (int i = 0; i < newIndices.Count; i++)
        {
            newIndices[i] += baseIndex;
        }

        indices.AddRange(newIndices);
        positions.Add(new Vector3(position.x, position.y + OffsetFromGround, position.z));
        uvs.Add(new Vector2(0.0f, 0.0f));

        foreach (var capPos in capPositions)
        {
            positions.Add(capPos);
            uvs.Add(new Vector2((positions.Count - baseIndex) / (float)(ShadowCapSteps - 1), 1.0f));
        }
    }
}
*/