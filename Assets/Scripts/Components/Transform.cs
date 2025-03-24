using System;

/*
using OpenTK.Mathematics;

public struct Transform
{
    public Vector3 Translation;
    public Quaternion Rotation;
    public Vector3 Scale;

    public static readonly Transform Identity = new Transform(Vector3.Zero, Quaternion.Identity, Vector3.One);

    public Transform(Vector3 translation, Quaternion rotation, Vector3 scale)
    {
        Translation = translation;
        Rotation = rotation;
        Scale = scale;
    }

    public Matrix4 ComputeMatrix()
    {
        return Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Translation);
    }

    public static Transform FromTranslation(Vector3 translation)
    {
        return new Transform(translation, Quaternion.Identity, Vector3.One);
    }

    public static Transform FromScale(Vector3 scale)
    {
        return new Transform(Vector3.Zero, Quaternion.Identity, scale);
    }

    public static Transform FromTranslationScale(Vector3 translation, Vector3 scale)
    {
        return new Transform(translation, Quaternion.Identity, scale);
    }
}
*/