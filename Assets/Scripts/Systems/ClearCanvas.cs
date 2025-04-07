using UnityEngine;
using UnityEngine.InputSystem; // Nécessite le Input System
using System.Collections.Generic;

public class ClearCanvas : MonoBehaviour
{
    public WallManager wallManager;
    public ComputePathMask computePathMask;
    public CurveSegmentsComputePass computeIndirect;

    void OnEnable()
    {
        GameEvents.OnMiddleMousePressed.AddListener(CanvasUpdated);
    }

    void OnDisable()
    {
        GameEvents.OnMiddleMousePressed.RemoveListener(CanvasUpdated);
    }
    void CanvasUpdated()
    {
        if (Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            Debug.Log("🧹 ClearCanvas triggered");

            // 1. Supprimer les murs via événements (simulé ici par appel direct)
            foreach (var wall in wallManager.GetWallsDictionary())
            {
                int index = wall.Key;
                GameEvents.OnCurveDeleted.Invoke(index); // Simule CurveDeletedEvent
            }

            // 2. Nettoyer la texture de masque
            computePathMask.GetTexture().Clear(); // Méthode à implémenter

            // 3. Réinitialiser les buffers GPU
            computeIndirect.ResetSegmentsBuffer();
            computeIndirect.ResetCmdBuffer();
        }
    }
}
