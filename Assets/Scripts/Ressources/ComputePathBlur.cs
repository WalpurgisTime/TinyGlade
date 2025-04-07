
using UnityEngine;

public class ComputePathBlur : MonoBehaviour
{
    public ComputeTexture Texture { get; private set; }

    public ComputePathBlur(ComputeTexture texture)
    {
        Texture = texture;
    }

    public RenderTexture Output => Texture.RenderTex;
}
