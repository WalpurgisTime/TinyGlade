using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum GlUniformType
{
    Bool,
    Int,
    Float,
    Vec2,
    Vec3,
    Vec4,
    Mat4
}

public enum ShaderType
{
    Graphics,
    Compute
}

public struct ShaderProgramId
{
    public int Id;
    public ShaderProgramId(int id) => Id = id;
}

public class ShaderProgram
{
    public ShaderProgramId Id;
    public ShaderType Type;

    public Shader Shader;
    public ComputeShader Compute;

    public Material Material;

    public string VertexShaderPath;
    public string FragmentShaderPath;
    public string ComputeShaderPath;

    private static int nextId = 1;

    // -----------------------------------------------------------------------
    //                              Création
    // -----------------------------------------------------------------------

    public static ShaderProgram New(Shader shader, string vertexShaderPath = "", string fragmentShaderPath = "")
    {
        if (shader == null)
            throw new ArgumentNullException(nameof(shader), "Shader cannot be null.");

        Material mat = new Material(shader);

        var prog = new ShaderProgram
        {
            Type = ShaderType.Graphics,
            Id = new ShaderProgramId(nextId++),
            Shader = shader,
            Material = mat,
            VertexShaderPath = vertexShaderPath,
            FragmentShaderPath = fragmentShaderPath
        };

        prog.set_gl_uniform("_ShaderId", GlUniformType.Int, prog.id());
        return prog;
    }

    public static ShaderProgram NewCompute(ComputeShader computeShader, string computeShaderPath = "")
    {
        if (computeShader == null)
            throw new ArgumentNullException(nameof(computeShader), "ComputeShader cannot be null.");

        var prog = new ShaderProgram
        {
            Type = ShaderType.Compute,
            Id = new ShaderProgramId(nextId++),
            Compute = computeShader,
            ComputeShaderPath = computeShaderPath
        };

        prog.set_gl_uniform_compute(0, "_ShaderId", GlUniformType.Int, prog.id());
        return prog;
    }

    // -----------------------------------------------------------------------
    //                             Utilisation
    // -----------------------------------------------------------------------

    public void gl_use_program()
    {
        if (Type == ShaderType.Graphics && Material == null)
            throw new InvalidOperationException("ShaderProgram has no material.");
        // En Unity, le Material est appliqué via un Renderer ou un DrawCall.
    }

    public int id() => Id.Id;

    public void recompile()
    {
        Debug.LogWarning("Shader recompilation is not supported at runtime in Unity.");
    }

    public List<string> src_paths()
    {
        List<string> paths = new List<string>();
        if (Type == ShaderType.Graphics)
        {
            if (!string.IsNullOrEmpty(VertexShaderPath)) paths.Add(VertexShaderPath);
            if (!string.IsNullOrEmpty(FragmentShaderPath)) paths.Add(FragmentShaderPath);
        }
        else
        {
            if (!string.IsNullOrEmpty(ComputeShaderPath)) paths.Add(ComputeShaderPath);
        }
        return paths;
    }

    // -----------------------------------------------------------------------
    //                           Uniforms (Material)
    // -----------------------------------------------------------------------

    public void set_gl_uniform(string name, GlUniformType type, object value)
    {
        if (Type != ShaderType.Graphics || Material == null)
            throw new InvalidOperationException("Use set_gl_uniform_compute for compute shaders or material missing.");

        switch (type)
        {
            case GlUniformType.Bool: Material.SetInt(name, (bool)value ? 1 : 0); break;
            case GlUniformType.Int: Material.SetInt(name, (int)value); break;
            case GlUniformType.Float: Material.SetFloat(name, (float)value); break;
            case GlUniformType.Vec2: Material.SetVector(name, (Vector2)value); break;
            case GlUniformType.Vec3: Material.SetVector(name, (Vector3)value); break;
            case GlUniformType.Vec4: Material.SetVector(name, (Vector4)value); break;
            case GlUniformType.Mat4: Material.SetMatrix(name, (Matrix4x4)value); break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public void set_gl_uniform_compute(int kernel, string name, GlUniformType type, object value)
    {
        if (Type != ShaderType.Compute || Compute == null)
            throw new InvalidOperationException("Use set_gl_uniform for graphics shaders or ComputeShader missing.");

        switch (type)
        {
            case GlUniformType.Bool: Compute.SetInt(name, (bool)value ? 1 : 0); break;
            case GlUniformType.Int: Compute.SetInt(name, (int)value); break;
            case GlUniformType.Float: Compute.SetFloat(name, (float)value); break;
            case GlUniformType.Vec2: Compute.SetVector(name, (Vector2)value); break;
            case GlUniformType.Vec3: Compute.SetVector(name, (Vector3)value); break;
            case GlUniformType.Vec4: Compute.SetVector(name, (Vector4)value); break;
            case GlUniformType.Mat4: Compute.SetMatrix(name, (Matrix4x4)value); break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public void DispatchCompute(int kernel, int x, int y, int z)
    {
        if (Type != ShaderType.Compute || Compute == null)
            throw new InvalidOperationException("Not a compute shader or missing ComputeShader.");

        Compute.Dispatch(kernel, x, y, z);
    }

    // -----------------------------------------------------------------------
    //                        Équivalents Rust / OpenGL
    // -----------------------------------------------------------------------

    public static string read_file_to_string(string relativePath)
    {
        string fullPath = Path.Combine(Application.dataPath, relativePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Shader file not found at: {fullPath}");

        return File.ReadAllText(fullPath);
    }

    public static Shader compile_shader(string shaderName)
    {
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
            throw new Exception($"Shader '{shaderName}' not found. Ensure it is in the project.");
        return shader;
    }

    public static Material link_shader_program(Shader shader)
    {
        return new Material(shader);
    }
}
