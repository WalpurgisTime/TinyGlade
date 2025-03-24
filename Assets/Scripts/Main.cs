using UnityEngine;

public class Main : MonoBehaviour
{
    private ShaderWatcher temp_shaderwatch;
    public ShaderLibrary temp_assets_shader;
    public RenderManager render_manager;

    // COMPUTE SHADERS -------------------------------------------
    private ComputePathMask compute_paths_mask;
    private ComputePathBlur compute_paths_blur;
    private CurveSegmentsComputePass compute_curve_segments;
    private ComputeArchesIndirect compute_arches_indirect;

    

    [SerializeField] private ConstructWallNShadows construct;

    void Start()
    {
        temp_shaderwatch = new ShaderWatcher();

        compute_paths_mask = new ComputePathMask(ComputeTexture.Init(
            "shaders/compute_path_mask", // ⚠️ Ne pas inclure ".compute"
            temp_shaderwatch,
            temp_assets_shader
        ));


        compute_paths_blur = new ComputePathBlur(ComputeTexture.Init(
            "shaders/blur",
            temp_shaderwatch,
            temp_assets_shader
        ));

        compute_curve_segments = new CurveSegmentsComputePass(temp_shaderwatch, temp_assets_shader);
        compute_arches_indirect = new ComputeArchesIndirect(temp_shaderwatch, temp_assets_shader);

        render_manager.Render();

    }

    void Update()
    {
        //construct.walls_update();
    }
}

/*
 using UnityEngine;


 * ────────────────────────────────────────────────────────────────────────────
 * 🚀 SYSTÈME PRINCIPAL - INITIALISATION ET BOUCLE PRINCIPALE 🚀
 * ────────────────────────────────────────────────────────────────────────────
 * Ce script est le point d'entrée principal du projet Unity et gère :
 * - L'initialisation des ressources et des bibliothèques graphiques.
 * - Le paramétrage et lancement du moteur Unity.
 * - La gestion des événements utilisateur et de la boucle principale.
 * 
 * 📦 MODULES IMPORTÉS 📦
 * ────────────────────────────────────────────────────────────────────────────
 * 🎨 Gestion des ressources graphiques → Chargement des meshes, shaders et textures.
 * 🔄 Gestion du moteur Unity → Paramétrage des systèmes et des objets de la scène.
 * 👀 Interactions utilisateur → Gestion des entrées clavier/souris et raycasts.
 * 📡 Gestion des shaders → Détection et rechargement dynamique des shaders.
 * 🌍 Système de caméra → Configuration et mise à jour de la caméra principale.
 * 🏗️ Organisation des systèmes → Configuration des mises à jour du jeu.
 * 🖥️ Gestion des événements → Gestion de la boucle principale et des interactions.
 * ────────────────────────────────────────────────────────────────────────────
 

public class Main : MonoBehaviour
{
    
     * 🎛️ SETTINGS - PARAMÈTRES GLOBAUX 🎛️
     * ────────────────────────────────────────────────────────────────────
     * Définit les valeurs de base du projet, comme :
     * - La résolution de la fenêtre pour le rendu.
     * - L'activation ou désactivation des vérifications de shaders.
     * - Les paramètres globaux influençant le moteur de jeu.
     
    private const int SCR_WIDTH = 1600; // 📏 Largeur de l'écran en pixels.
    private const int SCR_HEIGHT = 1200; // 📐 Hauteur de l'écran en pixels.
    private const bool VALIDATE_SHADERS = false; // ✅ Vérifier la validité des shaders au démarrage.

    
     * 🔄 POINT D'ENTRÉE PRINCIPAL 🔄
     * ────────────────────────────────────────────────────────────────────
     * Cette fonction représente l'entrée principale du programme Unity.
     * Elle gère toutes les étapes critiques nécessaires au bon fonctionnement du projet.
     
    public void Mainm()
    {
        // 📜 Étape 1 : Initialisation du système de logs
        // Permet d'afficher les messages d'erreur, de débogage et de suivi de l'exécution.

        // 🖥️ Étape 2 : Configuration de la fenêtre et du moteur graphique
        // Définit la résolution et initialise les paramètres graphiques pour l'affichage.

        // ✅ Étape 3 : Vérification et validation des shaders (si activé)
        // Vérifie que les shaders sont bien compilés et fonctionnels avant leur utilisation.

        // 🎨 Étape 4 : Chargement et surveillance des shaders
        // Permet de recharger dynamiquement les shaders en cas de modification sans redémarrer.

        // 🖥️ Étape 5 : Initialisation des compute shaders
        // Utilisé pour des calculs graphiques avancés (flou, simulation de textures, etc.).

        // 🏗️ Étape 6 : Création du moteur ECS (Entity Component System)
        // Configure la structure du jeu en créant un moteur basé sur des entités et composants.

        // 📂 Étape 7 : Chargement des ressources graphiques essentielles
        // Importation des modèles 3D, textures et shaders nécessaires au rendu de la scène.

        // 🎮 Étape 8 : Ajout des événements utilisateur
        // Enregistre et gère les entrées utilisateur (clavier, souris, manettes, etc.).

        // 🚀 Étape 9 : Ajout des systèmes ECS
        // Ajoute les systèmes ECS responsables de la simulation physique, du rendu et des interactions.

        // 🎥 Étape 10 : Mise en place du système de caméra
        // Configure la caméra principale et ses paramètres de suivi dans la scène.

        // 🎯 Étape 11 : Gestion des interactions utilisateur
        // Capture les entrées utilisateur et les traduit en actions dans le jeu.

        // 📌 Étape 12 : Intégration des systèmes de gestion des objets dynamiques
        // Active les mises à jour en temps réel des objets présents dans la scène.

        // 🔄 Étape 13 : Démarrage de la boucle principale du moteur
        // Exécute le jeu en continu en traitant les entrées, les mises à jour et le rendu graphique.

        // 🖥️ Étape 14 : Gestion des événements en temps réel
        // Surveille et applique les changements liés aux interactions utilisateur et au moteur.
    }
}

*/

