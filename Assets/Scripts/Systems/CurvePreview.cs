using System.Collections.Generic;
using UnityEngine;

public class CurvePreview: MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showDebug = true;

    [Header("References")]
    public WallManagerDebugger wallManagerDebugger;
    public Material debugMaterial;

    private Dictionary<int, GameObject> curvePreviewEntities = new();

    private void OnEnable()
    {
        GameEvents.OnCurveChanged.AddListener(OnCurveChanged);
    }

    private void OnDisable()
    {
        GameEvents.OnCurveChanged.RemoveListener(OnCurveChanged);
    }

    public void OnCurveChanged(int curveIndex)
    {
        if (!showDebug || wallManagerDebugger.wallManager == null) return;

        var wall = wallManagerDebugger.wallManager.GetWall(curveIndex);
        if (wall == null || wall.curve == null) return;

        if (curvePreviewEntities.ContainsKey(curveIndex))
        {
            UpdateCurveMesh(curvePreviewEntities[curveIndex], wall.curve);
        }
        else
        {
            GameObject preview = CreateCurvePreviewEntity(wall.curve);
            curvePreviewEntities[curveIndex] = preview;
            wall.curvePreviewEntity = preview;
        }
    }

    private GameObject CreateCurvePreviewEntity(Curve curve)
    {
        GameObject go = new GameObject("CurvePreview");
        go.transform.parent = this.transform;

        var meshFilter = go.AddComponent<MeshFilter>();
        var meshRenderer = go.AddComponent<MeshRenderer>();
        meshRenderer.material = debugMaterial;

        var mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        meshFilter.mesh = mesh;

        UpdateCurveMesh(go, curve);

        return go;
    }

    private void UpdateCurveMesh(GameObject go, Curve curve)
    {
        Mesh mesh = go.GetComponent<MeshFilter>().mesh;

        Vector3[] vertices = new Vector3[curve.points.Count];
        Color[] colors = new Color[curve.points.Count];
        int[] indices = new int[curve.points.Count];

        for (int i = 0; i < curve.points.Count; i++)
        {
            vertices[i] = curve.points[i] + new Vector3(0, 0.01f, 0);
            colors[i] = Color.red;
            indices[i] = i;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
    }
}