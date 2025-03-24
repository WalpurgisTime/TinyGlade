using System;
using UnityEngine;
using UnityEngine.Events;

// Classe contenant les événements utilisés dans Unity
public static class GameEvents
{
    // Événement pour quand une courbe est modifiée
    public class CurveChangedEvent : UnityEvent<int> { }
    public static CurveChangedEvent OnCurveChanged = new CurveChangedEvent();

    // Événement pour quand une courbe est supprimée
    public class CurveDeletedEvent : UnityEvent<int> { }
    public static CurveDeletedEvent OnCurveDeleted = new CurveDeletedEvent();

    // Événement pour quand le mode de pinceau change
    public class BrushModeChangedEvent : UnityEvent<BrushMode> { }
    public static BrushModeChangedEvent OnBrushModeChanged = new BrushModeChangedEvent();
}
