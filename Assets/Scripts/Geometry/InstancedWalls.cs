using System.Collections.Generic;
using UnityEngine;

public class InstancedWall : MonoBehaviour
{
    public float wallLength;
    public ComputeBuffer instanceBuffer;

    private const int SSBO_BUFFER_SIZE = 10000;
    private const int SSBO_BINDING_POINT = 2;

    [SerializeField]
    public List<Brick> bricks;

    public int instanceCount => instanceBuffer != null ? instanceBuffer.count : 0;


    public InstancedWall(float curveLength, List<Brick> bricks)
    {
        wallLength = curveLength;
        this.bricks = bricks;
        instanceBuffer = new ComputeBuffer(SSBO_BUFFER_SIZE, BrickTransformSSBO.Size);
        UpdateInstanceBuffer(bricks);
    }

    public void Init(float curveLength, List<Brick> bricks)
    {
        wallLength = curveLength;
        this.bricks = bricks;
        instanceBuffer = new ComputeBuffer(SSBO_BUFFER_SIZE, BrickTransformSSBO.Size);
        UpdateInstanceBuffer(bricks);
    }


    public void UpdateWall(float curveLength, List<Brick> bricks)
    {
        wallLength = curveLength;
        UpdateInstanceBuffer(bricks);
    }

    private void UpdateInstanceBuffer(List<Brick> bricks)
    {
        BrickTransformSSBO[] data = InstancedWallData(bricks);
        instanceBuffer.SetData(data);
    }

    private BrickTransformSSBO[] InstancedWallData(List<Brick> bricks)
    {
        BrickTransformSSBO[] data = new BrickTransformSSBO[bricks.Count];
        for (int i = 0; i < bricks.Count; i++)
        {
            Brick b = bricks[i];
            Vector2 min = b.pivotUV - b.boundsUV / 2.0f;
            Vector2 max = b.pivotUV + b.boundsUV / 2.0f;

            data[i] = new BrickTransformSSBO
            {
                transform = b.transform.ComputeMatrix(),
                curveUVBBoxMinMax = new Vector4(min.x, min.y, max.x, max.y)
            };
        }
        return data;
    }

    public void Dispose()
    {
        if (instanceBuffer != null)
        {
            instanceBuffer.Release();
            instanceBuffer = null;
        }
    }
}

public struct BrickTransformSSBO
{
    public Matrix4x4 transform;
    public Vector4 curveUVBBoxMinMax;

    public static int Size => sizeof(float) * 16 + sizeof(float) * 4;
}

