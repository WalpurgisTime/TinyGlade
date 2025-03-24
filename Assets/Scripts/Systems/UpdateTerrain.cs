using UnityEngine;

public class UpdateTerrain : MonoBehaviour
{
    public TerrainData terrain; // Référence au script TerrainData

    void Update()
    {
        if (terrain == null) return;

        if (Input.GetKey(KeyCode.Space))
        {
            terrain.offset += new Vector2(0.06f, 0.06f);
            terrain.recalculate_texture();
        }

        if (Input.GetKey(KeyCode.Q))
        {
            terrain.amp += 0.03f;
            terrain.recalculate_texture();
        }

        if (Input.GetKey(KeyCode.E))
        {
            terrain.amp -= 0.03f;
            terrain.recalculate_texture();
        }
    }
}
