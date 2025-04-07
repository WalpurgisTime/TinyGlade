using UnityEngine;

public class TerrainData : MonoBehaviour
{
    public int seed = 0;
    public float amp = 1.3f;
    public Vector2 offset = Vector2.zero;
    public Vector2Int texture_dims = new Vector2Int(512, 512);
    public Texture2D texture;
    public Texture2D selectableTexture;
    public Texture2D activateTex;

    [SerializeField] private VertexColorTerrain vertTerrain;

    [HideInInspector] public float min_y;
    [HideInInspector] public float max_y;

    private Material material;

    private SimplePerlin perlin;

    void Awake()
    {
        New();
        ActiveTexture();
    }
    
    public void GetMaterial(Material mat)
    {
        material = mat;
    }

    void OnEnable()
    {
        GameEvents.OnTextureChanged.AddListener(ReworkedTexure);
    }


    void OnDisable()
    {
        GameEvents.OnTextureChanged.RemoveListener(ReworkedTexure);
    }

    private void ReworkedTexure()
    {
        Debug.Log("Reworked Texture");
        ActiveTexture2();
    }

    public void ActiveTexture2()
    {
        if(activateTex == null)
        {
            activateTex = selectableTexture;
        }
        else if(activateTex == selectableTexture)
        {
            activateTex = texture;
        }
        else
        {
            activateTex = texture;
        }

        vertTerrain.GiveTexture();
    }
    

    public void ActiveTexture()
    {
        if(activateTex == null)
        {
            activateTex = selectableTexture;
        }
        else if(activateTex == selectableTexture)
        {
            activateTex = texture;
        }
        else
        {
            activateTex = selectableTexture;
        }

        vertTerrain.GiveTexture();
    }
    

    public void New()
    {
        perlin = new SimplePerlin(seed);
        recalculate_texture();
    }

    

    public void recalculate_texture()
    {
        var (raw_pixels, min, max) = raw_pixels_f32();

        min_y = min;
        max_y = max;

        texture = new Texture2D(texture_dims.x, texture_dims.y, TextureFormat.RFloat, false, true);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[texture_dims.x * texture_dims.y];

        for (int i = 0; i < pixels.Length; i++)
        {
            float v = raw_pixels[i * 4];
            pixels[i] = new Color(v, v, v, 1.0f);
        }

        texture.SetPixels(pixels);
        texture.Apply();
    }

    public float height_at(float x, float y)
    {
        return perlin.fbm(offset.x + x, offset.y + y) * amp;
    }

    public (float[] raw, float min, float max) raw_empty_f32()
    {
        return (new float[texture_dims.x * texture_dims.y * 4], 0f, 0f);
    }

    public (float[] raw, float min, float max) raw_pixels_f32()
    {
        float[] raw = new float[texture_dims.x * texture_dims.y * 4];

        float min_value = float.MaxValue;
        float max_value = float.MinValue;

        for (int y = 0; y < texture_dims.y; y++)
        {
            float p_y = ((float)y / texture_dims.y) * 20.0f - 10.0f;

            for (int x = 0; x < texture_dims.x; x++)
            {
                float p_x = ((float)x / texture_dims.x) * 20.0f - 10.0f;
                float n = perlin.fbm(p_x + offset.x, p_y + offset.y) * amp;

                int i = (y * texture_dims.x + x) * 4;
                raw[i + 0] = n;
                raw[i + 1] = n;
                raw[i + 2] = n;
                raw[i + 3] = 1.0f;

                if (n < min_value) min_value = n;
                if (n > max_value) max_value = n;
            }
        }

        return (raw, min_value, max_value);
    }
}

public class SimplePerlin
{
    private int seed;
    private System.Random rng;

    public int octaves = 3;
    public float gain = 1.0f;
    public float lacunarity = 3.0f;
    public float frequency = 0.05f;

    public SimplePerlin(int seed)
    {
        this.seed = seed;
        rng = new System.Random(seed);
    }

    public float fbm(float x, float y)
    {
        float total = 0f;
        float freq = frequency;
        float amp = 1f;

        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * freq, y * freq) * amp;
            freq *= lacunarity;
            amp *= gain;
        }

        return total;
    }
} 