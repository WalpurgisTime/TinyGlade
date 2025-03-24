using UnityEngine;
using System.Collections.Generic;

public class VAO
{
    private Mesh mesh;
    private Material material;
    private int indicesCount;

    public VAO(Mesh mesh, Shader shader)
    {
        this.mesh = mesh;
        this.material = new Material(shader);
        this.indicesCount = mesh.GetIndices(0).Length;
    }

    public void Rebuild(Mesh newMesh, Shader newShader)
    {
        this.mesh = newMesh;
        this.material.shader = newShader;
        this.indicesCount = newMesh.GetIndices(0).Length;
    }

    public void Render()
    {
        Graphics.DrawMesh(mesh, Matrix4x4.identity, material, 0);
    }
}