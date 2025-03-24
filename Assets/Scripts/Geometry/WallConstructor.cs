using System;
using System.Collections.Generic;
using UnityEngine;

public class WallConstructor
{
    private const float BRICK_WIDTH = 0.2f;
    private const float BRICK_WIDTH_VARIANCE = 0.14f;

    private const float BRICK_HEIGHT = 0.2f;
    private const float BRICK_HEIGHT_VARIANCE = 0.09f;

    private const float BRICK_DEPTH = 0.2f;
    private const float BRICK_DEPTH_VARIANCE = 0.05f;

    public const float WALL_HEIGHT = 1.4f;

    public static List<Brick> FromCurve(Curve curve)
    {
        System.Random rng = new System.Random(0);
        float wallLength = curve.length;

        int rowCount = Mathf.FloorToInt(WALL_HEIGHT / BRICK_HEIGHT);
        List<float> rows = RandomSplits(rowCount, BRICK_HEIGHT_VARIANCE / WALL_HEIGHT, rng);
        int bricksPerRow = Mathf.CeilToInt(wallLength / BRICK_WIDTH);


        List<Brick> bricks = new List<Brick>();

        for (int i = 0; i < rows.Count; i++)
        {
            float rowU = rows[i];
            float brickHeight = (i + 1 < rows.Count) ?
                (rows[i + 1] - rowU) * WALL_HEIGHT :
                BRICK_HEIGHT + ((float)rng.NextDouble() - 0.5f) * BRICK_HEIGHT_VARIANCE;

            List<float> brickWidths = RandomSplits(bricksPerRow, BRICK_WIDTH_VARIANCE / wallLength, rng);
            List<Brick> brickRow = new List<Brick>();

            for (int j = 0; j < brickWidths.Count - 1; j++)
            {
                float thisU = brickWidths[j];
                float nextU = brickWidths[j + 1];

                if (i == rows.Count - 1 && rng.NextDouble() < 0.35) continue;

                float brickDepth = BRICK_DEPTH + ((float)rng.NextDouble() - 0.5f) * BRICK_DEPTH_VARIANCE;

                if (rng.NextDouble() < 0.4 && i != rows.Count - 1)
                {
                    float randomSplit = (float)(rng.NextDouble() * 0.4 + 0.3);
                    float pivotU = (nextU + thisU) / 2.0f;
                    float heightU1 = brickHeight / WALL_HEIGHT * randomSplit;
                    float heightU2 = brickHeight / WALL_HEIGHT * (1.0f - randomSplit);
                    float pivotV1 = rowU + heightU1 / 2.0f;
                    float pivotV2 = (rowU + brickHeight / WALL_HEIGHT) - heightU2 / 2.0f;
                    float widthU = nextU - thisU;
                    float widthWS = widthU * wallLength;

                    brickRow.Add(new Brick(pivotU, pivotV1, widthU, heightU1, widthWS, brickDepth));
                    brickRow.Add(new Brick(pivotU, pivotV2, widthU, heightU2, widthWS, brickDepth));
                }
                else
                {
                    float pivotU = (nextU + thisU) / 2.0f;
                    float widthU = nextU - thisU;
                    float widthWS = widthU * wallLength;

                    brickRow.Add(new Brick(pivotU, rowU + brickHeight / WALL_HEIGHT / 2.0f, widthU, brickHeight / WALL_HEIGHT, widthWS, brickDepth));
                }
            }

            foreach (var brick in brickRow)
            {
                brick.transform.translation = curve.GetPosAtU(brick.pivotUV.x);
                brick.transform.translation.y = brick.pivotUV.y * WALL_HEIGHT;

                Vector3 curveTangent = curve.GetTangentAtU(brick.pivotUV.x);
                Vector3 normal = Vector3.Cross(curveTangent, Vector3.up);
                brick.transform.rotation = Quaternion.LookRotation(normal, Vector3.up);


            }



            bricks.AddRange(brickRow);
        }


        return bricks;
    }

    private static List<float> RandomSplits(int splits, float varianceU, System.Random rng)
    {
        List<float> rowU = new List<float>();
        for (int i = 0; i <= splits; i++)
            rowU.Add(i / (float)splits);

        for (int i = 1; i < rowU.Count - 1; i++)
            rowU[i] += ((float)rng.NextDouble() - 0.5f) * varianceU;

        return rowU;
    }
}

[System.Serializable]
public class Brick
{
    public Vector2 pivotUV;
    public Vector2 boundsUV;
    public TransformData transform;

    public Brick(float pivotU, float pivotV, float widthU, float heightU, float widthWS, float depth)
    {
        pivotUV = new Vector2(pivotU, pivotV);
        boundsUV = new Vector2(widthU, heightU);
        transform = new TransformData(new Vector3(pivotU * widthWS, 0, 0), Quaternion.identity, new Vector3(widthWS, heightU * WallConstructor.WALL_HEIGHT, depth));
    }
}

[System.Serializable]
public class TransformData
{
    public Vector3 translation;
    public Quaternion rotation;
    public Vector3 scale;

    public TransformData(Vector3 translation, Quaternion rotation, Vector3 scale)
    {
        this.translation = translation;
        this.rotation = rotation;
        this.scale = scale;
    }

    public Matrix4x4 ComputeMatrix()
    {
        return Matrix4x4.TRS(translation, rotation, scale);
    }
}


