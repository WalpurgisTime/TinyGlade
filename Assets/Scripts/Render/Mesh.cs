using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
/*


public class Mesh
{
    public Dictionary<string, VertexAttributeValues> Attributes { get; private set; }
    public List<uint> Indices { get; private set; }

    public const string ATTRIBUTE_POSITION = "Vertex_Position";
    public const string ATTRIBUTE_COLOR = "Vertex_Color";
    public const string ATTRIBUTE_UV = "Vertex_UV";
    public const string ATTRIBUTE_NORMAL = "Vertex_Normal";

    public Mesh()
    {
        Attributes = new Dictionary<string, VertexAttributeValues>();
        Indices = new List<uint>();
    }

    public bool IsValid()
    {
        return Indices.Count > 0;
    }

    public void SetAttribute(string name, VertexAttributeValues values)
    {
        if (Attributes.ContainsKey(name))
        {
            Attributes[name] = values;
        }
        else
        {
            Attributes.Add(name, values);
        }
    }

    public void SetIndices(List<uint> indices)
    {
        Indices = indices;
    }

    public void AddColor(float[] color)
    {
        if (Attributes.ContainsKey(ATTRIBUTE_POSITION))
        {
            int length = Attributes[ATTRIBUTE_POSITION].ArrayLength();
            List<float[]> colorValues = new List<float[]>();
            for (int i = 0; i < length; i++)
            {
                colorValues.Add(color);
            }
            SetAttribute(ATTRIBUTE_COLOR, new VertexAttributeValues.Float32x3(colorValues));
        }
    }

    public void AddColorSelf(float[] color)
    {
        AddColor(color);
    }

    public void AddUV()
    {
        if (Attributes.ContainsKey(ATTRIBUTE_POSITION))
        {
            int length = Attributes[ATTRIBUTE_POSITION].ArrayLength();
            List<float[]> uvValues = new List<float[]>();
            for (int i = 0; i < length; i++)
            {
                uvValues.Add(new float[] { 0.0f, 0.0f });
            }
            SetAttribute(ATTRIBUTE_UV, new VertexAttributeValues.Float32x2(uvValues));
        }
    }
}

public abstract class VertexAttributeValues
{
    public abstract int SizeInBytes();
    public abstract IntPtr AsPtr();
    public abstract int Size();
    public abstract VertexAttribPointerType GLType();
    public abstract int Stride();
    public abstract int ArrayLength();

    public class Sint32 : VertexAttributeValues
    {
        private List<int> Values;
        public Sint32(List<int> values) { Values = values; }
        public override int SizeInBytes() => Values.Count * Marshal.SizeOf<int>();
        public override IntPtr AsPtr() => Marshal.UnsafeAddrOfPinnedArrayElement(Values.ToArray(), 0);
        public override int Size() => 1;
        public override VertexAttribPointerType GLType() => VertexAttribPointerType.Int;
        public override int Stride() => Size() * Marshal.SizeOf<int>();
        public override int ArrayLength() => Values.Count;
    }

    public class Float32 : VertexAttributeValues
    {
        private List<float> Values;
        public Float32(List<float> values) { Values = values; }
        public override int SizeInBytes() => Values.Count * Marshal.SizeOf<float>();
        public override IntPtr AsPtr() => Marshal.UnsafeAddrOfPinnedArrayElement(Values.ToArray(), 0);
        public override int Size() => 1;
        public override VertexAttribPointerType GLType() => VertexAttribPointerType.Float;
        public override int Stride() => Size() * Marshal.SizeOf<float>();
        public override int ArrayLength() => Values.Count;
    }

    public class Float32x2 : VertexAttributeValues
    {
        private List<float[]> Values;
        public Float32x2(List<float[]> values) { Values = values; }
        public override int SizeInBytes() => Values.Count * 2 * Marshal.SizeOf<float>();
        public override IntPtr AsPtr() => Marshal.UnsafeAddrOfPinnedArrayElement(Values.ToArray(), 0);
        public override int Size() => 2;
        public override VertexAttribPointerType GLType() => VertexAttribPointerType.Float;
        public override int Stride() => Size() * Marshal.SizeOf<float>();
        public override int ArrayLength() => Values.Count;
    }

    public class Float32x3 : VertexAttributeValues
    {
        private List<float[]> Values;
        public Float32x3(List<float[]> values) { Values = values; }
        public override int SizeInBytes() => Values.Count * 3 * Marshal.SizeOf<float>();
        public override IntPtr AsPtr() => Marshal.UnsafeAddrOfPinnedArrayElement(Values.ToArray(), 0);
        public override int Size() => 3;
        public override VertexAttribPointerType GLType() => VertexAttribPointerType.Float;
        public override int Stride() => Size() * Marshal.SizeOf<float>();
        public override int ArrayLength() => Values.Count;
    }

    public class Float32x4 : VertexAttributeValues
    {
        private List<float[]> Values;
        public Float32x4(List<float[]> values) { Values = values; }
        public override int SizeInBytes() => Values.Count * 4 * Marshal.SizeOf<float>();
        public override IntPtr AsPtr() => Marshal.UnsafeAddrOfPinnedArrayElement(Values.ToArray(), 0);
        public override int Size() => 4;
        public override VertexAttribPointerType GLType() => VertexAttribPointerType.Float;
        public override int Stride() => Size() * Marshal.SizeOf<float>();
        public override int ArrayLength() => Values.Count;
    }
}

*/