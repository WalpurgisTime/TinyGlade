using UnityEngine;

public class ComputePathMask : MonoBehaviour
{
    public ComputeTexture Texture { get; private set; }

    public ComputePathMask(ComputeTexture texture)
    {
        Texture = texture;
    }

    public ComputeTexture GetTexture()
    {
        return Texture;
    }   
}

public static class ComputeConstants
{
    public static readonly Vector2 PATH_MASK_WS_DIMS = new Vector2(20.0f, 20.0f);
}
