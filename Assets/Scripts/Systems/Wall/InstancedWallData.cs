using System.Collections.Generic;
using UnityEngine;

public class InstancedWallData : MonoBehaviour
{
    public InstancedWall WallData { get; private set; }

    public void Initialize(InstancedWall wallData)
    {
        WallData = wallData;
    }

    public void UpdateWall(float curveLength, List<Brick> bricks)
    {
        WallData.UpdateWall(curveLength, bricks);
    }
}
