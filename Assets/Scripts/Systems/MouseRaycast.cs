using UnityEngine;

public class MouseRaycast : MonoBehaviour
{
    public Camera mainCamera;
    public TerrainData terrain;
    public Transform cursorIndicator; // Un GameObject pour montrer la position de l'intersection

    private Vector2 cursorScreenPosition;
    public Vector3 cursorWorldPosition;

    void Update()
    {
        if (mainCamera == null || terrain == null || cursorIndicator == null) return;

        cursorScreenPosition = Input.mousePosition;

        (Vector3 worldPosition, Vector3 rayDirection) = FromScreenspaceToWorld(cursorScreenPosition, mainCamera);
        cursorWorldPosition = RaymarchToTerrain(worldPosition, rayDirection);
        if (cursorIndicator != null)
        {
            cursorIndicator.position = cursorWorldPosition;
        }
    }

    public Vector3 GetWorldPosition()
    {
        return cursorWorldPosition;
    }

    /// <summary>
    /// Convertit la position écran de la souris en position monde et direction du rayon
    /// </summary>
    private (Vector3, Vector3) FromScreenspaceToWorld(Vector2 cursorPosScreen, Camera camera)
    {
        Ray ray = camera.ScreenPointToRay(cursorPosScreen);
        return (ray.origin, ray.direction.normalized);
    }

    /// <summary>
    /// Raymarching le long du rayon pour détecter l'intersection avec le terrain
    /// </summary>
    private Vector3 RaymarchToTerrain(Vector3 startPos, Vector3 rayDirection)
    {
        float maxY = terrain.maxY;
        float minY = terrain.minY;

        Vector3 upperBound = startPos + rayDirection * ((maxY - startPos.y) / rayDirection.y);
        Vector3 lowerBound = startPos + rayDirection * ((minY - startPos.y) / rayDirection.y);

        int steps = 1000;
        float stepSize = (lowerBound - upperBound).magnitude / steps;
        Vector3 p = upperBound;

        for (int i = 0; i < steps; i++)
        {
            Vector3 newP = p + rayDirection * stepSize;

            if (newP.y < terrain.height_at(newP.x, newP.z))
            {
                return p; // Intersection trouvée
            }

            p = newP;
        }

        return lowerBound; // Pas d'intersection trouvée
    }
}
