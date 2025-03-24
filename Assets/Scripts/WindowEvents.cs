using UnityEngine;
using UnityEngine.EventSystems;

public class WindowEvents : MonoBehaviour
{
    public delegate void CursorMovedEvent(Vector2 position, Vector2 delta);
    public static event CursorMovedEvent OnCursorMoved;

    public delegate void WindowResizedEvent(int width, int height);
    public static event WindowResizedEvent OnWindowResized;

    void Update()
    {
        ProcessKeyboardInput();
        ProcessMouseInput();
    }

    void ProcessKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Quitter l'application.");
            Application.Quit();
        }
    }

    void ProcessMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Clic gauche détecté.");
        }
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Clic droit détecté.");
        }
        if (Input.mouseScrollDelta.y != 0)
        {
            Debug.Log($"Molette de la souris : {Input.mouseScrollDelta.y}");
        }

        Vector2 currentMousePosition = Input.mousePosition;
        Vector2 previousMousePosition = currentMousePosition - new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        OnCursorMoved?.Invoke(currentMousePosition, currentMousePosition - previousMousePosition);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            Debug.Log("Fenêtre désactivée.");
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application quittée.");
    }

    void OnRectTransformDimensionsChange()
    {
        int width = Screen.width;
        int height = Screen.height;
        OnWindowResized?.Invoke(width, height);
    }
}