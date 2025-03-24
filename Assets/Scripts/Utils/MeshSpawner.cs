using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using UnityEditor.Rendering;

public class MeshSpawner : MonoBehaviour
{
    int i = 0;

    public Material redMaterial;
    private static MeshSpawner _instance;
    public static MeshSpawner Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject("MeshSpawnerSingleton");
                _instance = singletonObject.AddComponent<MeshSpawner>();
                DontDestroyOnLoad(singletonObject);
            }
            return _instance;
        }
    }// Assigne ce matériau dans l'Inspector

    void Start()
    {
        /*
        CreateMesh(MeshLoader.LoadJsonAsMesh("Assets/meshes/brush_arrow.json"));
        CreateMesh(MeshLoader.LoadJsonAsMesh("Assets/meshes/brush_circle.json"));
        CreateMesh(MeshLoader.LoadJsonAsMesh("Assets/meshes/brush_circle_cross.json"));
        CreateMesh(MeshLoader.LoadJsonAsMesh("Assets/meshes/circle.json"));
        CreateMesh(MeshLoader.LoadJsonAsMesh("Assets/meshes/grid_10x10.json"));
        CreateMesh(MeshLoader.LoadJsonAsMesh("Assets/meshes/plane.json"));
        CreateMesh(MeshLoader.LoadJsonAsMesh("Assets/meshes/road_pebbles.json"));
        */
        //CreateGLBMesh("Assets/Resources/meshes/brick.glb");
        //CreateGLBMesh("Assets/Resources/meshes/floor.glb");

        
    }

    void CreateMesh(Mesh mesh)
    {
        if (mesh == null)
        {
            Debug.LogError("Failed to load mesh!");
            return;
        }

        GameObject meshObject = new GameObject("GeneratedMesh_" + i);
        i++;
        meshObject.transform.position = new Vector3(0, i * 2, 0); // Espacement ajusté
        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;
        meshRenderer.material = redMaterial;
    }

    public void CreateGLBMesh(string path)
    {
        MeshBuffer meshBuffer = GLBMeshLoader.LoadGLBAsMeshBuffer(path);
        Mesh mesh = GLBMeshLoader.ConvertToUnityMesh(meshBuffer);
        GameObject meshObject = new GameObject("GeneratedMesh_" + i);
        meshObject.transform.position = new Vector3(0, i * 2, 0);
        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
        meshRenderer.material = redMaterial;
    }

   
}
