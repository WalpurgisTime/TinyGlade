using UnityEngine;

public struct PlaneMesh
{
    public float Size { get; private set; }

    public PlaneMesh(float size = 1.0f)
    {
        Size = size;
    }

    public static PlaneMesh Default => new PlaneMesh(1.0f);

    public static Mesh FromPlane(PlaneMesh plane)
    {
        float extent = plane.Size / 2.0f;

        Vector3[] vertices = new Vector3[]
        {
            new Vector3( extent, 0.0f, -extent),
            new Vector3( extent, 0.0f,  extent),
            new Vector3(-extent, 0.0f,  extent),
            new Vector3(-extent, 0.0f, -extent)
        };

        Vector3[] normals = new Vector3[]
        {
            Vector3.up, Vector3.up, Vector3.up, Vector3.up
        };

        Vector2[] uvs = new Vector2[]
        {
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 1.0f)
        };

        int[] indices = new int[]
        {
            0, 2, 1,
            0, 3, 2
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = indices;

        return mesh;
    }
}



/*
using System;
using System.Collections.Generic;


public struct Plane
{
    /// <summary>
    /// The total side length of the square.
    /// </summary>
    public float Size;

    public Plane(float size)
    {
        Size = size;
    }

    public static Plane Default => new Plane(1.0f);

    public Mesh ToMesh()
    {
        float extent = Size / 2.0f;

        Vector3[] positions = new Vector3[]
        {
            new Vector3(extent, 0.0f, -extent),
            new Vector3(extent, 0.0f, extent),
            new Vector3(-extent, 0.0f, extent),
            new Vector3(-extent, 0.0f, -extent)
        };

        Vector3 normal = new Vector3(0.0f, 1.0f, 0.0f);

        Vector3[] normals = new Vector3[]
        {
            normal, normal, normal, normal
        };

        Vector2[] uvs = new Vector2[]
        {
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 1.0f)
        };

        int[] indices = { 0, 2, 1, 0, 3, 2 };

        Mesh mesh = new Mesh();
        mesh.SetIndices(indices);
        mesh.SetAttribute(Mesh.AttributePosition, positions);
        mesh.SetAttribute(Mesh.AttributeNormal, normals);
        mesh.SetAttribute(Mesh.AttributeUV, uvs);
        mesh.SetAttribute(Mesh.AttributeColor, new Vector3[] { Vector3.One, Vector3.One, Vector3.One, Vector3.One });

        return mesh;
    }
}

*/