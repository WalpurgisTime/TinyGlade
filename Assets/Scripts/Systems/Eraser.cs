using System.Collections.Generic;
using UnityEngine;

public class Eraser : MonoBehaviour
{
    public BrushMode Mode; // Mode actuel de la brosse (Eraser, Brush, etc.)
    public WallManager wallManager; // Gestion des courbes/murs
    public MouseRaycast cursorRaycast; // Position du curseur en world-space
    private const float ERASE_BRUSH_SIZE = 0.75f * 0.9f; // Taille de la gomme

    void Update()
    {
        if (Mode != BrushMode.Eraser) return;
        if (!Input.GetMouseButton(0)) return;

        Vector3 cursorPos = cursorRaycast.cursorWorldPosition; // Position du curseur en world-space

        List<(int, List<Curve>)> newCurvesData = new List<(int, List<Curve>)>(); // Courbes restantes après suppression
        List<int> deletedCurves = new List<int>(); // Indices des courbes supprimées

        foreach (var wallEntry in wallManager.walls)
        {
            int curveIndex = wallEntry.Key;
            Wall wall = wallEntry.Value;
            Curve curve = wall.curve;

            List<List<Vector3>> newCurves = new List<List<Vector3>>() { new List<Vector3>() };
            int newCurveLastIndex = 0;

            for (int j = 0; j < curve.points.Count - 1; j++)
            {
                Vector3 p1 = curve.points[j];
                Vector3 p2 = curve.points[j + 1];

                bool p1Inside = Vector3.Distance(cursorPos, p1) < ERASE_BRUSH_SIZE;
                bool p2Inside = Vector3.Distance(cursorPos, p2) < ERASE_BRUSH_SIZE;

                if (!p1Inside && !p2Inside)
                {
                    newCurves[newCurveLastIndex].Add(p1);
                }
                else if (!p1Inside && p2Inside)
                {
                    newCurves[newCurveLastIndex].Add(p1);
                    Vector2 intersection = CircleSegmentIntersection(p1, p2, cursorPos, ERASE_BRUSH_SIZE);
                    newCurves[newCurveLastIndex].Add(new Vector3(intersection.x, 0, intersection.y));
                    newCurves.Add(new List<Vector3>());
                    newCurveLastIndex++;
                }
                else if (p1Inside && !p2Inside)
                {
                    Vector2 intersection = CircleSegmentIntersection(p1, p2, cursorPos, ERASE_BRUSH_SIZE);
                    newCurves[newCurveLastIndex].Add(new Vector3(intersection.x, 0, intersection.y));
                }

                if (j == curve.points.Count - 2 && !p2Inside)
                {
                    newCurves[newCurveLastIndex].Add(p2);
                }
            }

            // Vérification des courbes valides après suppression
            List<Curve> validCurves = new List<Curve>();
            foreach (var segment in newCurves)
            {
                if (segment.Count > 1)
                {
                   Curve newCurve = new Curve(); // Utilisation du constructeur par défaut
                    newCurve.points = new List<Vector3>(segment); 
                    validCurves.Add(newCurve);


                }
            }

            if (validCurves.Count == 0)
            {
                deletedCurves.Add(curveIndex);
            }
            else
            {
                newCurvesData.Add((curveIndex, validCurves));
            }
        }

        // Supprimer les courbes invalides et déclencher `OnCurveDeleted`
        foreach (int index in deletedCurves)
        {
            GameEvents.OnCurveDeleted.Invoke(index);
        }

        // Mettre à jour les courbes restantes et déclencher `OnCurveChanged`
        foreach (var (curveIndex, curves) in newCurvesData)
        {
            for (int j = 0; j < curves.Count; j++)
            {
                if (j == 0)
                {
                    // Utilisation de GetWall() pour récupérer la courbe existante
                    Wall wall = wallManager.GetWall(curveIndex);
                    if (wall != null)
                    {
                        wall.curve = curves[0]; // Mise à jour de la courbe
                        GameEvents.OnCurveChanged.Invoke(curveIndex);
                    }
                    else
                    {
                        Debug.LogWarning($"WallManager: Impossible de mettre à jour la courbe {curveIndex}, elle n'existe pas.");
                    }
                }
                else
                {
                    // Création d'une nouvelle courbe et ajout direct au WallManager
                    int newIndex = wallManager.NewWall(curves[j]);
                    GameEvents.OnCurveChanged.Invoke(newIndex);
                }
            }
        }

    }

    // Calcul d'intersection entre un segment et un cercle
    private Vector2 CircleSegmentIntersection(Vector3 start, Vector3 end, Vector3 center, float radius)
    {
        int subdivisions = 50;
        float minDist = Mathf.Abs(radius - Vector3.Distance(start, center));

        for (int i = 1; i <= subdivisions; i++)
        {
            float t = i / (float)subdivisions;
            Vector3 p = Vector3.Lerp(start, end, t);
            float dist = Mathf.Abs(radius - Vector3.Distance(p, center));

            if (dist < minDist)
            {
                minDist = dist;
            }
            else
            {
                return new Vector2(p.x, p.z);
            }
        }

        return new Vector2(end.x, end.z);
    }
}
