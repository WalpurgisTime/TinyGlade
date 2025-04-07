using System.Collections.Generic;
using UnityEngine;

public class Shader_Update : MonoBehaviour
{
    public ShaderWatcher shaderWatcher;
    public ShaderLibrary shaderLibrary;

    void OnEnable()
    {
        GameEvents.OnMiddleMousePressed.AddListener(ShaderWatch);
    }

    void OnDisable()
    {
        GameEvents.OnMiddleMousePressed.RemoveListener(ShaderWatch);
    }

    void ShaderWatch()
    {
        // Récupération des shaders modifiés
        HashSet<string> changedShaders = shaderWatcher.changedShaders;

        if (changedShaders.Count > 0)
        {
            Debug.Log("ShaderWatcher: detected changes: " + string.Join(", ", changedShaders));

            foreach (var kvp in shaderLibrary.shadersByName)
            {
                Shader shaderAsset = kvp.Value;

                /*
                // Vérifie si un chemin de fichier du shader a changé
                foreach (string path in shaderAsset.GetSourcePaths())
                {
                    if (changedShaders.Contains(path))
                    {
                        bool success = shaderAsset.Recompile();

                        if (!success)
                        {
                            Debug.LogError($"Failed to recompile shader: {path}");
                        }

                        break; // On recompilé, on peut passer au shader suivant
                    }
                }
                */
            }

            shaderWatcher.changedShaders.Clear();
        }
    }
}
