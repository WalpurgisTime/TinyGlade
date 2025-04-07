using UnityEngine;
using UnityEngine.Rendering;

public class TestMask : MonoBehaviour
{
    public Shader drawMaskShader;
    private Material drawMaskMaterial;

    private Mesh ndcQuadMesh;
    private RenderTexture renderTexture;
    private int width = 512;
    private int height = 512;

    void Start()
    {
        // Create Material from Shader
        drawMaskMaterial = new Material(drawMaskShader);

        // Create NDC Quad mesh
        ndcQuadMesh = new Mesh();
        ndcQuadMesh.vertices = new Vector3[]
        {
            new Vector3(-1, 1, 0),
            new Vector3(-1, -1, 0),
            new Vector3(1, -1, 0),
            new Vector3(1, 1, 0)
        };
        ndcQuadMesh.triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3
        };
        ndcQuadMesh.RecalculateNormals();

        // Create RenderTexture
        renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
    }

    void OnEnable()
    {
        GameEvents.OnMiddleMousePressed.AddListener(RenderToTexture);
    }

    void OnDisable()
    {
        GameEvents.OnMiddleMousePressed.RemoveListener(RenderToTexture);
    }

    void RenderToTexture()
    {
        // Save previous RT
        RenderTexture tmp = RenderTexture.active;

        // Set our render texture
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.clear);

        // Set up command buffer or just use Graphics.DrawMeshNow
        drawMaskMaterial.SetVector("_Mouse_Position", Input.mousePosition);

        drawMaskMaterial.SetPass(0);
        Graphics.DrawMeshNow(ndcQuadMesh, Matrix4x4.identity);

        RenderTexture.active = tmp;
    }

    public RenderTexture GetTexture()
    {
        return renderTexture;
    }
}
