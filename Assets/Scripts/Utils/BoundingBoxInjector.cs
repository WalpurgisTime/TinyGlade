using UnityEngine;
using System.Collections.Generic;

public static class BoundingBoxInjector
{
    public static void ApplyBoundingBoxes(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int vertexCount = vertices.Length;

        List<Vector4> bbxBounds = new List<Vector4>(vertexCount);

        // Injecter un bounding box unique pour chaque quad (4 sommets)
        for (int i = 0; i < vertexCount; i += 4)
        {
            if (i + 3 >= vertexCount) break;

            Vector3 p0 = vertices[i];
            Vector3 p1 = vertices[i + 1];
            Vector3 p2 = vertices[i + 2];
            Vector3 p3 = vertices[i + 3];

            float minX = Mathf.Min(p0.x, p1.x, p2.x, p3.x);
            float minZ = Mathf.Min(p0.z, p1.z, p2.z, p3.z);
            float maxX = Mathf.Max(p0.x, p1.x, p2.x, p3.x);
            float maxZ = Mathf.Max(p0.z, p1.z, p2.z, p3.z);

            Vector4 bbx = new Vector4(minX, minZ, maxX, maxZ);

            for (int j = 0; j < 4; j++)
                bbxBounds.Add(bbx);
        }

        // Compléter si nombre de sommets non multiple de 4
        while (bbxBounds.Count < vertexCount)
            bbxBounds.Add(Vector4.zero);

        mesh.SetUVs(1, bbxBounds);
    }
}
