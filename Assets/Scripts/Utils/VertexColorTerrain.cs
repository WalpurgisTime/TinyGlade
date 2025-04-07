using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class VertexColorTerrain : MonoBehaviour
{
    
    [Header("Texture à afficher (ex: _PathTex ou terrain height)")]
    public TerrainData terrainTexture;
    public ShaderLibrary   shaderLibrary;

    private GameObject _Go;


    public void ShowMask(GameObject GO)
    {
        Shader vertexColorTerrainShader = shaderLibrary.GetShaderByName("terrain_shader");
        // Vérifie que le shader est valide
        if (vertexColorTerrainShader == null )
        {
            Debug.LogError("Shader VertexColorTerrain introuvable ou non supporté.");
            return;
        }

        // Crée un matériau à partir du shader
        Material mat = new Material(vertexColorTerrainShader);
        mat.SetTexture("_TerrainTex", terrainTexture.activateTex);
        // Applique le matériel
        GO.GetComponent<Renderer>().material = mat;
        _Go = GO;
    }

    public void GiveTexture()
    {

        if(_Go != null)
        {
            var mat = _Go.GetComponent<Renderer>().material;
            mat.SetTexture("_TerrainTex", terrainTexture.activateTex);

        }

    }
}
