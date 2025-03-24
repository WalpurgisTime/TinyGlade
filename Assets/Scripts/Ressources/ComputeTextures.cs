
using UnityEngine;


public class ComputeTexture
{
    public ComputeShader ComputeProgram { get; private set; }
    public RenderTexture RenderTex { get; private set; }

    public static ComputeTexture Init(string computeShaderPath, ShaderWatcher shaderwatch, ShaderLibrary shaderLibrary)
    {
        return new ComputeTexture(computeShaderPath, shaderwatch, shaderLibrary);
    }

    public ComputeTexture(string computeShaderPath, ShaderWatcher shaderwatch, ShaderLibrary shaderLibrary)
    {
        ComputeProgram = Resources.Load<ComputeShader>(computeShaderPath);
        if (ComputeProgram == null)
        {
            Debug.LogError($"ComputeTexture: Impossible de charger {computeShaderPath}");
            return;
        }

        RenderTex = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGBFloat)
        {
            enableRandomWrite = true
        };
        RenderTex.Create();

        // Ajouter le Compute Shader à la bibliothèque
        shaderLibrary.AddComputeShader(computeShaderPath, ComputeProgram);
        shaderwatch.OnShaderChanged += (path) =>
        {
            if (path.Contains(computeShaderPath))
            {
                Debug.Log($"ComputeTexture: Shader {computeShaderPath} mis à jour !");
                ComputeProgram = Resources.Load<ComputeShader>(computeShaderPath);
            }
        };
    }

    public void Dispatch()
    {
        if (ComputeProgram == null) return;

        int kernelHandle = ComputeProgram.FindKernel("CSMain");
        ComputeProgram.SetTexture(kernelHandle, "Result", RenderTex);
        ComputeProgram.Dispatch(kernelHandle, 512 / 8, 512 / 8, 1);
    }
}

