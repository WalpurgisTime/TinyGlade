using UnityEngine;
using System.Collections.Generic;

public class SSBODelete : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   private static readonly List<ComputeBuffer> ssboToDelete = new();

    private static readonly object locker = new();

    public static void MarkForDeletion(ComputeBuffer buffer)
    {
        lock (locker)
        {
            if (buffer != null)
            {
                ssboToDelete.Add(buffer);
            }
        }
    }

    public static void DeleteDroppedSSBOs()
    {
        lock (locker)
        {
            foreach (var buffer in ssboToDelete)
            {
                buffer.Release();
            }
            ssboToDelete.Clear();
        }
    }
}
