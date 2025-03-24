using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConstructWallNShadows : MonoBehaviour
{
    public BrushMode currentMode;

    public WallManagerDebugger wallManagerdebugger;
    public MeshLibrary assetMeshLibrary;
    public ShaderLibrary assetShaderLibrary;

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
        curveChangedBuffer.Add(curveIndex);
        walls_update(); 
    }

    public void walls_update()
    {
        // Check mode
        if (currentMode != BrushMode.Wall && currentMode != BrushMode.EraserAll)
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
                changedWall.wallEntity = CreateWall(
                    changedWall.curve.length,
                    bricks
                );
            }
            else
            {
                Debug.Log("creating wall..");
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

        Debug.Log("Wall construction done");

        curveChangedBuffer.Clear();
    }

    private GameObject CreateWall(float curveLength, List<Brick> bricks)
    {
        GameObject wallGO = new GameObject("InstancedWall");
        wallGO.AddComponent<MeshFilter>();
        wallGO.AddComponent<MeshRenderer>();

        var wallComponent = wallGO.AddComponent<InstancedWall>();
        wallComponent.Init(curveLength, bricks);

        Mesh mesh = assetMeshLibrary.GetMeshByName("bricks");
        Shader shader = assetShaderLibrary.GetShaderByName("instanced_wall_shader");

        // ✅ AJOUT : vérifie si shader est null
        if (shader == null)
        {
            Debug.LogError("[ConstructWallNShadows] Shader 'instanced_wall_shader' not found in ShaderLibrary!");
            return wallGO;
        }

        Material material = new Material(shader);
        wallGO.GetComponent<MeshFilter>().mesh = mesh;
        wallGO.GetComponent<MeshRenderer>().material = material;

        // CREATION DE BRICK # SUP

        foreach (var brick in bricks)
        {
            GameObject brickGO = new GameObject("Brick");

            brickGO.transform.SetParent(wallGO.transform);

            brickGO.AddComponent<MeshFilter>().mesh = mesh;
            brickGO.AddComponent<MeshRenderer>().material = redMaterial;
            brickGO.AddComponent<BrickInstanceMarker>();

            // Appliquer la transformation de la brique
            Matrix4x4 mat = brick.transform.ComputeMatrix();

            brickGO.transform.position = mat.GetColumn(3); // position
            brickGO.transform.rotation = Quaternion.LookRotation(
                mat.GetColumn(2), // Z → forward
                mat.GetColumn(1)  // Y → up
            );

            brickGO.transform.localScale = new Vector3(
                mat.GetColumn(0).magnitude,
                mat.GetColumn(1).magnitude,
                mat.GetColumn(2).magnitude
            );

            brickGO.transform.localScale = brickGO.transform.localScale * 100f;
        }

        return wallGO;
    }

    public void ClearBricks(GameObject wallGO)
    {
        var markers = wallGO.GetComponentsInChildren<BrickInstanceMarker>();
        foreach (var marker in markers)
        {
            DestroyImmediate(marker.gameObject);
        }

        Debug.Log($"Cleared {markers.Length} brick GameObjects.");
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