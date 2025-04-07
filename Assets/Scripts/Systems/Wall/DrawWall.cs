using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DrawWallSystem : MonoBehaviour
{
    public BrushMode CurrentMode = BrushMode.Wall;

    public WallManager wallManager;
    public InputAction leftClickAction;
    public Camera mainCamera;
    public MouseRaycast mouseRaycast;
    public WallManagerDebugger wallManagerDebugger;

    private const float CONTINUE_CURVE_DIST_THRESHOLD = 0.2f;
    private const float DIST_THRESHOLD = 0.001f;

    private bool isDrawing = false;
    private int activeCurveIndex = -1;
    private AddPointsTo drawMode = AddPointsTo.End;

    private void OnEnable()
    {
        leftClickAction.Enable();
    }

    private void OnDisable()
    {
        leftClickAction.Disable();
    }

    void Update()
    {
        if (CurrentMode != BrushMode.Wall) return;

        Vector3 cursor_ws = mouseRaycast.cursorWorldPosition;
        cursor_ws.y = 0.0f;

        if (leftClickAction.WasPressedThisFrame())
        {
            isDrawing = true;

            var continuation = FindNearbyCurve(cursor_ws);
            if (continuation != null)
            {
                var result = continuation.Value;
                activeCurveIndex = result.index;
                var tempCurve = result.curve;
                drawMode = result.mode;
                wallManagerDebugger.wallManager.tempCurve = new InProgressCurve(tempCurve, activeCurveIndex, drawMode);
            }
            else
            {
                activeCurveIndex = wallManagerDebugger.wallManager.NewWall(new Curve());
                wallManagerDebugger.wallManager.tempCurve = new InProgressCurve(new Curve(), activeCurveIndex, AddPointsTo.End);
                drawMode = AddPointsTo.End;
            }

            GameEvents.OnCurveChanged.Invoke(activeCurveIndex);
        }
        else if (leftClickAction.IsPressed() && isDrawing && wallManagerDebugger.wallManager.tempCurve != null)
        {
            var temp = wallManagerDebugger.wallManager.tempCurve;
            var active_curve = temp.curve;

            int ptIndex = drawMode == AddPointsTo.End ? active_curve.points.Count - 1 : 0;

            bool shouldAdd =
                active_curve.points.Count == 0 ||
                Vector3.Distance(active_curve.points[ptIndex], cursor_ws) > DIST_THRESHOLD;

            if (shouldAdd)
            {
                if (drawMode == AddPointsTo.End)
                    active_curve.points.Add(cursor_ws);
                else
                    active_curve.points.Insert(0, cursor_ws);

                if (active_curve.points.Count > 2)
                {
                    Curve clone_temp_curve = new Curve(active_curve.points);
                    clone_temp_curve.Smooth(4);
                    clone_temp_curve.Resample(0.05f);
                    Wall wall = wallManagerDebugger.wallManager.GetWall(temp.index);
                    if (wall != null)
                    {
                        wall.curve = clone_temp_curve;
                    }
                    else
                    {
                        Debug.LogWarning($"[DrawWall] Can't set curve: Wall at index {temp.index} not found.");
                    }

                }

                GameEvents.OnCurveChanged.Invoke(temp.index);
            }
        }
    }

    private (int index, Curve curve, AddPointsTo mode)? FindNearbyCurve(Vector3 pos)
    {
        if (wallManagerDebugger.wallManager == null || wallManagerDebugger.wallManager.walls == null)
            return null;

        foreach (var pair in wallManagerDebugger.wallManager.walls)
        {
            int idx = pair.Key;
            Curve curve = pair.Value.curve;

            if (curve == null || curve.points == null || curve.points.Count == 0)
                continue;

            if (Vector3.Distance(curve.points[^1], pos) < CONTINUE_CURVE_DIST_THRESHOLD)
                return (idx, new Curve(curve.points), AddPointsTo.End);

            if (Vector3.Distance(curve.points[0], pos) < CONTINUE_CURVE_DIST_THRESHOLD)
                return (idx, new Curve(curve.points), AddPointsTo.Beginning);
        }

        return null;
    }
}
