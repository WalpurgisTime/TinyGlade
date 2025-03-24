using System;

/*
public class MainCamera
{
    public Camera Camera { get; private set; }
    public CameraRig CameraRig { get; private set; }

    private const float CameraFOVDegrees = 45.0f;

    /*
    public MainCamera(float aspectRatio)
    {
        CameraRig = new CameraRig()
            .WithYawPitch(45.0f, -30.0f)
            .WithPosition(Vector3.Zero)
            .WithSmooth(1.0f, 1.0f)
            .WithArm(Vector3.UnitZ * 9.0f);

        Camera = new Camera(Transform.Identity, MathHelper.DegreesToRadians(CameraFOVDegrees), aspectRatio);
    }

    
}


*/
/*

public class Camera
{
    public Matrix4 Transform { get; private set; }
    public Matrix4 PerspectiveProjection { get; private set; }

    public Camera(Transform transform, float fov, float aspectRatio)
    {
        Transform = transform.ComputeMatrix();
        PerspectiveProjection = Matrix4.CreatePerspectiveFieldOfView(fov, aspectRatio, 0.1f, 100.0f);
    }

    public Matrix4 WorldToCameraView()
    {
        return Transform.Inverted();
    }

    public Vector3 Position()
    {
        Transform.Decompose(out _, out _, out Vector3 translation);
        return translation;
    }
}

public class CameraRig
{
    private Vector3 _position;
    private float _yaw;
    private float _pitch;
    private Vector3 _armOffset;
    private float _smoothPosition;
    private float _smoothRotation;

    public CameraRig WithYawPitch(float yaw, float pitch)
    {
        _yaw = yaw;
        _pitch = pitch;
        return this;
    }

    public CameraRig WithPosition(Vector3 position)
    {
        _position = position;
        return this;
    }

    public CameraRig WithSmooth(float smoothPosition, float smoothRotation)
    {
        _smoothPosition = smoothPosition;
        _smoothRotation = smoothRotation;
        return this;
    }

    public CameraRig WithArm(Vector3 armOffset)
    {
        _armOffset = armOffset;
        return this;
    }
}

*/