using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using UnityEngine;

public class MeshBuffer
{
    public List<uint> Indices { get; set; } = new List<uint>();
    public List<UnityEngine.Vector3> Positions { get; set; } = new List<UnityEngine.Vector3>();
    public List<UnityEngine.Vector3> Normals { get; set; } = new List<UnityEngine.Vector3>();
    public List<UnityEngine.Vector4> Tangents { get; set; } = new List<UnityEngine.Vector4>();
    public List<UnityEngine.Vector2> TexCoords { get; set; } = new List<UnityEngine.Vector2>();
}

public class GLBMeshLoader : MonoBehaviour
{
    public static MeshBuffer LoadGLBAsMeshBuffer(string path)
    {
        var meshBuffer = new MeshBuffer();

        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(fs))
        {
            uint magic = reader.ReadUInt32();
            uint version = reader.ReadUInt32();
            uint length = reader.ReadUInt32();

            if (magic != 0x46546C67) // "glTF" en hexadécimal
                throw new Exception("Fichier GLB invalide.");

            uint jsonLength = reader.ReadUInt32();
            uint jsonFormat = reader.ReadUInt32(); // Devrait être 0 (JSON)

            string jsonText = Encoding.UTF8.GetString(reader.ReadBytes((int)jsonLength));

            uint binLength = reader.ReadUInt32();
            uint binFormat = reader.ReadUInt32(); // Devrait être 1 (BIN)
            byte[] binData = reader.ReadBytes((int)binLength);

            int bufferViewIndex, count, byteOffset, componentType;
            ParseJsonValue(jsonText, "accessors", out bufferViewIndex, out count, out componentType);
            ParseJsonValue(jsonText, "bufferViews", out _, out _, out byteOffset);

            if (componentType == 5123)
            {
                for (int i = 0; i < count; i++)
                    meshBuffer.Indices.Add(BitConverter.ToUInt16(binData, byteOffset + i * 2));
            }
            else if (componentType == 5126)
            {
                for (int i = 0; i < count; i++)
                {
                    float x = BitConverter.ToSingle(binData, byteOffset + i * 12);
                    float y = BitConverter.ToSingle(binData, byteOffset + i * 12 + 4);
                    float z = BitConverter.ToSingle(binData, byteOffset + i * 12 + 8);
                    meshBuffer.Positions.Add(new UnityEngine.Vector3(x, y, z));
                }
            }
        }

        return meshBuffer;
    }

    private static void ParseJsonValue(string json, string key, out int bufferView, out int count, out int componentType)
    {
        bufferView = 0;
        count = 0;
        componentType = 0;

        int index = json.IndexOf(key);
        if (index == -1) return;

        string sub = json.Substring(index);
        string[] parts = sub.Split(new char[] { '{', '}', ':', ',', '"' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == "bufferView") bufferView = int.Parse(parts[i + 1]);
            if (parts[i] == "count") count = int.Parse(parts[i + 1]);
            if (parts[i] == "componentType") componentType = int.Parse(parts[i + 1]);
        }
    }

    public static Mesh ConvertToUnityMesh(MeshBuffer meshBuffer)
    {
        Mesh mesh = new Mesh();

        mesh.vertices = meshBuffer.Positions.ToArray();
        mesh.triangles = Array.ConvertAll(meshBuffer.Indices.ToArray(), i => (int)i);
        
        if (meshBuffer.Normals.Count > 0)
            mesh.normals = meshBuffer.Normals.ToArray();

        if (meshBuffer.TexCoords.Count > 0)
            mesh.uv = meshBuffer.TexCoords.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    public static void ApplyMeshToGameObject(GameObject gameObject, Mesh mesh)
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        meshFilter.mesh = mesh;
        meshRenderer.material = new Material(Shader.Find("Standard"));
    }
}
