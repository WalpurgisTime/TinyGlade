using System.Collections.Generic;
using UnityEngine;

/// Simule GLVertexArray (wrapper Mesh ID ou structure d'encapsulation)
public struct GLVertexArray
{
    public Mesh mesh;
    public GLVertexArray(Mesh mesh) => this.mesh = mesh;
}

/// Simule GLVertexBuffer (pas utilisé en Unity, mais gardé pour compat)
public struct GLVertexBuffer
{
    public string name; // attribut (ex: "position", "normal", etc.)
    public GLVertexBuffer(string name) => this.name = name;
}

/// Adaptation du VAO Rust vers Unity
public class VAO
{
    public GLVertexArray id;
    public int indices_count;
    public List<GLVertexBuffer> vertex_buffer_ids;

    public Mesh unity_mesh => id.mesh;

    public int id_() => unity_mesh.GetInstanceID();

    public static VAO new_(Mesh mesh)
    {
        var (vao, buffers) = build_vao(mesh);
        return new VAO
        {
            id = vao,
            vertex_buffer_ids = buffers,
            indices_count = (int)mesh.GetIndexCount(0)
        };
    }

    public void rebuild(Mesh mesh)
    {
        clear();
        var (vao, buffers) = build_vao(mesh);
        id = vao;
        vertex_buffer_ids = buffers;
        indices_count = (int)mesh.GetIndexCount(0);
    }


    public void clear()
    {
        if (id.mesh != null)
        {
            Object.Destroy(id.mesh);
        }
        id = default;
        vertex_buffer_ids?.Clear();
    }

    /// Équiv. Rust: build_vao(mesh, shader_program_id)
    public static (GLVertexArray, List<GLVertexBuffer>) build_vao(Mesh sourceMesh)
    {
        var mesh = Object.Instantiate(sourceMesh); // copie isolée
        var buffers = new List<GLVertexBuffer>();

        // Détection "automatique" des attributs présents dans le mesh
        if (mesh.vertices.Length > 0)
            buffers.Add(new GLVertexBuffer("position"));
        if (mesh.normals.Length > 0)
            buffers.Add(new GLVertexBuffer("normal"));
        if (mesh.tangents.Length > 0)
            buffers.Add(new GLVertexBuffer("tangent"));
        if (mesh.uv.Length > 0)
            buffers.Add(new GLVertexBuffer("uv"));
        if (mesh.colors.Length > 0)
            buffers.Add(new GLVertexBuffer("color"));

        return (new GLVertexArray(mesh), buffers);
    }
}
