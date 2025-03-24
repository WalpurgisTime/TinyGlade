using System;
using System.Collections.Generic;


// Represents a Cube mesh
public struct Cube
{
    public float Size;

    public Cube(float size)
    {
        Size = size;
    }

    public static Cube Default => new Cube(1.0f);
}

// Represents a Box mesh
public struct Box
{
    public float MinX, MaxX, MinY, MaxY, MinZ, MaxZ;

    public Box(float xLength, float yLength, float zLength)
    {
        MaxX = xLength / 2.0f;
        MinX = -xLength / 2.0f;
        MaxY = yLength / 2.0f;
        MinY = -yLength / 2.0f;
        MaxZ = zLength / 2.0f;
        MinZ = -zLength / 2.0f;
    }

    public static Box Default => new Box(2.0f, 1.0f, 1.0f);
}