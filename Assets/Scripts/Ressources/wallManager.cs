using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Wall
{
    public Curve curve;
    public GameObject curvePreviewEntity;
    public GameObject wallEntity;
    public GameObject shadowEntity;

    public Wall(Curve curve)
    {
        this.curve = curve;
        this.curvePreviewEntity = null;
        this.wallEntity = null;
        this.shadowEntity = null;
    }
}

public class WallManager 
{
    public InProgressCurve tempCurve;
    public Dictionary<int, Wall> walls;
    private int maxIndex;

    public WallManager()
    {
        tempCurve = null;
        walls = new Dictionary<int, Wall>();
        maxIndex = 0;
    }

    public int NewWall(Curve curve)
    {
        maxIndex++;
        tempCurve = new InProgressCurve(new Curve(), maxIndex, AddPointsTo.End);
        walls[maxIndex] = new Wall(curve);
        return maxIndex;
    }

    public Wall GetWall(int index)
    {
        return walls.ContainsKey(index) ? walls[index] : null;
    }

    public List<Wall> GetWalls()
    {
        return new List<Wall>(walls.Values);
    }

    public void RemoveWall(int index)
    {
        if (!walls.ContainsKey(index)) return;

        Wall wallToRemove = walls[index];

        DespawnIfExists(wallToRemove.curvePreviewEntity);
        DespawnIfExists(wallToRemove.wallEntity);
        DespawnIfExists(wallToRemove.shadowEntity);

        walls.Remove(index);
    }

    private void DespawnIfExists(GameObject entity)
    {
        if (entity != null)
        {
            UnityEngine.Object.Destroy(entity);
        }
    }

    public System.Action<int> TriggerCurveChangedEvent;

}

public enum AddPointsTo
{
    End,
    Beginning
}
public class InProgressCurve
{
    public Curve curve;
    public int index;
    public AddPointsTo mode;

    public InProgressCurve(Curve from, int index, AddPointsTo mode)
    {
        this.curve = from;
        this.index = index;
        this.mode = mode;
    }
}