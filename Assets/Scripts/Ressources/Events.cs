using System;
using UnityEngine;
using UnityEngine.Events;

// Classe contenant les �v�nements utilis�s dans Unity
public static class GameEvents
{
    // �v�nement pour quand une courbe est modifi�e
    public class CurveChangedEvent : UnityEvent<int> { }
    public static CurveChangedEvent OnCurveChanged = new CurveChangedEvent();

    // �v�nement pour quand une courbe est supprim�e
    public class CurveDeletedEvent : UnityEvent<int> { }
    public static CurveDeletedEvent OnCurveDeleted = new CurveDeletedEvent();

    // �v�nement pour quand le mode de pinceau change
    public class BrushModeChangedEvent : UnityEvent<BrushMode> { }
    public static BrushModeChangedEvent OnBrushModeChanged = new BrushModeChangedEvent();

    public class MiddleMousePressedEvent : UnityEvent { }
    public static MiddleMousePressedEvent OnMiddleMousePressed = new MiddleMousePressedEvent();

    public class TextureChangedEvent : UnityEvent{ }
    public static TextureChangedEvent OnTextureChanged = new TextureChangedEvent();

    public class MiddleMouseReleasedEvent : UnityEvent { }
    public static MiddleMouseReleasedEvent OnMiddleMouseReleased = new MiddleMouseReleasedEvent();
}
