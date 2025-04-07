using System.Collections.Generic;
using UnityEngine;

public class VAOLibrary
{
    private Dictionary<(Mesh, ShaderProgram), VAO> vaoMap = new();
    private Dictionary<Mesh, List<ShaderProgram>> meshToShaders = new();

    // Récupère un VAO existant
    public VAO Get(Mesh mesh, ShaderProgram shader)
    {
        vaoMap.TryGetValue((mesh, shader), out var result);
        return result;
    }

    // Vérifie si un VAO existe déjà pour un mesh donné
    public bool HasVAO(Mesh mesh)
    {
        return meshToShaders.ContainsKey(mesh);
    }

    // Ajoute un nouveau VAO si besoin
    public VAO Add(Mesh mesh, ShaderProgram shader)
    {
        if (vaoMap.TryGetValue((mesh, shader), out var existingVAO))
        {
            return existingVAO;
        }

        if (!IsValid(mesh) || shader == null || shader.Material == null)
        {
            Debug.LogWarning("[VAOLibrary] Invalid mesh or shader");
            return null;
        }

        var newVao = VAO.new_(mesh);
        vaoMap[(mesh, shader)] = newVao;

        if (!meshToShaders.ContainsKey(mesh))
            meshToShaders[mesh] = new List<ShaderProgram>();
        meshToShaders[mesh].Add(shader);

        return newVao;
    }

    // Rebuild tous les VAO liés à un Mesh (ex: si Mesh modifié)
    public void RebuildVAO(Mesh mesh)
    {
        if (!meshToShaders.TryGetValue(mesh, out var shaders))
        {
            Debug.LogWarning("[VAOLibrary] No VAO found for this mesh");
            return;
        }

        foreach (var shader in shaders)
        {
            if (vaoMap.TryGetValue((mesh, shader), out var vao))
            {
                vao.rebuild(mesh);
            }
        }
    }

    public void Remove(Mesh mesh)
    {
        if (!meshToShaders.TryGetValue(mesh, out var shaders)) return;

        foreach (var shader in shaders)
        {
            vaoMap.Remove((mesh, shader));
        }

        meshToShaders.Remove(mesh);
    }

    private static bool IsValid(Mesh mesh)
    {
        return mesh != null && mesh.vertexCount > 0 && mesh.GetIndices(0).Length > 0;
    }
}
