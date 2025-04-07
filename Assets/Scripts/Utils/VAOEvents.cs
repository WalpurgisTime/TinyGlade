using UnityEngine;
using UnityEngine.InputSystem;

public class VAOEvents : MonoBehaviour
{
    // ✅ Événement global
    public static event System.Action OnRequestVAOUpdate;

    private InputAction anyInputAction;

    /*
    private void OnEnable()
    {
        🎯 Capture clavier, souris, manette...
        anyInputAction = new InputAction(
            name: "AnyInput",
            type: InputActionType.Button,
            binding: "<Button>" // ← touche clavier, clic souris, boutons manette
        );

        anyInputAction.performed += ctx => TriggerVAOUpdate("🎯 Input detected");

        anyInputAction.Enable();
    }

    private void OnDisable()
    {
        anyInputAction.Disable();
    }

    private void TriggerVAOUpdate(string source)
    {
        //Debug.Log($"[VAOInputTrigger] Event déclenché via ");
        //OnRequestVAOUpdate?.Invoke();
    }

    */
}

