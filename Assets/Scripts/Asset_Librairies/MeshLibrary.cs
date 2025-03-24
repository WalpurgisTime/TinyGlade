
/*using System;
using System.Collections.Generic;


public class MeshLibrary
{
    private Dictionary<Guid, Mesh> meshes = new Dictionary<Guid, Mesh>();
    private Dictionary<string, Guid> byName = new Dictionary<string, Guid>();
    public List<Guid> markedAsDirty = new List<Guid>();

    public Guid Add(Mesh asset, string name = null)
    {
        Guid id = Guid.NewGuid();
        meshes[id] = asset;

        if (!string.IsNullOrEmpty(name))
        {
            byName[name] = id;
        }

        return id;
    }

    public Mesh Get(Guid id)
    {
        return meshes.TryGetValue(id, out Mesh mesh) ? mesh : null;
    }

    public Mesh GetByName(string name)
    {
        if (byName.TryGetValue(name, out Guid id))
        {
            return Get(id);
        }
        return null;
    }

    public Guid? GetHandleByName(string name)
    {
        return byName.TryGetValue(name, out Guid id) ? id : (Guid?)null;
    }

    public bool IsDirty(Guid id)
    {
        return markedAsDirty.Contains(id);
    }

    public void MarkAsDirty(Guid id)
    {
        if (!markedAsDirty.Contains(id))
        {
            markedAsDirty.Add(id);
        }
    }

    public void Remove(Guid id)
    {
        meshes.Remove(id);
        byName = new Dictionary<string, Guid>(byName);
        foreach (var key in new List<string>(byName.Keys))
        {
            if (byName[key] == id)
            {
                byName.Remove(key);
            }
        }
    }
}

*/

using System.Collections.Generic;
using UnityEngine;

public class MeshLibrary : MonoBehaviour
{
    [SerializeField]
    private Dictionary<int, Mesh> meshLibrary = new Dictionary<int, Mesh>();
    private Dictionary<string, int> byName = new Dictionary<string, int>();
    private List<int> markedAsDirty = new List<int>();

    private int handleCounter = 0;

    public int AddMesh(string name, Mesh mesh)
    {
        int handle = ++handleCounter;
        meshLibrary[handle] = mesh;
        byName[name] = handle;
        return handle;
    }

    public Mesh GetMesh(int handle)
    {
        if (meshLibrary.TryGetValue(handle, out Mesh mesh))
            return mesh;
        Debug.LogWarning($"Mesh avec Handle {handle} introuvable !");
        return null;
    }

    public Mesh GetMeshByName(string name)
    {
        if (byName.TryGetValue(name, out int handle))
            return GetMesh(handle);
        Debug.LogWarning($"Mesh '{name}' introuvable !");
        return null;
    }

    public bool IsDirty(int handle)
    {
        return markedAsDirty.Contains(handle);
    }

    public void MarkDirty(int handle)
    {
        if (!markedAsDirty.Contains(handle))
            markedAsDirty.Add(handle);
    }

    public void RemoveMesh(int handle)
    {
        meshLibrary.Remove(handle);
        byName = new Dictionary<string, int>(byName);
        foreach (var kvp in byName)
        {
            if (kvp.Value == handle)
                byName.Remove(kvp.Key);
        }
    }
}
