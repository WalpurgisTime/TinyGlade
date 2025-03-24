using System.Collections.Generic;
using UnityEngine;

public class WallManagerDebugger : MonoBehaviour
{
    [Header("R�f�rence logique (remplie ailleurs)")]
    public WallManager wallManager = new WallManager();

    [Header("Liste des murs (debug)")]
    [SerializeField]
    private List<Wall> debugWalls = new();


    void Update()
    {
        if (wallManager != null)
        {
            debugWalls = wallManager.GetWalls();
        }
    }
}
