using UnityEngine;

public class SimpleGPUInstancedCube : MonoBehaviour
{
    public Material material;
    private Mesh cubeMesh;
    private ComputeBuffer argsBuffer;

    private MaterialPropertyBlock props;

    private const int instanceCount = 1;

    void Start()
    {
        CreateCubeMesh();

        // Props = bloc pour passer des variables au shader sans modifier le Material
        props = new MaterialPropertyBlock();
        props.SetColor("_Color", Color.cyan); // 👈 Envoie la couleur

        // Buffer indirect pour DrawMeshInstancedProcedural
        uint[] args = new uint[5] {
            cubeMesh.GetIndexCount(0),  // index count
            (uint)instanceCount,        // instance count
            cubeMesh.GetIndexStart(0),  // start index
            cubeMesh.GetBaseVertex(0),  // base vertex
            0                           // padding
        };

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }


    void Update()
    {
        if (material == null || cubeMesh == null || argsBuffer == null)
            return;

        // Optional: mise à jour dynamique d'une couleur (ex : couleur qui change dans le temps)
        props.SetColor("_Color", Color.Lerp(Color.blue, Color.red, Mathf.PingPong(Time.time, 1)));

        // Bounds arbitraires (pour culling)
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 10f);

        Graphics.DrawMeshInstancedProcedural(
            cubeMesh,
            0,
            material,
            bounds,
            instanceCount,
            props
        );
    }

    void OnDestroy()
    {
        argsBuffer?.Release();
    }

    void CreateCubeMesh()
    {
        cubeMesh = new Mesh();

        Vector3[] vertices = {
            // Front
            new Vector3(-1, -1,  1),
            new Vector3(1, -1,  1),
            new Vector3(1,  1,  1),
            new Vector3(-1,  1,  1),

            // Back
            new Vector3(-1, -1, -1),
            new Vector3(1, -1, -1),
            new Vector3(1,  1, -1),
            new Vector3(-1,  1, -1),
        };

        int[] triangles = {
            0, 2, 1, 0, 3, 2,      // Front
            3, 6, 2, 3, 7, 6,      // Top
            7, 5, 6, 7, 4, 5,      // Back
            4, 1, 5, 4, 0, 1,      // Bottom
            4, 3, 0, 4, 7, 3,      // Left
            1, 6, 5, 1, 2, 6       // Right
        };

        cubeMesh.vertices = vertices;
        cubeMesh.triangles = triangles;
        cubeMesh.RecalculateNormals();
    }
}
