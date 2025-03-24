using UnityEngine;
using System.Collections.Generic;

public enum BrushPreviewType
{
    Wall,
    Path,
    Eraser
}

public class BrushPreviewMarker : MonoBehaviour
{
    public BrushPreviewType type;
}

public class BrushPreview : MonoBehaviour
{
    public MouseRaycast mouseRaycast;

    private Dictionary<BrushPreviewType, GameObject> previews;
    private BrushPreviewType currentPreviewType;

    public void BrushBindings()
    {
        // Cherche automatiquement les entités dans la scène avec le bon composant
        previews = new Dictionary<BrushPreviewType, GameObject>();

        foreach (var marker in Object.FindObjectsByType<BrushPreviewMarker>(FindObjectsSortMode.None))
        {

            previews[marker.type] = marker.gameObject;
        }

        DisableAllPreviews();
        GameEvents.OnBrushModeChanged.AddListener(OnBrushModeChanged);
    }
    void Update()
    {
        if (previews[currentPreviewType] != null && previews[currentPreviewType].activeSelf)
        {
            Vector3? worldPos = mouseRaycast.cursorWorldPosition;
            if (worldPos.HasValue)
            {
                previews[currentPreviewType].transform.position = worldPos.Value;
            }
        }
    }

    void OnBrushModeChanged(BrushMode mode)
    {
        BrushPreviewType keep = mode switch
        {
            BrushMode.Wall => BrushPreviewType.Wall,
            BrushMode.Path => BrushPreviewType.Path,
            BrushMode.Eraser => BrushPreviewType.Eraser,
            _ => BrushPreviewType.Wall

        };

        // Eraser All
        currentPreviewType = keep;

        foreach (var pair in previews)
        {
            if (pair.Key == keep)
            {
                bool shouldBeActive = pair.Key == keep;
                pair.Value.SetActive(shouldBeActive);

        
            }
            else
            {
                pair.Value.SetActive(false);
            }
        }

    }

    private void DisableAllPreviews()
    {
        foreach (var pair in previews)
        {
            if (pair.Value != null)
                pair.Value.SetActive(false);
        }
    }
}
