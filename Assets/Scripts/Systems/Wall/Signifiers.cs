using UnityEngine;

public class SignifierContinueWall : MonoBehaviour
{
    public WallManager wallManager;
    public Transform cursorTransform;

    public float continueThreshold = 0.3f; // équivalent de CONTINUE_CURVE_DIST_THRESHOLD
    public Transform signifier; // objet visuel à déplacer
    public float offsetDistance = 0.12f;

    void OnEnable()
    {
        GameEvents.OnMiddleMousePressed.AddListener( SignifiersUpdate);
    }

    void OnDisable()
    {
        GameEvents.OnMiddleMousePressed.RemoveListener( SignifiersUpdate);
    }

    void SignifiersUpdate()
    {
        if (wallManager == null || cursorTransform == null || signifier == null)
            return;

        Vector3 cursorPos = cursorTransform.position;
        Vector3? signifierPos = null;

        foreach (var wall in wallManager.GetWalls())
        {
            var points = wall.curve.points;
            int count = points.Count;

            if (count >= 2)
            {
                // Vérifie la fin de la courbe
                Vector3 last = points[count - 1];
                if (Vector3.Distance(cursorPos, last) < continueThreshold)
                {
                    Vector3 prev = points[count - 2];
                    Vector3 dir = (last - prev).normalized;
                    signifierPos = last + dir * offsetDistance;
                    break;
                }

                // Vérifie le début de la courbe
                Vector3 first = points[0];
                if (Vector3.Distance(cursorPos, first) < continueThreshold)
                {
                    Vector3 next = points[1];
                    Vector3 dir = (first - next).normalized;
                    signifierPos = first + dir * offsetDistance;
                    break;
                }
            }
        }

        if (signifierPos.HasValue)
        {
            signifier.position = signifierPos.Value + Vector3.up * 0.01f;
        }
        else
        {
            signifier.position = Vector3.down; // désactive visuellement
        }
    }
}
