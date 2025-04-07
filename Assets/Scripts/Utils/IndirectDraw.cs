using UnityEngine;

public class RenderEntry : MonoBehaviour
{
    public string meshName;          
    public string shaderName;
    public VAO vao;

    public bool hasVAO;
    [SerializeField]
    private Mesh mesh;
    [SerializeField]
    private Material material;

    public GameObject gameObject;
    public ShaderProgram shader;
    public Matrix4x4 modelMatrix;
    public bool isTransparent;
    public bool isRoad;
    public bool showPathMask;
    public InstancedWall instancedWall;
    public IndirectDrawData indirectDraw;

    public void Change()
    {
        hasVAO = vao != null;
        if(vao != null)
        {
            if (vao.unity_mesh != null)
            {
                mesh = vao.unity_mesh;
            }
        }
       
        if(shader != null)
        {
            if(shader.Material != null)
            {
                material = shader.Material;
            }

        }
    }
}

public class IndirectDrawData
{
    public StructuredBufferHandle transformsBuffer;
    public int instanceCount;

    public IndirectDrawData(StructuredBufferHandle buffer, int count)
    {
        transformsBuffer = buffer;
        instanceCount = count;
    }
}

public class StructuredBufferHandle
{
    public ComputeBuffer buffer;

    public StructuredBufferHandle(ComputeBuffer buffer)
    {
        this.buffer = buffer;
    }

    public void Bind(ShaderProgram shader, string name)
    {
        if (shader.Type == ShaderType.Graphics)
        {
            shader.Material.SetBuffer(name, buffer);
        }
        else if (shader.Type == ShaderType.Compute)
        {
            shader.Compute.SetBuffer(0, name, buffer);
        }
    }
}