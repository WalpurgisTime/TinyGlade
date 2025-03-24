using System.Collections.Generic;
using UnityEngine;

public class ShaderLibrary : MonoBehaviour
{
    private Dictionary<string, Shader> shadersByName = new Dictionary<string, Shader>();
    private Dictionary<int, Shader> shadersByHandle = new Dictionary<int, Shader>();
    private Dictionary<string, ComputeShader> computeShadersByName = new Dictionary<string, ComputeShader>();
    private Dictionary<int, ComputeShader> computeShadersByHandle = new Dictionary<int, ComputeShader>();
    private int handleCounter = 0; // Génère un ID unique

    public int AddShader(string name, Shader shader)
    {
        if (shadersByName.ContainsKey(name))
        {
            Debug.LogError($"ShaderLibrary: Le shader '{name}' existe déjà !");
            return -1;
        }

        int handle = ++handleCounter;
        shadersByName[name] = shader;
        shadersByHandle[handle] = shader;

        //Debug.Log($"Shader ajouté: {name} (Handle: {handle})");
        return handle;
    }

    public int AddComputeShader(string name, ComputeShader computeShader)
    {
        if (computeShadersByName.ContainsKey(name))
        {
            Debug.LogError($"ShaderLibrary: Le Compute Shader '{name}' existe déjà !");
            return -1;
        }

        int handle = ++handleCounter;
        computeShadersByName[name] = computeShader;
        computeShadersByHandle[handle] = computeShader;

        //Debug.Log($"Compute Shader ajouté: {name} (Handle: {handle})");
        return handle;
    }

    public Shader GetShaderByName(string name)
    {
        if (shadersByName.TryGetValue(name, out Shader shader))
        {
            return shader;
        }
        Debug.LogWarning($"Shader '{name}' introuvable !");
        return null;
    }

    public ComputeShader GetComputeShaderByName(string name)
    {
        if (computeShadersByName.TryGetValue(name, out ComputeShader computeShader))
        {
            return computeShader;
        }
        Debug.LogWarning($"Compute Shader '{name}' introuvable !");
        return null;
    }

    public Shader GetShaderByHandle(int handle)
    {
        if (shadersByHandle.TryGetValue(handle, out Shader shader))
        {
            return shader;
        }
        Debug.LogWarning($"Shader avec Handle '{handle}' introuvable !");
        return null;
    }

    public ComputeShader GetComputeShaderByHandle(int handle)
    {
        if (computeShadersByHandle.TryGetValue(handle, out ComputeShader computeShader))
        {
            return computeShader;
        }
        Debug.LogWarning($"Compute Shader avec Handle '{handle}' introuvable !");
        return null;
    }

    public bool RemoveShader(string name)
    {
        if (shadersByName.TryGetValue(name, out Shader shader))
        {
            shadersByName.Remove(name);
            shadersByHandle.Remove(handleCounter); // Suppression via handle
            return true;
        }
        return false;
    }

    public bool RemoveComputeShader(string name)
    {
        if (computeShadersByName.TryGetValue(name, out ComputeShader computeShader))
        {
            computeShadersByName.Remove(name);
            computeShadersByHandle.Remove(handleCounter); // Suppression via handle
            return true;
        }
        return false;
    }

    private Dictionary<string, Material> materialsByName = new Dictionary<string, Material>();

    public Material GetMaterial(string shaderName)
    {
        if (materialsByName.TryGetValue(shaderName, out Material existingMaterial))
        {
            return existingMaterial;
        }

        Shader shader = GetShaderByName(shaderName);
        if (shader == null)
        {
            Debug.LogError($"Shader '{shaderName}' introuvable. Impossible de créer le matériau.");
            return null;
        }

        Material newMaterial = new Material(shader);
        materialsByName[shaderName] = newMaterial;
        return newMaterial;
    }



}
