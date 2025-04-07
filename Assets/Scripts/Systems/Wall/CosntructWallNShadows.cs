using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConstructWallNShadows : MonoBehaviour
{
    public BrushMode currentMode;
    public bool IsWallActivated;
    public WallManagerDebugger wallManagerdebugger;
    public MeshLibrary assetMeshLibrary;
    public ShaderLibrary assetShaderLibrary;

    public RenderManager render_manager;
    public List<Material> brickMaterials; 
    public int Erased;



    public Material redMaterial;

    // Simule l'équivalent de EventReader<CurveChangedEvent>
    private List<int> curveChangedBuffer = new List<int>();

    void OnEnable()
    {
        GameEvents.OnCurveChanged.AddListener(OnCurveChangedEvent);
    }

    void OnDisable()
    {
        GameEvents.OnCurveChanged.RemoveListener(OnCurveChangedEvent);
    }

    void OnCurveChangedEvent(int curveIndex)
    {
        if(currentMode == BrushMode.Wall )
        {
            curveChangedBuffer.Add(curveIndex);

        }
        walls_update();
        render_manager.Render();

        if(currentMode == BrushMode.Eraser)
        {
            //Debug.Log(curveIndex + "wallData");
            Wall changedWall = wallManagerdebugger.wallManager.GetWall(curveIndex);
            
            if(Erased == 1)
            {
                changedWall.curve.RecalculateCurve();
                ClearBricks(changedWall.wallEntity);
                List<Brick> bricks = WallConstructor.FromCurve(changedWall.curve);
                FillBricks(changedWall.wallEntity, bricks);

                Erased = 0;
            }
            else if(Erased == 2)
            {
                changedWall.curve.RecalculateCurve();
                List<Brick> bricks = WallConstructor.FromCurve(changedWall.curve);
                changedWall.wallEntity = CreateWall(changedWall.curve.length,bricks);
                wallManagerdebugger.wallManager.RemoveWallsWithoutEntities();
                
                
                // === SHADOW DECAL ===
                if (changedWall.shadowEntity != null)
                {
                    var meshFilter = changedWall.shadowEntity.GetComponent<MeshFilter>();
                    Mesh mesh = meshFilter.sharedMesh;
                    ShadowDecal.Update(changedWall.curve, mesh);
                }
                else
                {
                    changedWall.shadowEntity = ShadowDecal.New(
                        changedWall.curve,
                        assetMeshLibrary,
                        assetShaderLibrary
                    );
                }

                Erased = 0;
                

            }

        }

    }

    public void walls_update()
    {

        // Check mode
        if (currentMode != BrushMode.Wall && currentMode != BrushMode.Eraser)
            return;

        foreach (int curveIndex in curveChangedBuffer)
        {
            Wall changedWall = wallManagerdebugger.wallManager.GetWall(curveIndex);

            if (changedWall == null)
            {
                Debug.LogError($"Wall construction failed: couldn't get Wall index {curveIndex}");
                continue;
            }

            if (changedWall.curve.points.Count < 2)
                continue;

            // === CONSTRUCTION DU MUR ===
            List<Brick> bricks = WallConstructor.FromCurve(changedWall.curve);
            if (bricks.Count == 0)
            {
                Debug.LogWarning("WallConstructor returned empty wall");
                continue;
            }

            if (changedWall.wallEntity != null)
            {
                InstancedWall wallComponent = changedWall.wallEntity.GetComponent<InstancedWall>();
                wallComponent.UpdateWall(changedWall.curve.length, bricks);
            

                // Clear bricks  + Creation # SUP
                ClearBricks(changedWall.wallEntity);
                FillBricks(changedWall.wallEntity, bricks);
                

            }
            else
            {
                //Debug.Log("creating wall..");
                changedWall.wallEntity = CreateWall(
                    changedWall.curve.length,
                    bricks
                );
            }

            // === SHADOW DECAL ===
            if (changedWall.shadowEntity != null)
            {
                var meshFilter = changedWall.shadowEntity.GetComponent<MeshFilter>();
                Mesh mesh = meshFilter.sharedMesh;
                ShadowDecal.Update(changedWall.curve, mesh);
            }
            else
            {
                changedWall.shadowEntity = ShadowDecal.New(
                    changedWall.curve,
                    assetMeshLibrary,
                    assetShaderLibrary
                );
            }
        }

        //Debug.Log("Wall construction done");

        curveChangedBuffer.Clear();
    }

    private GameObject CreateWall(float curveLength, List<Brick> bricks)
    {
        //Debug.Log("Called");
        InstancedWall wallComponent = InstancedWall.From(curveLength, bricks);
        GameObject wallGO = wallComponent.gameObject;
        wallManagerdebugger.wallManager.NewWallGameObject(wallGO);

        wallGO.AddComponent<MeshFilter>();
        wallGO.AddComponent<MeshRenderer>();

        Mesh mesh = assetMeshLibrary.GetMeshByName("bricks");
        Shader shader = assetShaderLibrary.GetShaderByName("instanced_wall_shader");
        ShaderProgram shaderProgram = ShaderProgram.New(shader);

        var renderEntry = wallGO.AddComponent<RenderEntry>();
        renderEntry.gameObject = wallGO;
        renderEntry.instancedWall = wallComponent;
        renderEntry.meshName = "bricks";
        renderEntry.shaderName = "instanced_wall_shader";
        renderEntry.shader = shaderProgram;

        if (shader == null)
        {
            Debug.LogError("[ConstructWallNShadows] Shader 'instanced_wall_shader' not found in ShaderLibrary!");
            return wallGO;
        }
        wallGO.GetComponent<MeshFilter>().mesh = mesh;
        wallGO.GetComponent<MeshRenderer>().material =  renderEntry.shader.Material;

        FillBricks(wallGO, bricks);

        return wallGO;
    }

    public void IsActivated() => IsWallActivated = !IsWallActivated;
    private void FillBricks(GameObject wallGO, List<Brick> bricks)
    {
        if (wallGO == null)
        {
            Debug.LogWarning("FillBricks called with null wall GameObject.");
            return;
        }

        if (brickMaterials == null || brickMaterials.Count == 0)
        {
            Debug.LogWarning("No materials assigned in brickMaterials list.");
            return;
        }

        Mesh mesh = assetMeshLibrary.GetMeshByName("bricks");

        foreach (var brick in bricks)
        {
            GameObject brickGO = new GameObject("Brick");
            brickGO.transform.SetParent(wallGO.transform);

            brickGO.AddComponent<MeshFilter>().mesh = mesh;

            // Choisir un matériau aléatoire depuis la liste
            Material randomMat = brickMaterials[Random.Range(0, brickMaterials.Count)];
            brickGO.AddComponent<MeshRenderer>().material = randomMat;

            brickGO.AddComponent<BrickInstanceMarker>();

            // Appliquer la transformation de la brique
            Matrix4x4 mat = brick.transform.ComputeMatrix();

            Vector3 pos = mat.GetColumn(3);
            Vector3 forward = mat.GetColumn(2);
            Vector3 up = mat.GetColumn(1);
            Vector3 scale = new Vector3(
                mat.GetColumn(0).magnitude,
                up.magnitude,
                forward.magnitude
            );

            brickGO.transform.position = pos;
            brickGO.transform.rotation = Quaternion.LookRotation(forward, up);
            brickGO.transform.localScale = scale * 100f;

            brickGO.SetActive(IsWallActivated);
        }
    }


    public void ClearBricks(GameObject wallGO)
    {
        if (wallGO == null)
        {
            Debug.LogWarning("ClearBricks called with null wall GameObject.");
            return;
        }

        var markers = wallGO.GetComponentsInChildren<BrickInstanceMarker>();

        foreach (var marker in markers)
        {
            DestroyImmediate(marker.gameObject);
        }

        //Debug.Log($"Cleared {markers.Length} brick GameObjects.");
    }



}

public class BrickInstanceMarker : MonoBehaviour { }



/*
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;


public class CosntructWallNShadows : MonoBehaviour
{
    public BrushMode _mode;
    public WallManager wallManager;
    public MeshLibrary assetsMesh;
    public ShaderLibrary assetsShader;

    void OnEnable()
    {
        GameEvents.OnCurveChanged.AddListener(OnCurveChanged);
    }

    void OnDisable()
    {
        GameEvents.OnCurveChanged.RemoveListener(OnCurveChanged);
    }

    private void OnCurveChanged(int curveIndex)
    {
        if (_mode != BrushMode.Wall && _mode != BrushMode.EraserAll) return;

        Wall changedWall = wallManager.GetWall(curveIndex);
        if (changedWall == null || changedWall.curve.points.Count < 2) return;

        UpdateWall(changedWall);
        UpdateShadow(changedWall);
    }

    private void UpdateWall(Wall changedWall)
    {
        List<Brick> bricks = WallConstructor.FromCurve(changedWall.curve);

        if (bricks.Count == 0)
        {
            Debug.LogWarning("WallConstructor returned empty wall");
            return;
        }

        if (changedWall.wallEntity != null)
        {
            InstancedWall wallComponent = changedWall.wallEntity.GetComponent<InstancedWall>();
            wallComponent.Update(changedWall.curve.length, bricks);
        }
        else
        {
            Debug.Log("Creating wall...");
            changedWall.wallEntity = CreateWall(changedWall.curve.length, bricks);
        }
    }

    private void UpdateShadow(Wall changedWall)
    {
        if (changedWall.shadowEntity != null)
        {
            MeshFilter meshFilter = changedWall.shadowEntity.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                ShadowDecal.UpdateShadowMesh(changedWall.curve, meshFilter.mesh);
            }
        }
        else
        {
            GameObject shadowObject = new GameObject("ShadowDecal");
            shadowObject.AddComponent<MeshFilter>().mesh = ShadowDecal.CreateShadowMesh(changedWall.curve);
            changedWall.shadowEntity = shadowObject;
        }
    }

    private GameObject CreateWall(float curveLength, List<Brick> bricks)
    {
        InstancedWall wallComponent = new InstancedWall(curveLength, bricks);
        GameObject wallObject = new GameObject("InstancedWall");
        wallObject.AddComponent<MeshFilter>().mesh = assetsMesh.GetMeshByName("brick");
        wallObject.AddComponent<MeshRenderer>().material = assetsShader.GetMaterial("instanced_wall_shader");
        wallObject.AddComponent<InstancedWallData>().Initialize(wallComponent);

        return wallObject;
    }


}

*/