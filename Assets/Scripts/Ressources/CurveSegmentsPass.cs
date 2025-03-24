using UnityEngine;

public class CurveSegmentsComputePass : MonoBehaviour
{
    public ComputeShader computeTexture; // Shader principal "shaders/arch_curve_segments"

    private ComputeBuffer curvesBuffer;
    private ComputeBuffer segmentsBuffer;
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
        computeTexture = computeTex.ComputeProgram; // Récupération du ComputeShader

        cmdBufferBindingPoint = 5; // Valeur par défaut (correspond à Bevy)

        // Initialisation des buffers
        curvesBuffer = new ComputeBuffer(CURVE_BUFFER_SIZE, sizeof(float) * 2);
        segmentsBuffer = new ComputeBuffer(COMMAND_BUFFER_SIZE, sizeof(float) * 4);
        computeIndirectCmdBuffer = new ComputeBuffer(1, sizeof(uint) * 3, ComputeBufferType.IndirectArguments);

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
        computeTexture = computeTex.ComputeProgram; // Correction : Récupération du ComputeShader

        cmdBufferBindingPoint = cmdBufferBinding;

        // Initialisation des buffers
        curvesBuffer = new ComputeBuffer(CURVE_BUFFER_SIZE, sizeof(float) * 2);
        segmentsBuffer = new ComputeBuffer(COMMAND_BUFFER_SIZE, sizeof(float) * 4);
        computeIndirectCmdBuffer = new ComputeBuffer(1, sizeof(uint) * 3, ComputeBufferType.IndirectArguments);

        // Initialisation des courbes avec des valeurs de test
        Vector2[] curveData = new Vector2[CURVE_BUFFER_SIZE];
        for (int i = 0; i < CURVE_BUFFER_SIZE; i++)
        {
            curveData[i] = new Vector2(i * 0.1f, Mathf.Sin(i * 0.1f));
        }
        curvesBuffer.SetData(curveData);
    }

    public void Bind(ShaderLibrary assetsShader, uint pathMask, Vector2 pathMaskWsDims, uint pathMaskImgUnit)
    {
        // 1?? Récupérer le ComputeShader depuis la ShaderLibrary
        ComputeShader shader = assetsShader.GetComputeShaderByName("shaders/compute_shader_name"); 
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

        // 4?? Lier la texture (équivalent de `gl::BindImageTexture`)
       // shader.SetTexture(kernel, "PathMaskTexture", assetsShader.GetRenderTextureFromID(pathMask));

        // 5?? Lier les buffers
        shader.SetBuffer(kernel, "CurvesBuffer", curvesBuffer);
        shader.SetBuffer(kernel, "SegmentsBuffer", segmentsBuffer);

        // 6?? Exécuter le Compute Shader
        shader.Dispatch(kernel, 1, 1, 1);
    }



    public void ResetCmdBuffer()
    {
        // Réinitialiser le buffer de commande
        DispatchIndirectCommand[] commandData = new DispatchIndirectCommand[1];
        commandData[0] = new DispatchIndirectCommand { numGroupsX = 0, numGroupsY = 1, numGroupsZ = 1 };
        computeIndirectCmdBuffer.SetData(commandData);
    }

    public void ResetSegmentsBuffer()
    {
        // Réinitialiser le buffer de segments
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
        // Libérer la mémoire GPU
        curvesBuffer.Release();
        segmentsBuffer.Release();
        computeIndirectCmdBuffer.Release();
    }
}
