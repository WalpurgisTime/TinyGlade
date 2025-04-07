using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class Eraser : MonoBehaviour
{
    public BrushMode Mode;
    public WallManagerDebugger wallManagerDebugger;
    public MouseRaycast cursorRaycast;
    public CurvePreview curvePreview;

    public ConstructWallNShadows construct;
    //private const float ERASE_BRUSH_SIZE = 0.75f * 0.9f;
    
    [UnityEngine.Range(0.1f, 0.3f)]
    [SerializeField]
    private float eraseBrushSize = 0.1f;

    [SerializeField] private Slider eraseSlider;
    [SerializeField] private float minSize = 0.1f;
    [SerializeField] private float maxSize = 0.3f;

    
    public float ERASE_BRUSH_SIZE { get; private set; } = 0.1f;
    private void Start()
    {
        // Initialisation du slider
        eraseSlider.minValue = minSize;
        eraseSlider.maxValue = maxSize;
        eraseSlider.value = ERASE_BRUSH_SIZE;

        eraseSlider.onValueChanged.AddListener(OnSliderChanged);
    }
    private void OnSliderChanged(float value)
    {
        ERASE_BRUSH_SIZE = value;
        // Debug.Log($"Brush size updated: {CurrentBrushSize}");
    }

    void OnEnable()
    {
        GameEvents.OnMiddleMousePressed.AddListener( EraserUpdated);
    }

    void OnDisable()
    {
        GameEvents.OnMiddleMousePressed.RemoveListener( EraserUpdated);
    }


    void EraserUpdated()
    {
        if (Mode != BrushMode.Eraser) return;
        if (!Input.GetMouseButton(2)) return; // clic molette

        Vector3 cursorPos = cursorRaycast.cursorWorldPosition;
        //Debug.Log($"[Eraser] Cursor position: {cursorPos}"); // DEBUG

        List<(int, List<Curve>)> newCurvesData = new();
        List<int> deletedCurves = new();
    
        foreach (var wallEntry in wallManagerDebugger.wallManager.walls)
        {
            int curveIndex = wallEntry.Key;
            Wall wall = wallEntry.Value;
            Curve curve = wall.curve;

            //Debug.Log($"[Eraser] Processing curve {curveIndex} with {curve.points.Count} points"); // DEBUG

            List<List<Vector3>> newCurves = new() { new List<Vector3>() };
            int newCurveLastIndex = 0;

            for (int j = 0; j < curve.points.Count - 1; j++)
            {
                
                Vector3 p1 = curve.points[j];
                Vector3 p2 = curve.points[j + 1];

                /*
                bool p1Inside = Vector3.Distance(cursorPos, p1) < ERASE_BRUSH_SIZE;
                bool p2Inside = Vector3.Distance(cursorPos, p2) < ERASE_BRUSH_SIZE;

                Debug.Log($"[Eraser] Segment [{j}] - p1:{p1} ({p1Inside}), p2:{p2} ({p2Inside})"); // DEBUG
                Debug.Log($"[Eraser] Distance p1: {Vector3.Distance(cursorPos, p1)} | Distance p2: {Vector3.Distance(cursorPos, p2)} | Brush: {ERASE_BRUSH_SIZE}");
                */

                Vector2 cursor2D = new Vector2(cursorPos.x, cursorPos.z);
                Vector2 p1_2D = new Vector2(p1.x, p1.z);
                Vector2 p2_2D = new Vector2(p2.x, p2.z);

                bool p1Inside = Vector2.Distance(cursor2D, p1_2D) < ERASE_BRUSH_SIZE;
                bool p2Inside = Vector2.Distance(cursor2D, p2_2D) < ERASE_BRUSH_SIZE;

                //Debug.Log($"[Eraser] Segment [{j}] - p1:{p1} ({p1Inside}), p2:{p2} ({p2Inside}) — Cursor2D: {cursor2D}, Distances: {Vector2.Distance(cursor2D, p1_2D)}, {Vector2.Distance(cursor2D, p2_2D)}");

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
                    //Debug.Log($"[Eraser] Cut end of segment [{j}] at {intersection}"); // DEBUG
                }
                else if (p1Inside && !p2Inside)
                {
                    Vector2 intersection = CircleSegmentIntersection(p1, p2, cursorPos, ERASE_BRUSH_SIZE);
                    newCurves[newCurveLastIndex].Add(new Vector3(intersection.x, 0, intersection.y));
                    //Debug.Log($"[Eraser] Cut start of segment [{j}] at {intersection}"); // DEBUG
                }
                else
                {
                    //Debug.Log($"[Eraser] Segment [{j}] fully inside eraser — skipped"); // DEBUG
                }

                if (j == curve.points.Count - 2 && !p2Inside)
                {
                    newCurves[newCurveLastIndex].Add(p2);
                }
            }

            List<Curve> validCurves = new();
            foreach (var segment in newCurves)
            {
                if (segment.Count > 1)
                {
                    Curve newCurve = new Curve { points = new List<Vector3>(segment) };
     
                    validCurves.Add(newCurve);
                    //Debug.Log($"[Eraser] Valid sub-curve created with {segment.Count} points"); // DEBUG
                }
            }

            Wall changedWall = wallManagerDebugger.wallManager.GetWall(curveIndex);

            if (validCurves.Count == 0 || changedWall.curve.length < 0.5f) 
            {
                //Debug.Log($"[Eraser] Curve {curveIndex} has been completely deleted."); // DEBUG

                deletedCurves.Add(curveIndex);
            }
            else
            {
                //Debug.Log($"[Eraser] Curve {curveIndex} split into {validCurves.Count} parts."); // DEBUG
                newCurvesData.Add((curveIndex, validCurves));

            }
        }

        foreach (int index in deletedCurves)
        {
            //Debug.Log($"[Eraser] Sending OnCurveDeleted for curve {index}"); // DEBUG
            
            GameEvents.OnCurveDeleted.Invoke(index);
        }

        //
        foreach (var (curveIndex, curves) in newCurvesData)
        {
            for (int j = 0; j < curves.Count; j++)
            {
                if (j == 0)
                {
                    Wall wall = wallManagerDebugger.wallManager.GetWall(curveIndex);
                    if (wall != null )
                    {
                        wall.curve = curves[0];
                        construct.Erased = 1;
                        GameEvents.OnCurveChanged.Invoke(curveIndex); 
                    }
                }
                else
                {
                    if(curveIndex == curvePreview.MaxValue)
                    {
                        wallManagerDebugger.wallManager.RemoveWallsWithoutEntities();
                        return;
                    }

                    int newIndex = wallManagerDebugger.wallManager.NewWall(curves[j]);
                    construct.Erased = 2;
                    GameEvents.OnCurveChanged.Invoke(newIndex);

                    wallManagerDebugger.wallManager.NewWall(new Curve());
                    //Debug.Log($"[Eraser] New sub-curve created with index {newIndex}."); // DEBUG
                }
            }
        }
    }

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
                //Debug.Log($"[Eraser] Intersection found at t={t}, point={p}"); // DEBUG
                return new Vector2(p.x, p.z);
            }
        }

        //Debug.LogWarning("[Eraser] No sharp intersection found, using segment end"); // DEBUG
        return new Vector2(end.x, end.z);
    }
}
