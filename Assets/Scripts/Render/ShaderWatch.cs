using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShaderWatcher : MonoBehaviour
{
    private FileSystemWatcher fileWatcher;
    private HashSet<string> changedShaders = new HashSet<string>();
    private readonly object lockObject = new object();

    public event Action<string> OnShaderChanged;

    [SerializeField] private string shaderFolderPath = "Assets/shaders";

    void Start()
    {
        if (!Directory.Exists(shaderFolderPath))
        {
            Debug.LogError($"ShaderWatcher: Le dossier '{shaderFolderPath}' n'existe pas !");
            return;
        }

        fileWatcher = new FileSystemWatcher(shaderFolderPath, "*.shader");
        fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
        fileWatcher.Changed += OnFileChanged;
        fileWatcher.EnableRaisingEvents = true;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        lock (lockObject)
        {
            changedShaders.Add(e.FullPath);
        }
    }

    void Update()
    {
        lock (lockObject)
        {
            foreach (var shaderPath in changedShaders)
            {
                Debug.Log($"ShaderWatcher: Shader modifié -> {shaderPath}");
                OnShaderChanged?.Invoke(shaderPath);
            }
            changedShaders.Clear();
        }
    }

    void OnDestroy()
    {
        if (fileWatcher != null)
        {
            fileWatcher.Changed -= OnFileChanged;
            fileWatcher.Dispose();
        }
    }
}
