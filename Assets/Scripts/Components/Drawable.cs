using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

[System.Serializable]
public class DrawableMesh
{
    public string name;
    public int vaoHandle;
    public int shaderHandle;
    public int meshHandle;

    public Mesh mesh;
    public ShaderProgram shader;
    public Transform transform;

    public bool isTransparent;
    public bool isIndirectDraw;
    public bool isWall;
    public bool isRoad;
    public bool showPathMask;

    public float wallLength;
    public Matrix4x4[] instanceTransforms;
    public ComputeBuffer instanceBuffer;
    public InstancedWall instancedWall;


    public GLDrawMode drawMode = new GLDrawMode(MeshTopology.Triangles);
    public Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 100); // default
    public MaterialPropertyBlock propertyBlock;

    public DrawableMesh(Mesh mesh, ShaderProgram shader, Transform transform)
    {
        this.mesh = mesh;
        this.shader = shader;
        this.transform = transform;
    }

    public Matrix4x4 transformMatrix => transform.localToWorldMatrix;
    public int instanceCount => instanceTransforms != null ? instanceTransforms.Length : 0;
}

public class GLDrawMode
{
    public UnityEngine.MeshTopology drawMode;

    public GLDrawMode(UnityEngine.MeshTopology mode)
    {
        drawMode = mode;
    }
}

public class TransparencyPass : MonoBehaviour
{
    // This class can act as a tag to indicate this object should be rendered in the transparency pass
}
