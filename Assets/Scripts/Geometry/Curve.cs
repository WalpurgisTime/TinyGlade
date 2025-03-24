using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class Curve
{
    [SerializeField] public List<Vector3> points = new();
    [SerializeField] public List<float> pointsU = new();
    [SerializeField] public float length = 0.0f;

    public Curve()
    {
        points = new List<Vector3>();
        pointsU = new List<float>();
        length = 0.0f;
    }

    public Curve(List<Vector3> pts)
    {
        points = new List<Vector3>(pts);
        pointsU = new List<float>();
        RecalculateCurve();
    }


    public void AddPoint(Vector3 pt)
    {
        points.Add(pt);
        RecalculateCurve();
    }

    public void AddToFront(Vector3 pt)
    {
        points.Insert(0, pt);
        RecalculateCurve();
    }

    private void RecalculateCurve()
    {


        length = 0.0f;

        if (pointsU == null)
        {
            Debug.LogError("RecalculateCurve: pointsU list is null!");
            return;
        }

        pointsU.Clear();

        for (int i = 0; i < points.Count - 1; i++)
        {
            length += Vector3.Distance(points[i], points[i + 1]);
        }

        float lengthTraveled = 0.0f;
        for (int i = 0; i < points.Count; i++)
        {
            pointsU.Add(lengthTraveled / length);
            if (i < points.Count - 1)
            {
                lengthTraveled += Vector3.Distance(points[i], points[i + 1]);
            }
        }
    }

    public Curve Smooth(int smoothingSteps)
    {
        if (points.Count < 3)
            return this;

        for (int i = 0; i < smoothingSteps; i++)
        {
            List<Vector3> currentSmooth = new List<Vector3>(points);
            for (int j = 1; j < points.Count - 1; j++)
            {
                Vector3 avg = (points[j - 1] + points[j + 1]) / 2.0f;
                currentSmooth[j] = points[j] + (avg - points[j]) * 0.5f;
            }
            points = currentSmooth;
        }
        return this;
    }

    public Curve Resample(float segmentLength)
    {
        if (segmentLength >= length)
        {
            return new Curve { points = new List<Vector3> { points[0], points[points.Count - 1] } };
        }

        List<Vector3> newPoints = new List<Vector3>();
        float uSpacing = segmentLength / length;
        int targetPoints = Mathf.RoundToInt(1.0f / uSpacing);
        float targetUSpacing = 1.0f / targetPoints;

        for (int i = 0; i <= targetPoints; i++)
        {
            newPoints.Add(GetPosAtU(i * targetUSpacing));
        }

        return new Curve { points = newPoints };
    }

    public bool IsNear(Vector3 position, float threshold)
    {
        foreach (var point in points)
        {
            if (Vector3.Distance(point, position) < threshold)
            {
                return true;
            }
        }
        return false;
    }

    public bool CanAddPoint(Vector3 position)
    {
        return points.Count == 0 || Vector3.Distance(points[points.Count - 1], position) > 0.001f;
    }

    public void SmoothAndResample()
    {
        this.Smooth(50).Resample(0.2f);
    }

    private (int, int) GetCurveSegmentFromU(float u)
    {
        if (u == 1.0f)
            return (points.Count - 2, points.Count - 1);
        if (u == 0.0f)
            return (0, 1);

        for (int i = 1; i < pointsU.Count; i++)
        {
            if (u <= pointsU[i])
                return (i - 1, i);
        }
        return (0, 1);
    }

    public Vector3 GetPosAtU(float u)
    {
        var (idx1, idx2) = GetCurveSegmentFromU(u);
        Vector3 dir = points[idx2] - points[idx1];
        float uRangeStart = pointsU[idx1];
        float uRangeEnd = pointsU[idx2];
        float mag = (u - uRangeStart) / (uRangeEnd - uRangeStart);
        return points[idx1] + dir * mag;
    }

    public Vector3 GetTangentAtU(float u)
    {
        var (idx1, idx2) = GetCurveSegmentFromU(u);
        return (points[idx2] - points[idx1]).normalized;
    }
}