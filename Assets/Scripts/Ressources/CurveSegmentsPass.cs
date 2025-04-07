using UnityEngine;

public class CurveSegmentsComputePass : MonoBehaviour
{
    public ComputeShader computeTexture; // Shader principal "shaders/arch_curve_segments"

    public ComputeBuffer curvesBuffer;
    public ComputeBuffer segmentsBuffer;
    private ComputeBuffer computeIndirectCmdBuffer;

    private const int COMMAND_BUFFER_SIZE = 1000;
    public const int CURVE_BUFFER_SIZE = 1000;
    private uint cmdBufferBindingPoint;

    struct ArchSegmentDataSSBO
    {
        public Vector2 start;
        public Vector2 end;
    }

    struct DispatchIndirectCommand
    {
        public uint numGroupsX;
        public uint numGroupsY;
        public uint numGroupsZ;
    }

    // ? **Ajout du constructeur pour initialiser l'objet comme en Bevy**
    public CurveSegmentsComputePass(ShaderWatcher shaderwatch, ShaderLibrary shaderLibrary)
    {
        // Charger le Compute Shader
        ComputeTexture computeTex = ComputeTexture.Init("shaders/arch_curve_segments", shaderwatch, shaderLibrary);
        computeTexture = computeTex.ComputeProgram; // R�cup�ration du ComputeShader

        cmdBufferBindingPoint = 5; // Valeur par d�faut (correspond � Bevy)

        // Initialisation des buffers
        /*
        curvesBuffer = new ComputeBuffer(CURVE_BUFFER_SIZE, sizeof(float) * 2);
        segmentsBuffer = new ComputeBuffer(COMMAND_BUFFER_SIZE, sizeof(float) * 4);
        computeIndirectCmdBuffer = new ComputeBuffer(1, sizeof(uint) * 3, ComputeBufferType.IndirectArguments);
        */

        curvesBuffer = new ComputeBuffer(CURVE_BUFFER_SIZE, sizeof(float) * 2);
        //Debug.Log($"[Init] CURVE_BUFFER_SIZE stride = {sizeof(float) * 2}");

        segmentsBuffer = new ComputeBuffer(COMMAND_BUFFER_SIZE, sizeof(float) * 4);
        //Debug.Log($"[Init] SEGMENTS stride = {sizeof(float) * 4}");

        computeIndirectCmdBuffer = new ComputeBuffer(1, sizeof(uint) * 3, ComputeBufferType.IndirectArguments);
        //Debug.Log($"[Init] computeIndirectCmdBuffer stride = {sizeof(uint) * 3}");

        // Initialisation des courbes avec des valeurs de test
        Vector2[] curveData = new Vector2[CURVE_BUFFER_SIZE];
        for (int i = 0; i < CURVE_BUFFER_SIZE; i++)
        {
            curveData[i] = new Vector2(i * 0.1f, Mathf.Sin(i * 0.1f));
        }
        curvesBuffer.SetData(curveData);
    }

    public void Init(ShaderWatcher shaderwatch, ShaderLibrary shaderLibrary, uint cmdBufferBinding)
    {
        // Charger le Compute Shader en gardant le nom comme en Rust
        ComputeTexture computeTex = ComputeTexture.Init("shaders/arch_curve_segments", shaderwatch, shaderLibrary);
        computeTexture = computeTex.ComputeProgram; // Correction : R�cup�ration du ComputeShader

        cmdBufferBindingPoint = cmdBufferBinding;

        // Initialisation des buffers
        /*
        curvesBuffer = new ComputeBuffer(CURVE_BUFFER_SIZE, sizeof(float) * 2);
        segmentsBuffer = new ComputeBuffer(COMMAND_BUFFER_SIZE, sizeof(float) * 4);
        computeIndirectCmdBuffer = new ComputeBuffer(1, sizeof(uint) * 3, ComputeBufferType.IndirectArguments);
        */

        curvesBuffer = new ComputeBuffer(CURVE_BUFFER_SIZE, sizeof(float) * 2);
        Debug.Log($"[Init] CURVE_BUFFER_SIZE stride = {sizeof(float) * 2}");

        segmentsBuffer = new ComputeBuffer(COMMAND_BUFFER_SIZE, sizeof(float) * 4);
        Debug.Log($"[Init] SEGMENTS stride = {sizeof(float) * 4}");

        computeIndirectCmdBuffer = new ComputeBuffer(1, sizeof(uint) * 3, ComputeBufferType.IndirectArguments);
        Debug.Log($"[Init] computeIndirectCmdBuffer stride = {sizeof(uint) * 3}");

        // Initialisation des courbes avec des valeurs de test
        Vector2[] curveData = new Vector2[CURVE_BUFFER_SIZE];
        for (int i = 0; i < CURVE_BUFFER_SIZE; i++)
        {
            curveData[i] = new Vector2(i * 0.1f, Mathf.Sin(i * 0.1f));
        }
        curvesBuffer.SetData(curveData);
    }

    public void Bind(ShaderLibrary assetsShader, RenderTexture pathMask, Vector2 pathMaskWsDims)
    {
        // 1. Récupérer le ComputeShader depuis la ShaderLibrary
        ComputeShader shader = assetsShader.GetComputeShaderByName("shaders/arch_curve_segments");
        if (shader == null)
        {
            Debug.LogError("Shader introuvable dans ShaderLibrary !");
            return;
        }

        int kernel = shader.FindKernel("CSMain");

        // 2. Lier le buffer de commande indirecte
        shader.SetBuffer(kernel, "CommandBuffer", computeIndirectCmdBuffer);

        // 3. Lier les uniforms de position/dimension
        shader.SetVector("path_mask_ws_dims", pathMaskWsDims);

        // 4. Lier directement la RenderTexture comme image
        shader.SetTexture(kernel, "PathMaskTexture", pathMask);

        // 5. Lier les buffers nécessaires
        shader.SetBuffer(kernel, "CurvesBuffer", curvesBuffer);
        shader.SetBuffer(kernel, "SegmentsBuffer", segmentsBuffer);

        // 6. Dispatch du compute shader
        shader.Dispatch(kernel, 1, 1, 1);
    }

    public void Bind(ShaderLibrary assetsShader, uint pathMask, Vector2 pathMaskWsDims, uint pathMaskImgUnit)
    {
        // 1?? R�cup�rer le ComputeShader depuis la ShaderLibrary
        ComputeShader shader = assetsShader.GetComputeShaderByName("shaders/arch_curve_segments"); 
        if (shader == null)
        {
            Debug.LogError("Shader introuvable dans ShaderLibrary !");
            return;
        }

        int kernel = shader.FindKernel("CSMain");

        // 2?? Lier le buffer de commande indirecte
        shader.SetBuffer(kernel, "CommandBuffer", computeIndirectCmdBuffer);

        // 3?? Lier le masque de la route
        shader.SetInt("path_mask", (int)pathMaskImgUnit);
        shader.SetVector("path_mask_ws_dims", pathMaskWsDims);

        // 4?? Lier la texture (�quivalent de `gl::BindImageTexture`)
       // shader.SetTexture(kernel, "PathMaskTexture", assetsShader.GetRenderTextureFromID(pathMask));

        // 5?? Lier les buffers
        shader.SetBuffer(kernel, "CurvesBuffer", curvesBuffer);
        shader.SetBuffer(kernel, "SegmentsBuffer", segmentsBuffer);

        // 6?? Ex�cuter le Compute Shader
        shader.Dispatch(kernel, 1, 1, 1);
    }

    





    public void ResetCmdBuffer()
    {
        // R�initialiser le buffer de commande
        DispatchIndirectCommand[] commandData = new DispatchIndirectCommand[1];
        commandData[0] = new DispatchIndirectCommand { numGroupsX = 0, numGroupsY = 1, numGroupsZ = 1 };
        computeIndirectCmdBuffer.SetData(commandData);
    }

    public void ResetSegmentsBuffer()
    {
        // R�initialiser le buffer de segments
        ArchSegmentDataSSBO[] emptyData = new ArchSegmentDataSSBO[COMMAND_BUFFER_SIZE];
        for (int i = 0; i < COMMAND_BUFFER_SIZE; i++)
        {
            emptyData[i].start = Vector2.zero;
            emptyData[i].end = new Vector2(0.0f, -1.0f);
        }
        segmentsBuffer.SetData(emptyData);
    }

    private void OnDestroy()
    {
        // Lib�rer la m�moire GPU
        curvesBuffer.Release();
        segmentsBuffer.Release();
        computeIndirectCmdBuffer.Release();
    }
}
