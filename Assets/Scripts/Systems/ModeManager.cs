using UnityEngine;

public class ModeManager : MonoBehaviour
{
    public static BrushMode CurrentMode { get; private set; } = BrushMode.Wall;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            ChangeMode(BrushMode.Wall);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) 
        {
            ChangeMode(BrushMode.Path);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) 
        {
            ChangeMode(BrushMode.Eraser);
        }

    }

    private void ChangeMode(BrushMode newMode)
    {
        CurrentMode = newMode;
        Debug.Log($"Mode changé : {newMode}");
        GameEvents.OnBrushModeChanged.Invoke(newMode);
    }
}


public enum EraseLayer
{
    All,
    Wall,
    Path
}

public enum BrushMode
{
    Wall,
    Path,
    Eraser,
    EraserAll,
    EraserWall,
    EraserPath
}
