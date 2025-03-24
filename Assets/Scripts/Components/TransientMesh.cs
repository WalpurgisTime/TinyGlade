using System;
using System.Collections.Generic;
using UnityEngine;

public static class TransientMeshManager
{
    private static readonly List<Guid> MeshesToDelete = new List<Guid>();

    public static void MarkForDeletion(Guid meshHandle)
    {
        MeshesToDelete.Add(meshHandle);
    }

    public static List<Guid> GetMeshesToDelete()
    {
        return new List<Guid>(MeshesToDelete);
    }
}

// When this component is out of scope, the associated mesh will be deleted
public class TransientMesh : MonoBehaviour
{
    public Guid MeshHandle { get; }

    public TransientMesh(Guid meshHandle)
    {
        MeshHandle = meshHandle;
    }

    ~TransientMesh()
    {
        Console.WriteLine("Transient mesh has been dropped");
        TransientMeshManager.MarkForDeletion(MeshHandle);
    }
}
