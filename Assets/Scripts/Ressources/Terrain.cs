using UnityEngine;

public class TerrainData : MonoBehaviour
{
    public int textureSize = 512; // Taille de la texture
    public float amp = 1.3f; // Amplitude du bruit
    public Vector2 offset = Vector2.zero; // Décalage du bruit
    public float minY, maxY; // Min et Max de la hauteur

    private Texture2D heightTexture; // Texture de hauteur

    void Start()
    {
        recalculate_texture();
    }

    /// <summary>
    /// Recalcule la texture de hauteur à partir du bruit de Perlin
    /// </summary>
    public void recalculate_texture()
    {
        (Color[] rawPixels, float min, float max) = raw_pixels_f32(textureSize, offset, amp);

        heightTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        heightTexture.SetPixels(rawPixels);
        heightTexture.Apply();

        minY = min;
        maxY = max;

        //Debug.Log($"Texture recalculée. Min: {minY}, Max: {maxY}");
    }

    /// <summary>
    /// Retourne la hauteur à une position donnée
    /// </summary>
    public float height_at(float x, float y)
    {
        return Mathf.PerlinNoise(offset.x + x, offset.y + y) * amp;
    }

    /// <summary>
    /// Génère un tableau de pixels basé sur le bruit de Perlin
    /// </summary>
    private (Color[], float, float) raw_pixels_f32(int size, Vector2 offset, float amp)
    {
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;
        Color[] rawPixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            float pY = (y / (float)size) * 20.0f - 10.0f;

            for (int x = 0; x < size; x++)
            {
                float pX = (x / (float)size) * 20.0f - 10.0f;
                float noiseValue = Mathf.PerlinNoise(pX + offset.x, pY + offset.y) * amp;
                rawPixels[y * size + x] = new Color(noiseValue, noiseValue, noiseValue, 1.0f);

                if (noiseValue < minValue) minValue = noiseValue;
                if (noiseValue > maxValue) maxValue = noiseValue;
            }
        }

        return (rawPixels, minValue, maxValue);
    }
}
