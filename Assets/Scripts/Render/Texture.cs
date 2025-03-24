using System;

using UnityEngine;

/*
public class GlTextureRGBAf32
{
    public int Id { get; private set; }
    public (int Width, int Height) Dims { get; private set; }

    public GlTextureRGBAf32((int, int) dims, float[] rawF32Pixels = null)
    {
        Dims = dims;

        Id = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, Id);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

        IntPtr pixelData = rawF32Pixels != null
            ? rawF32Pixels.AsMemory().Pin().Pointer
            : IntPtr.Zero;

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f,
                      Dims.Width, Dims.Height, 0, PixelFormat.Rgba, PixelType.Float, pixelData);

        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Update(float[] rawF32Pixels)
    {
        if (rawF32Pixels == null || rawF32Pixels.Length != Dims.Width * Dims.Height * 4)
        {
            throw new ArgumentException("Invalid pixel data size.");
        }

        GL.BindTexture(TextureTarget.Texture2D, Id);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, Dims.Width, Dims.Height,
                         PixelFormat.Rgba, PixelType.Float, rawF32Pixels);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Clear()
    {
        GL.BindTexture(TextureTarget.Texture2D, Id);
        float[] rawPixels = new float[Dims.Width * Dims.Height * 4];

        for (int i = 0; i < rawPixels.Length; i += 4)
        {
            rawPixels[i] = 0.0f;      // R
            rawPixels[i + 1] = 0.0f;  // G
            rawPixels[i + 2] = 0.0f;  // B
            rawPixels[i + 3] = 1.0f;  // A
        }

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f,
                      Dims.Width, Dims.Height, 0, PixelFormat.Rgba, PixelType.Float, rawPixels);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }
}
*/