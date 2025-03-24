using UnityEngine;
using System.Runtime.InteropServices;

public class ComputeArchesIndirect : MonoBehaviour
{
    public ComputeShader computeShader;
    public Material renderMaterial;

    private ComputeBuffer transformsBuffer;
    private ComputeBuffer drawIndirectCommandBuffer;
    private ComputeBuffer curvesBuffer;
    private ComputeBuffer segmentsBuffer;

    private RenderTexture pathMaskTexture;

    private const int INSTANCE_COUNT = 1000;
    private const int COMMAND_BUFFER_SIZE = 1;

    [StructLayout(LayoutKind.Sequential)]
    public struct DrawElementsIndirectCommand
    {
        public uint count;
        public uint instanceCount;
        public uint firstIndex;
        public uint baseVertex;
        public uint baseInstance;
    }


    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CurveDataSSBO
    {
        public uint pointsCount;
        public uint pad0;
        public uint pad1;
        public uint pad2;

        public fixed float positions[1000 * 4]; // Tableau FIXE pour être blittable

        public static CurveDataSSBO FromCurve(Vector3[] curvePoints)
        {
            CurveDataSSBO data = new CurveDataSSBO
            {
                pointsCount = (uint)curvePoints.Length,
                pad0 = 0,
                pad1 = 0,
                pad2 = 0
            };

            for (int i = 0; i < curvePoints.Length; i++)
            {
                data.positions[i * 4 + 0] = curvePoints[i].x;
                data.positions[i * 4 + 1] = curvePoints[i].y;
                data.positions[i * 4 + 2] = curvePoints[i].z;
                data.positions[i * 4 + 3] = 1.0f; // Coordonnée homogène
            }

            return data;
        }

        public static CurveDataSSBO Empty()
        {
            CurveDataSSBO data = new CurveDataSSBO
            {
                pointsCount = 0,
                pad0 = 0,
                pad1 = 0,
                pad2 = 0
            };

            for (int i = 0; i < 1000 * 4; i++)
            {
                data.positions[i] = 0.0f;
            }

            return data;
        }
    }



public ComputeArchesIndirect(ShaderWatcher shaderwatch, ShaderLibrary shaderLibrary)
    {
        InitComputeShader(shaderwatch, shaderLibrary);
        InitBuffers();
        InitPathMask();
    }

    void InitComputeShader(ShaderWatcher shaderwatch, ShaderLibrary shaderLibrary)
    {


        // Charger le Compute Shader avec `ComputeTexture.Init()`
        ComputeTexture computeTexture = ComputeTexture.Init(
            "shaders/arch_layout_bricks",
            shaderwatch,
            shaderLibrary
        );

        computeShader = computeTexture.ComputeProgram;
    }

    void InitBuffers()
    {
        // Buffer pour transformations
        transformsBuffer = new ComputeBuffer(INSTANCE_COUNT, Marshal.SizeOf(typeof(Matrix4x4)), ComputeBufferType.Structured);

        // Buffer pour le rendu indirect
        drawIndirectCommandBuffer = new ComputeBuffer(COMMAND_BUFFER_SIZE, Marshal.SizeOf(typeof(DrawElementsIndirectCommand)), ComputeBufferType.IndirectArguments);

        // Buffer pour les courbes
        curvesBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(CurveDataSSBO)), ComputeBufferType.Structured);

        // Buffer pour les segments
        segmentsBuffer = new ComputeBuffer(INSTANCE_COUNT, Marshal.SizeOf(typeof(Vector4)), ComputeBufferType.Structured);

        // Initialisation des commandes de rendu
        DrawElementsIndirectCommand[] drawCommand = new DrawElementsIndirectCommand[1];
        drawCommand[0].count = 312;
        drawCommand[0].instanceCount = (uint)INSTANCE_COUNT;
        drawCommand[0].firstIndex = 0;
        drawCommand[0].baseVertex = 0;
        drawCommand[0].baseInstance = 0;

        drawIndirectCommandBuffer.SetData(drawCommand);

        // Initialisation du buffer de courbes avec des valeurs vides
        CurveDataSSBO[] emptyCurve = new CurveDataSSBO[1];
        emptyCurve[0] = CurveDataSSBO.Empty();
        curvesBuffer.SetData(emptyCurve);
    }

    void InitPathMask()
    {
        pathMaskTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32)
        {
            enableRandomWrite = true
        };
        pathMaskTexture.Create();
    }

    public void Bind()
    {
        if (computeShader == null) return;

        int kernelHandle = computeShader.FindKernel("CSMain");

        // Associer les buffers au Compute Shader
        computeShader.SetBuffer(kernelHandle, "transformsBuffer", transformsBuffer);
        computeShader.SetBuffer(kernelHandle, "curvesBuffer", curvesBuffer);
        computeShader.SetBuffer(kernelHandle, "drawCommands", drawIndirectCommandBuffer);
        computeShader.SetBuffer(kernelHandle, "segmentsBuffer", segmentsBuffer);

        // Associer la texture de masque
        computeShader.SetTexture(kernelHandle, "path_mask", pathMaskTexture);
        computeShader.SetVector("path_mask_ws_dims", new Vector2(20.0f, 20.0f)); // Simule `path_mask_ws_dims`
    }

    public void ResetDrawCommandBuffer()
    {
        DrawElementsIndirectCommand[] drawCommand = new DrawElementsIndirectCommand[1];
        drawCommand[0].count = 312;
        drawCommand[0].instanceCount = 0;
        drawCommand[0].firstIndex = 0;
        drawCommand[0].baseVertex = 0;
        drawCommand[0].baseInstance = 0;

        drawIndirectCommandBuffer.SetData(drawCommand);
    }

    public void ResetTransformBuffer()
    {
        Matrix4x4[] identityMatrices = new Matrix4x4[INSTANCE_COUNT];
        for (int i = 0; i < INSTANCE_COUNT; i++)
        {
            identityMatrices[i] = Matrix4x4.identity;
        }
        transformsBuffer.SetData(identityMatrices);
    }

    void Update()
    {
        if (computeShader == null) return;

        int kernelHandle = computeShader.FindKernel("CSMain");

        // Exécuter le Compute Shader
        computeShader.Dispatch(kernelHandle, INSTANCE_COUNT / 64, 1, 1);

        // Lier les buffers au matériel et exécuter le rendu indirect
        renderMaterial.SetBuffer("transformsBuffer", transformsBuffer);
        Graphics.DrawProceduralIndirect(renderMaterial, new Bounds(Vector3.zero, Vector3.one * 10), MeshTopology.Triangles, drawIndirectCommandBuffer);
    }

    private void OnDestroy()
    {
        transformsBuffer?.Dispose();
        drawIndirectCommandBuffer?.Dispose();
        curvesBuffer?.Dispose();
        segmentsBuffer?.Dispose();
    }
}
