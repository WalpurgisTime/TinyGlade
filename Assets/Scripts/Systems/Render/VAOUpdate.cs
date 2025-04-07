using System.Collections.Generic;
using UnityEngine;

public class VAOUpdate : MonoBehaviour
{
    public MeshLibrary meshLibrary;
    public ShaderLibrary shaderLibrary;
    public VAOLibrary vaoLibrary;

    public void BuildMissingVAOs()
    {
       
        var allRenderEntries = Object.FindObjectsByType<RenderEntry>(FindObjectsSortMode.None);

        foreach (var entry in allRenderEntries)
        {

            // Si d�j� un VAO, on skip
            if (entry.vao != null)
                continue;

            // R�cup�re le mesh
            //Debug.Log($"[VAOUpdate] entry = {entry.name}, meshName = {entry.meshName ?? "NULL"}");
            var mesh = meshLibrary.GetMeshByName(entry.meshName);
            if (mesh == null || mesh.vertexCount == 0)
            {
                //Debug.Log($"[VAOBuilder] Skipped empty mesh: {entry.meshName}");
                continue;
            }

         
            // R�cup�re le shader
            var shader = shaderLibrary.GetShaderByName(entry.shaderName);
            if (shader == null)
            {
                //Debug.LogWarning($"[VAOBuilder] Shader not found: {entry.shaderName}");
                continue;
            }

            // Cr�e un nouveau VAO
            var vao = VAO.new_(mesh);
            entry.vao = vao;
            entry.Change();
            //Debug.Log($"[VAOBuilder] VAO created for {entry.vao.unity_mesh}");
        }
    }

    public void RebuildVAOs()
    {
        var dirtyMeshes = meshLibrary.markedAsDirty;
        if (dirtyMeshes.Count == 0)
            return;

        List<Mesh> stillDirty = new();
        

        foreach (var meshName in dirtyMeshes)
        {
            var vao = vaoLibrary.HasVAO(meshName);
            if (vao != null)
            {
                var mesh = meshLibrary.GetMesh(meshName);
                var shader = shaderLibrary.GetShaderByName("default_shader"); // ou li� dynamiquement

                if (mesh != null && shader != null)
                {
                    vaoLibrary.RebuildVAO(mesh); // ou .rebuild(mesh, shader)
                    Debug.Log($"[VAOBuilder] VAO rebuilt for {meshName}");
                }
                else
                {
                    Debug.LogWarning($"[VAOBuilder] Rebuild failed for {meshName}");
                    stillDirty.Add(meshName);
                }
            }
            else
            {
                Debug.LogWarning($"[VAOBuilder] No VAO exists for dirty mesh {meshName}");
            }
        }

        meshLibrary.markedAsDirty = stillDirty;
    }
}
