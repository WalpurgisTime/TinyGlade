using UnityEngine;

public class ShaderProgram : MonoBehaviour
{
    public ComputeShader computeShader;
    private RenderTexture renderTexture;

    void Start()
    {
        InitRenderTexture();
        DispatchShader();
    }

    void InitRenderTexture()
    {
        renderTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGBFloat);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
    }

    public void DispatchShader()
    {
        int kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetTexture(kernelHandle, "Result", renderTexture);
        computeShader.Dispatch(kernelHandle, renderTexture.width / 8, renderTexture.height / 8, 1);
    }

    public void SetUniform(string name, float value)
    {
        computeShader.SetFloat(name, value);
    }

    public void SetUniform(string name, Vector4 value)
    {
        computeShader.SetVector(name, value);
    }

    public RenderTexture GetRenderTexture()
    {
        return renderTexture;
    }
}
