using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Unity.Entities;
using UnityEngine;

/*
public class GLShaderStorageBuffer<T> where T : struct
{
    private int _id;
    private int _bufferSize;

    public int InstanceNum { get; private set; }
    public int BindingPoint { get; private set; }

    private static readonly List<int> SsboToDelete = new List<int>();

    public GLShaderStorageBuffer(T[] data, int bufferSize, int bindingPoint)
    {
        _id = CreateStorageBuffer<T>(bufferSize);
        _bufferSize = bufferSize;
        InstanceNum = data.Length;
        BindingPoint = bindingPoint;
    }

    public void Update(T[] data)
    {
        if (data.Length > _bufferSize)
        {
            throw new ArgumentException("Data size exceeds buffer size.");
        }

        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _id);
        IntPtr ptr = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.WriteOnly);

        if (ptr == IntPtr.Zero)
        {
            throw new Exception("Failed to map SSBO buffer.");
        }

        Marshal.Copy(data, 0, ptr, Math.Min(_bufferSize, data.Length));
        GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer);

        InstanceNum = data.Length;
    }

    public void UpdateElement(T data, int index)
    {
        if (index >= _bufferSize)
        {
            throw new ArgumentException("Index exceeds buffer size.");
        }

        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _id);

        int offset = Marshal.SizeOf<T>() * index;
        int size = Marshal.SizeOf<T>();

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        IntPtr dataPtr = handle.AddrOfPinnedObject();

        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)offset, size, dataPtr);

        handle.Free();

        // Unbind
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
    }

    public void Bind(ShaderProgram shaderProgram, string name)
    {
        int blockIndex = GL.GetProgramResourceIndex(shaderProgram.Id.Id, ProgramInterface.ShaderStorageBlock, name);
        GL.ShaderStorageBlockBinding(shaderProgram.Id.Id, blockIndex, BindingPoint);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, BindingPoint, _id);
    }

    public int GetGLId()
    {
        return _id;
    }

    ~GLShaderStorageBuffer()
    {
        Console.WriteLine($"SSBO {_id} has been dropped");
        SsboToDelete.Add(_id);
    }

    private static int CreateStorageBuffer<U>(int size) where U : struct
    {
        int ssbo;
        GL.GenBuffers(1, out ssbo);

        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, Marshal.SizeOf<U>() * size, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        // Unbind
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        Console.WriteLine($"Created a new storage buffer, id: {ssbo}");

        return ssbo;
    }
}
*/