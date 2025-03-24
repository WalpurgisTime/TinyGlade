

/*
 * ????????????????????????????????????????????????????????????????????????????
 * ?? SYSTÈME DE DÉMARRAGE - ADAPTATION UNITY DU SCRIPT BEVY ECS ??
 * ????????????????????????????????????????????????????????????????????????????
 * Ce fichier met en place un système de démarrage inspiré de Bevy ECS en Rust.
 * Il gère le chargement des ressources, la création des entités et leur comportement.
 * 
 * ??? ÉQUIVALENCES BEVY ? UNITY ???
 * ????????????????????????????????????????????????????????????????????????????
 * ??? "Handle" ? Gestion des références aux ressources (ex : Meshes, Shaders).
 * ?? "AssetMeshLibrary" ? Stockage et accès aux modèles 3D.
 * ?? "AssetShaderLibrary" ? Gestion centralisée des shaders pour le rendu.
 * ?? "ShaderWatch" ? Détection des modifications de shaders en temps réel.
 * ?? "BrushPreview" ? Prévisualisation des outils de dessin (ex: murs, chemins).
 * ?? "SignifierContinueWall" ? Indicateur de continuation d'un mur interactif.
 * ?? "Plane" ? Définition et génération de plans géométriques.
 * ?? "load_json_as_mesh" ? Chargement de modèles 3D au format JSON.
 * ??? "DrawableMeshBundle" ? Structure contenant un maillage, un shader et un transform.
 * ?? "FollowMouse" ? Composant permettant aux entités de suivre la souris.
 * ????????????????????????????????????????????????????????????????????????????
 */


/*

[Serializable]
public class Startup : MonoBehaviour
{
    [SerializeField] private Mesh someMesh; // ??? Mesh pouvant être assigné via l'Inspector.
    [SerializeField] private Shader someShader; // ?? Shader assignable dans l'Inspector.

    public void ResMut<T>() where T : Component
    {
        // ?? Accès à une ressource mutable du type spécifié.
        // Dans un environnement ECS, cela permettrait de modifier dynamiquement un composant.
    }

    public void StartupSystem()
    {
         ????????????????????????????????????????????????????????????????????
         * ?? INITIALISATION DES RESSOURCES & CHARGEMENT DES DONNÉES ??
         * ????????????????????????????????????????????????????????????????????
         * Cette fonction est responsable du lancement des éléments essentiels :
         * - Chargement des maillages 3D et stockage dans une bibliothèque.
         * - Initialisation des shaders pour les effets visuels.
         * - Création et insertion des entités dans la scène Unity.
         * - Ajout de composants interactifs (ex : suivi de la souris, indicateurs).
         

        // ?? CHARGEMENT DES MESHES
        // Extraction des modèles 3D (ex : sol, briques, formes) depuis les fichiers sources.
        // Conversion de ces fichiers en objets Mesh utilisables dans Unity.
        // Stockage des meshes dans une bibliothèque pour un accès facile et performant.

        // ?? CHARGEMENT DES SHADERS
        // Récupération et compilation des shaders nécessaires au rendu graphique.
        // Ajout de ces shaders à une bibliothèque de ressources pour une réutilisation efficace.
        // Assurer que chaque shader est correctement lié aux objets 3D correspondants.

        // ??? CRÉATION DES ENTITÉS
        // Instanciation des objets 3D en tant qu'entités dans la scène.
        // Attribution des maillages et shaders aux objets pour un affichage correct.
        // Définition de leurs transformations (position, rotation, échelle).

        // ?? AJOUT DES COMPOSANTS INTERACTIFS
        // Intégration de comportements spécifiques aux entités (ex: suivi de la souris).
        // Ajout de scripts qui réagissent aux entrées utilisateur.
        // Paramétrage des interactions et animations.

        // ??? AJOUT DES SIGNIFIERS DE CONTINUITÉ
        // Définition des marqueurs pour les éléments nécessitant une logique spéciale.
        // Exemple : gestion dynamique de murs modulables.

        // ? CONFIRMATION DU DÉMARRAGE
        // Affichage d'un message dans la console pour signaler que le processus est terminé.
        Debug.Log("? Startup system initialized successfully!");
    }

    public Mesh LoadMesh(string path)
    {
        /* ????????????????????????????????????????????????????????????????????
         * ?? CHARGEMENT D'UN MESH 3D
         * ????????????????????????????????????????????????????????????????????
         * Cette fonction récupère un fichier 3D et le convertit en Mesh Unity.
         * Les données extraites comprennent :
         * - Positions des sommets ???
         * - Normales pour l'éclairage ??
         * - Indices pour le rendu des faces ??
         * - Vérification et application des couleurs de matériau.
         
        return null;
    }

    public Mesh LoadMeshIntoLibrary(Mesh mesh, string name)
    {
        ????????????????????????????????????????????????????????????????????
         * ?? STOCKAGE DU MESH DANS LA BIBLIOTHÈQUE
         * ????????????????????????????????????????????????????????????????????
         * Cette fonction stocke un mesh dans la bibliothèque des ressources.
         * Le nom associé permet un accès rapide et structuré.
         * Assure la disponibilité du maillage pour d'autres objets ou scènes.
         
        return null;
    }

    public Shader LoadShaderIntoLibrary(string vertexShaderPath, string fragmentShaderPath, string name)
    {
        ????????????????????????????????????????????????????????????????????
         * ?? CHARGEMENT ET STOCKAGE D'UN SHADER
         * ????????????????????????????????????????????????????????????????????
         * Cette fonction charge un shader depuis des fichiers GLSL.
         * Compilation des fichiers vertex et fragment pour obtenir un shader exploitable.
         * Ajout du shader à une bibliothèque centralisée pour le rendu des objets 3D.
         * Vérification des erreurs de compilation et affichage en console si nécessaire.
         
        return null;
    }
}


*/


using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;

[System.Serializable]
public class glbMesh
{
    public string name;
    public Mesh mesh;
}


public class Startup : MonoBehaviour
{
    public MeshLibrary meshLibrary;
    public ShaderLibrary shaderLibrary;
    public MouseRaycast mouseRaycast;
    public BrushPreview brushPreview;

    [SerializeField]
    public glbMesh[] glb;

    void Start()
    {
        Debug.Log("Démarrage de l'initialisation...");

        // Charger les Meshes
        Mesh floor = LoadMeshIntoLibrary("Assets/Resources/meshes/floor.glb", "floor");
        Mesh brick = LoadMeshIntoLibrary("Assets/Resources/meshes/brick.glb", "bricks");
        Mesh plane = LoadMeshIntoLibrary(PlaneMesh.FromPlane(new PlaneMesh(20.0f)), "plane");
        Mesh circle = LoadMeshIntoLibrary("Assets/Resources/meshes/circle.json", "circle");
        //Mesh roadPebbles = LoadMeshIntoLibrary("Assets/Resources/meshes/road_pebbles.json", "road");
        Mesh roadPebbles = Roadpebbles();
        BoundingBoxInjector.ApplyBoundingBoxes(roadPebbles);



        Mesh brushArrow = LoadMeshIntoLibrary("Assets/Resources/meshes/brush_arrow.json", "brush_arrow");
        Mesh brushCircle = LoadMeshIntoLibrary("Assets/Resources/meshes/brush_circle.json", "brush_circle");
        Mesh brushCircleCross = LoadMeshIntoLibrary("Assets/Resources/meshes/brush_circle_cross.json", "brush_circle_cross");

        // Charger les Shaders
        Shader vertColor = LoadShaderIntoLibrary("shaders/vertex_color", "vertex_color_shader");
        //Shader roadShader = LoadShaderIntoLibrary("shaders/Paths", "road_shader");
        Shader roadShader = LoadShaderIntoLibrary("shaders/RoadShader", "road_shader");
        LoadShaderIntoLibrary("shaders/Instanced_wall", "instanced_wall_shader");
        LoadShaderIntoLibrary("shaders/shadow", "shadow_shader");


        Shader indirectTest = LoadShaderIntoLibrary("shaders/Instanced_wall_arch", "indirect_instance_test");

        // Création des entités
        CreateEntity(brick, indirectTest, "bricks");
        CreateEntity(floor, vertColor, "floor", new Vector3(0, 0, 0), 1.0f, new Vector3(-90, 0, 0));
        //CreateEntity(roadPebbles, roadShader, "roadPebbles", new Vector3(0, 0.1f, 0), 1.0f, new Vector3(180, 0, 0));
        CreatePebbles(roadPebbles, roadShader, "roadPebbles", new Vector3(0, 0.1f, 0), 1.0f, new Vector3(180, 0, 0));
        mouseRaycast.cursorIndicator = CreateEntity(brushArrow, vertColor, "brushArrow").transform;


        var wallPreview = CreateEntity(brushArrow, vertColor, "BrushPreview_Wall");
        wallPreview.AddComponent<BrushPreviewMarker>().type = BrushPreviewType.Wall;


        var pathPreview = CreateEntity(brushCircle, vertColor, "BrushPreview_Path");

        pathPreview.AddComponent<BrushPreviewMarker>().type = BrushPreviewType.Path;


        var eraserPreview = CreateEntity(brushCircleCross, vertColor, "BrushPreview_Eraser");
        eraserPreview.AddComponent<BrushPreviewMarker>().type = BrushPreviewType.Eraser;

        brushPreview.BrushBindings();



        //MeshSpawner.Instance.CreateGLBMesh("Assets/Resources/meshes/brick.glb");
        //MeshSpawner.Instance.CreateGLBMesh("Assets/Resources/meshes/floor.glb");

        //Debug.Log("Initialisation terminée !");
    }

    Mesh LoadMeshIntoLibrary(string path, string name)
    {
        //Debug.Log($"🔍 Tentative de chargement du Mesh: {path}");

        Mesh mesh = null;

        // Si le chemin correspond à un fichier JSON, utilise MeshLoader
        if (path.EndsWith(".json"))
        {
            //Debug.Log(path);
            mesh = MeshLoader.LoadJsonAsMesh(path);
        }
        // Si le chemin correspond à un fichier GLB, utilise GLBMeshLoader
        else if (path.EndsWith(".glb"))
        {
            //MeshBuffer meshBuffer = GLBMeshLoader.LoadGLBAsMeshBuffer(path);
            //mesh = GLBMeshLoader.ConvertToUnityMesh(meshBuffer);
            foreach (var glb in glb)
            {
                if (glb.name == name)
                {
                    mesh = glb.mesh;
                }
                    
            }
        }
        // Sinon, essaye de charger depuis Resources
        else
        {
            mesh = Resources.Load<Mesh>(path);
        }

        if (mesh == null)
        {
            Debug.LogError($"❌ ERREUR: Impossible de charger le Mesh '{name}' depuis '{path}'. Vérifie que le fichier existe et est correct.");
            return null;
        }


        // Ajoute le mesh à la bibliothèque
        meshLibrary.AddMesh(name, mesh);
        //Debug.Log($"📂 Mesh '{name}' ajouté à la bibliothèque.");

        return mesh;
    }



    Mesh LoadMeshIntoLibrary(Mesh mesh, string name)
    {
        meshLibrary.AddMesh(name, mesh);
        return mesh;
    }

    Shader LoadShaderIntoLibrary(string path, string name)
    {
        Shader shader = Resources.Load<Shader>(path);
        shaderLibrary.AddShader(name, shader);
        return shader;
    }

    GameObject CreateEntity(Mesh mesh, Shader shader, string name, Vector3 position = default, float scale = 1.0f , Vector3 rotation = default)
    {
        GameObject obj = new GameObject(mesh.name);
        obj.name = name;
        obj.AddComponent<MeshFilter>().mesh = mesh;
        obj.AddComponent<MeshRenderer>().material = new Material(shader);

        obj.transform.position = position;
        obj.transform.rotation = Quaternion.Euler(rotation);
        obj.transform.localScale = Vector3.one * scale;
        return obj;
    }

    GameObject CreatePebbles(Mesh mesh, Shader shader, string name, Vector3 position = default, float scale = 1.0f, Vector3 rotation = default)
    {
        GameObject obj = new GameObject(mesh.name);
        obj.name = name;
        obj.AddComponent<MeshFilter>().mesh = mesh;
       

        Material roadMat = new Material(shader);
        Texture2D terrainTex = Resources.Load<Texture2D>("Textures/fake_terrain_texture");
        Texture2D pathTex = Resources.Load<Texture2D>("Textures/fake_path_texture"); 

        roadMat.SetTexture("_TerrainTex", terrainTex);
        roadMat.SetTexture("_PathTex", pathTex);
        obj.AddComponent<MeshRenderer>().material = roadMat;

        obj.transform.position = position;
        obj.transform.rotation = Quaternion.Euler(rotation);
        obj.transform.localScale = Vector3.one * scale;
        return obj;
    }



    Mesh Roadpebbles ()
    {
        Mesh roadPebbles = MeshLoader.LoadJsonAsMesh("Assets/Resources/meshes/road_pebbles.json");
      

        // Ajout de couleurs blanches (équivalent à add_color([1.0; 3]))
        if (!roadPebbles.HasVertexAttribute(VertexAttribute.Color))
        {
            Color[] colors = new Color[roadPebbles.vertexCount];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }
            roadPebbles.colors = colors;

            //Debug.Log("Ajout de couleurs blanches");
        }

        // Ajout d’UVs (équivalent à add_uv())
        if (!roadPebbles.HasVertexAttribute(VertexAttribute.TexCoord0))
        {
            Vector3[] vertices = roadPebbles.vertices;
            Vector2[] uvs = new Vector2[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                // Simple mapping XY → UV
                uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
            }

            roadPebbles.uv = uvs;

            Debug.Log("Ajout de coordonnées UV");
        }

        // Enregistrement dans la bibliothèque
        LoadMeshIntoLibrary(roadPebbles, "road");
        return roadPebbles;

    }
}
