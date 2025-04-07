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
    public Dictionary<int,GameObject> wallGameObjects ;
    private int maxIndex;

    public WallManager()
    {
        tempCurve = null;
        walls = new Dictionary<int, Wall>();
        wallGameObjects = new Dictionary<int, GameObject>();
        maxIndex = 0;
    }

    public int NewWall(Curve curve)
    {
        //Debug.Log("New Wall");
        maxIndex++;
        tempCurve = new InProgressCurve(new Curve(), maxIndex, AddPointsTo.End);
        walls[maxIndex] = new Wall(curve);
        //Debug.Log("Wall Curve " + maxIndex + " | Point Count: " + curve.points.Count +
        //  " | First Point: " + (curve.points.Count > 0 ? curve.points[0].ToString("F3") : "None"));
        return maxIndex;
    }

    

    public int NewWallGameObject(GameObject wallGameObject)
    {
        wallGameObjects[maxIndex] = wallGameObject;
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

    public List<GameObject> GetWallGameObject()
    {
          
        return new List<GameObject>(wallGameObjects.Values);
    }

    public GameObject GetSingleWallGameObject(int index)
    {
        return wallGameObjects.ContainsKey(index) ? wallGameObjects[index] : null;
        
    }

    public Dictionary<int, Wall> GetWallsDictionary()
    {
        return walls;
    }

    public void RemoveWall(int index)
    {
        if (!walls.ContainsKey(index))
        {
            Debug.LogWarning($"[WallManager] Wall index {index} not found in walls.");
            return;
        }

        Wall wallToRemove = walls[index];

        GameObject wallGameObject = null;
        if (wallGameObjects.ContainsKey(index))
        {
            wallGameObject = wallGameObjects[index];
        }

        DespawnIfExists(wallToRemove.curvePreviewEntity);
        DespawnIfExists(wallToRemove.wallEntity);
        DespawnIfExists(wallToRemove.shadowEntity);
        DespawnIfExists(wallGameObject);

        if (wallGameObjects.ContainsKey(index))
            wallGameObjects.Remove(index);

        walls.Remove(index);

        //Debug.Log($"[WallManager] Wall index {index} removed successfully.");
    }

    private void DespawnIfExists(GameObject entity)
    {
        if (entity != null)
        {
            UnityEngine.Object.Destroy(entity);
        }
    }

    public void RemoveWallsWithoutEntities()
    {
        List<int> toRemove = new List<int>();
        foreach (var pair in walls)
        {
            int index = pair.Key;
            Wall wall = pair.Value;

            if (wall.wallEntity == null)
            {
                if (wall.curvePreviewEntity != null)
                {
                    UnityEngine.Object.Destroy(wall.curvePreviewEntity);
                }
                toRemove.Add(index);
            }

            /*
            if (wall.curve.length < 0.05f)
            {
                GameEvents.OnCurveDeleted.Invoke(index);
            }
            */
        }

        foreach (int index in toRemove)
        {
            walls.Remove(index);
            if (wallGameObjects.ContainsKey(index))
            {
                wallGameObjects.Remove(index);
            }
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