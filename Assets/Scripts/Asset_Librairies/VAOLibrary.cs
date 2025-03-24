using UnityEngine;
using System.Collections.Generic;

public class VAOLibrary
{
    private Dictionary<Handle<VAO>, VAO> vao;
    private Dictionary<(Handle<Mesh>, Handle<ShaderProgram>), Handle<VAO>> byMeshAndShader;
    private Dictionary<Handle<Mesh>, List<(Handle<Mesh>, Handle<ShaderProgram>)>> byMesh;

    public VAOLibrary()
    {
        vao = new Dictionary<Handle<VAO>, VAO>();
        byMeshAndShader = new Dictionary<(Handle<Mesh>, Handle<ShaderProgram>), Handle<VAO>>();
        byMesh = new Dictionary<Handle<Mesh>, List<(Handle<Mesh>, Handle<ShaderProgram>)>>();
    }

    public VAO Get(Handle<VAO> vaoHandle)
    {
        return vao.TryGetValue(vaoHandle, out VAO value) ? value : null;
    }

    public Handle<VAO> Add(MeshLibrary meshLibrary, ShaderLibrary shaderLibrary, Handle<Mesh> meshHandle, Handle<ShaderProgram> shaderHandle)
    {
        if (byMeshAndShader.TryGetValue((meshHandle, shaderHandle), out Handle<VAO> existingHandle))
        {
            return existingHandle;
        }
        else
        {
            Handle<VAO> vaoHandle = new Handle<VAO>(HandleId.Random());
            VAO newVAO = BuildVAO(meshLibrary, shaderLibrary, meshHandle, shaderHandle);
            vao[vaoHandle] = newVAO;
            byMeshAndShader[(meshHandle, shaderHandle)] = vaoHandle;

            if (!byMesh.ContainsKey(meshHandle))
            {
                byMesh[meshHandle] = new List<(Handle<Mesh>, Handle<ShaderProgram>)>();
            }
            byMesh[meshHandle].Add((meshHandle, shaderHandle));

            return vaoHandle;
        }
    }

    public bool HasVAO(Handle<Mesh> meshHandle)
    {
        return byMesh.ContainsKey(meshHandle);
    }

    private VAO BuildVAO(MeshLibrary meshLibrary, ShaderLibrary shaderLibrary, Handle<Mesh> meshHandle, Handle<ShaderProgram> shaderHandle)
    {
        Mesh mesh = meshLibrary.GetMesh(meshHandle.Id.Id.GetHashCode());
        if (mesh != null && IsValid(mesh))
        {
            Shader shader = shaderLibrary.GetShaderByHandle(shaderHandle.Id.Id.GetHashCode());
            if (shader != null)
            {
                return new VAO(mesh, shader);
            }
            else
            {
                throw new System.Exception("Shader is not in the shader library");
            }
        }
        else
        {
            throw new System.Exception("Mesh is invalid");
        }
    }

    private static bool IsValid(Mesh mesh)
    {
        return mesh != null && mesh.vertexCount > 0 && mesh.GetIndices(0).Length > 0;
    }


    public void RebuildVAO(MeshLibrary meshLibrary, ShaderLibrary shaderLibrary, Handle<Mesh> meshHandle)
    {
        if (!byMesh.TryGetValue(meshHandle, out List<(Handle<Mesh>, Handle<ShaderProgram>)> handles))
        {
            throw new System.Exception("No VAOs match the Mesh Handle");
        }

        foreach ((Handle<Mesh> meshH, Handle<ShaderProgram> shaderH) in handles)
        {
            Mesh mesh = meshLibrary.GetMesh(meshH.Id.Id.GetHashCode());
            if (mesh != null && IsValid(mesh))
            {
                Shader shader = shaderLibrary.GetShaderByHandle(shaderH.Id.Id.GetHashCode());
                if (shader == null)
                {
                    throw new System.Exception("Shader is not in the shader library");
                }

                if (byMeshAndShader.TryGetValue((meshH, shaderH), out Handle<VAO> vaoHandle))
                {
                    if (vao.TryGetValue(vaoHandle, out VAO vaoObject))
                    {
                        vaoObject.Rebuild(mesh, shader);
                    }
                    else
                    {
                        throw new System.Exception("VAO handle is invalid");
                    }
                }
            }
            else
            {
                throw new System.Exception("VAO couldn't be rebuilt: Mesh is invalid");
            }
        }
    }

    public void Remove(Handle<Mesh> meshHandle)
    {
        if (byMesh.TryGetValue(meshHandle, out List<(Handle<Mesh>, Handle<ShaderProgram>)> affectedHandles))
        {
            foreach (var meshShaderH in affectedHandles)
            {
                if (byMeshAndShader.TryGetValue(meshShaderH, out Handle<VAO> vaoHandle))
                {
                    vao.Remove(vaoHandle);
                    byMeshAndShader.Remove(meshShaderH);
                }
            }
            byMesh.Remove(meshHandle);
        }
    }
}
