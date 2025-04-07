using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;


public class ModeManager : MonoBehaviour
{
    public static BrushMode CurrentMode { get; private set; } = BrushMode.Wall;

    public GameObject wallGroup;
    public GameObject pathGroup;
    public GameObject eraserGroup;

    [Range(0f, 1f)] public float inactiveAlpha = 0.3f;
    [Range(0f, 1f)] public float activeAlpha = 1f;
    public float fadeDuration = 0.3f;

    void Start()
    {
        ApplyAlphaToAllGroups(CurrentMode, instant: true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            ChangeMode(BrushMode.Wall);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            ChangeMode(BrushMode.Path);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            ChangeMode(BrushMode.Eraser);
    }

    private void ChangeMode(BrushMode newMode)
    {

        CurrentMode = newMode;
        //Debug.Log($"Mode changé : {newMode}");
        GameEvents.OnBrushModeChanged.Invoke(newMode);

        ApplyAlphaToAllGroups(newMode, instant: false);
    }

    private void ApplyAlphaToAllGroups(BrushMode activeMode, bool instant)
    {
        ApplyAlphaToGroup(wallGroup, activeMode == BrushMode.Wall, instant);
        ApplyAlphaToGroup(pathGroup, activeMode == BrushMode.Path, instant);
        ApplyAlphaToGroup(eraserGroup, activeMode == BrushMode.Eraser, instant);
    }

    private void ApplyAlphaToGroup(GameObject group, bool isActive, bool instant)
    {
        if (group == null) return;

        float targetAlpha = isActive ? activeAlpha : inactiveAlpha;

        // Fader toutes les CanvasGroup
        var canvasGroups = group.GetComponentsInChildren<CanvasGroup>(includeInactive: true);
        foreach (var cg in canvasGroups)
        {
            cg.DOKill();
            if (instant)
                cg.alpha = targetAlpha;
            else
                cg.DOFade(targetAlpha, fadeDuration);
        }

        // Fader toutes les Images aussi (au cas où pas de CanvasGroup)
        var images = group.GetComponentsInChildren<Image>(includeInactive: true);
        foreach (var img in images)
        {
            img.DOKill();
            Color c = img.color;
            c.a = targetAlpha;
            if (instant)
                img.color = c;
            else
                img.DOFade(targetAlpha, fadeDuration);
        }

        // Si actif → animation et son
        if (isActive && !instant)
        {
            JuicyPop(group);
            SoundManager.Play("wall_pop");
        }
    }

    private void JuicyPop(GameObject group)
    {
        var t = group.transform;
        t.localScale = Vector3.one * 0.95f;

        Sequence seq = DOTween.Sequence();
        seq.Append(t.DOScale(1.1f, 0.15f).SetEase(Ease.OutBack));
        seq.Join(t.DOPunchRotation(new Vector3(0, 0, 5f), 0.25f, 8, 1));
        seq.Append(t.DOScale(1f, 0.1f));
    }

        void OnEnable()
    {
        GameEvents.OnMiddleMousePressed.AddListener(PlayBrushModeSound);
    }

    void OnDisable()
    {
        GameEvents.OnMiddleMousePressed.RemoveListener(PlayBrushModeSound);
    }

    private void PlayBrushModeSound()
    {
        switch (CurrentMode)
        {
            case BrushMode.Wall:
                SoundManager.Play("brick_build"); // 🔊 son de construction de mur
                break;
            case BrushMode.Path:
                SoundManager.Play("path_draw"); // 🔊 son de tracé de chemin
                break;
            case BrushMode.Eraser:
                SoundManager.Play("eraser_scratch"); // 🔊 son de gomme
                break;
        }
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
