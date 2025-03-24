using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

public class MeshLoader : MonoBehaviour
{
    [DataContract]
    public class JsonData
    {
        [DataMember] public Indices indices { get; set; }
        [DataMember] public List<string> attributes { get; set; }
        [DataMember] public VertexPosition Vertex_Position { get; set; }
    }

    [DataContract]
    public class Indices
    {
        [DataMember] public List<int> buffer { get; set; }
    }

    [DataContract]
    public class VertexPosition
    {
        [DataMember] public List<List<float>> buffer { get; set; }
        [DataMember] public List<object> type { get; set; }
    }

    public static Mesh LoadJsonAsMesh(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"❌ Fichier JSON introuvable : {filePath}");
            return null;
        }

        string json = File.ReadAllText(filePath);
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var serializer = new DataContractJsonSerializer(typeof(JsonData));
        JsonData jsonData = (JsonData)serializer.ReadObject(ms);

        Mesh mesh = new Mesh();

        int vertexCount = jsonData.Vertex_Position.buffer.Count;
        Vector3[] vertices = new Vector3[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            if (jsonData.Vertex_Position.buffer[i].Count != 3)
            {
                Debug.LogError($"❌ ERREUR : Le sommet {i} n'a pas exactement 3 valeurs !");
                return null;
            }

            vertices[i] = new Vector3(
                jsonData.Vertex_Position.buffer[i][0],
                jsonData.Vertex_Position.buffer[i][1],
                jsonData.Vertex_Position.buffer[i][2]
            );
        }
        mesh.vertices = vertices;

        int maxIndex = vertexCount - 1;
        List<int> validIndices = new List<int>();

        foreach (var index in jsonData.indices.buffer)
        {
            if (index <= maxIndex)
            {
                validIndices.Add(index);
            }
            else
            {
                Debug.LogWarning($"⚠️ Indice {index} ignoré car hors limites (max: {maxIndex}) !");
            }
        }

        if (validIndices.Count < 3)
        {
            Debug.LogError("❌ ERREUR : Trop peu d'indices valides pour former des triangles !");
            return null;
        }

        mesh.triangles = validIndices.ToArray();
        mesh.RecalculateNormals();

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        mesh.uv = uvs;

        mesh.RecalculateBounds();

        //Debug.Log($"✅ Mesh chargé avec {vertexCount} sommets et {validIndices.Count / 3} triangles !");
        return mesh;
    }
}
