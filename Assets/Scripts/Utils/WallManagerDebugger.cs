using System.Collections.Generic;
using UnityEngine;

public class WallManagerDebugger : MonoBehaviour
{
    [Header("R�f�rence logique (remplie ailleurs)")]
    public WallManager wallManager = new WallManager();

    [Header("Liste des murs (debug)")]
    [SerializeField]
    private List<Wall> debugWalls = new();
    [SerializeField]
    private List<GameObject> debugWallsObjects = new();


    void OnEnable()
    {
        GameEvents.OnMiddleMousePressed.AddListener(WallUpdated);
    }

    void OnDisable()
    {
        GameEvents.OnMiddleMousePressed.RemoveListener(WallUpdated);
    }

    void WallUpdated()
    {
        if (wallManager != null)
        {
            debugWalls = wallManager.GetWalls();
            debugWallsObjects = wallManager.GetWallGameObject();

        }
    }
}
