using System.Collections.Generic;
using UnityEngine;

public class UpdateCurveSSBO: MonoBehaviour
{
    private List<int> pendingChanged = new List<int>();
    private List<int> pendingDeleted = new List<int>();

    public WallManager wallManager;
    public CurveSegmentsComputePass computeIndirect;

    void OnEnable()
    {
        GameEvents.OnCurveChanged.AddListener(OnCurveChanged);
        GameEvents.OnCurveDeleted.AddListener(OnCurveDeleted);
        GameEvents.OnMiddleMousePressed.AddListener(update_curve_ssbo);
    }

    void OnDisable()
    {
        GameEvents.OnCurveChanged.RemoveListener(OnCurveChanged);
        GameEvents.OnCurveDeleted.RemoveListener(OnCurveDeleted);
        GameEvents.OnMiddleMousePressed.RemoveListener(update_curve_ssbo);
    }

    void OnCurveChanged(int index)
    {
        pendingChanged.Add(index);
    }

    void OnCurveDeleted(int index)
    {
        pendingDeleted.Add(index);
    }


    public void update_curve_ssbo()
    {
        foreach (int index in pendingChanged)
        {
            var curveData = wallManager.GetWall(index).curve;

           var data = (curveData.points.Count > 0)
            ? ComputeArchesIndirect.CurveDataSSBO.FromCurve(curveData.points.ToArray()).ToVector4Array()
            : ComputeArchesIndirect.CurveDataSSBO.Empty().ToVector4Array();


            computeIndirect.curvesBuffer.SetData(data, 0, index * ComputeArchesIndirect.CurveDataSSBO.MAX_POINTS, data.Length);
        }

        foreach (int index in pendingDeleted)
        {
            var empty = ComputeArchesIndirect.CurveDataSSBO.Empty().ToVector4Array();
            computeIndirect.curvesBuffer.SetData(empty, 0, index * ComputeArchesIndirect.CurveDataSSBO.MAX_POINTS, empty.Length);
        }

        pendingChanged.Clear();
        pendingDeleted.Clear();
    }
}