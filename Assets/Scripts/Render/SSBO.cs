using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GLShaderStorageBuffer<T> where T : struct
{
    public ComputeBuffer id { get; private set; }
    public int buffer_size { get; private set; }
    public int instance_num { get; private set; }
    public uint binding_point { get; private set; }

    private int stride;

    public static GLShaderStorageBuffer<T> new_(List<T> data, int buffer_size, uint binding_point)
    {
        GLShaderStorageBuffer<T> ssbo = new GLShaderStorageBuffer<T>();
        ssbo.id = create_storage_buffer<T>(buffer_size);
        ssbo.buffer_size = buffer_size;
        ssbo.instance_num = data.Count;
        ssbo.binding_point = binding_point;
        ssbo.stride = Marshal.SizeOf<T>();

        ssbo.id.SetData(data);
        //Debug.Log($"[SSBO] Created a new storage buffer, id: {ssbo.gl_id()}");
        return ssbo;
    }

    public void update(List<T> data)
    {
        if (data.Count > buffer_size)
            throw new ArgumentException("Data exceeds buffer size");

        id.SetData(data);
        instance_num = data.Count;
    }

    public void update_element(T data, int index)
    {
        if (index >= buffer_size)
            throw new ArgumentOutOfRangeException();

        id.SetData(new T[] { data }, 0, index, 1);
    }

    public void bind(Material shader_program, string name)
    {
        shader_program.SetBuffer(name, id);
    }

    public int gl_id()
    {
        return id.GetNativeBufferPtr().ToInt32();
    }

    public void drop()
    {
        if (id != null)
        {
            //Debug.Log($"[SSBO] {gl_id()} has been dropped");
            id.Release();
            id = null;
        }
    }

    // �quivaut � la fonction Rust `create_storage_buffer<T>`
   public static ComputeBuffer create_storage_buffer<TType>(int size) where TType : struct
{
    int stride = Marshal.SizeOf(typeof(TType));

    if (stride <= 0 || stride > 2048 || stride % 4 != 0)
    {
        //Debug.LogError($"[❌ INVALID STRIDE] TType = {typeof(TType).Name}, stride = {stride}");
        //Debug.LogError(Environment.StackTrace); // pour savoir d'où ça vient
    }
    else
    {
        //Debug.Log($"[✅ BUFFER] {typeof(TType).Name} | size = {size} | stride = {stride}");
    }

    return new ComputeBuffer(size, stride);
}

}


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