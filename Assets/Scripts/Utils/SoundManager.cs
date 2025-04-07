using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource audioSource;

    public AudioClip wallPopClip;
    public AudioClip brickBuildClip;
    public AudioClip pathDrawClip;
    public AudioClip eraserScratchClip;

    private static Dictionary<string, float> lastPlayedTime = new();
    private static Dictionary<string, float> soundCooldowns = new()
    {
        { "brick_build", 0.2f },
        { "path_draw", 0.1f },
        { "eraser_scratch", 0.1f },
        { "wall_pop", 0f }
    };

    void Awake()
    {
        instance = this;
    }

    public static void Play(string soundName)
    {
        if (instance == null || instance.audioSource == null) return;

        float lastTime = lastPlayedTime.ContainsKey(soundName) ? lastPlayedTime[soundName] : -999f;
        float cooldown = soundCooldowns.ContainsKey(soundName) ? soundCooldowns[soundName] : 0f;

        if (soundName != "brick_build" && Time.time - lastTime < cooldown) return;

        lastPlayedTime[soundName] = Time.time;

        switch (soundName)
        {
            case "wall_pop":
                instance.audioSource.PlayOneShot(instance.wallPopClip);
                break;

            case "brick_build":
                if (instance.audioSource.isPlaying && instance.audioSource.clip == instance.brickBuildClip)
                    return;

                instance.audioSource.DOKill();
                instance.audioSource.Stop();
                instance.audioSource.clip = instance.brickBuildClip;
                instance.audioSource.volume = 0.6f;
                instance.audioSource.pitch = 1.4f;
                instance.audioSource.loop = false;
                instance.audioSource.Play();
                break;

            case "path_draw":
                if (instance.audioSource.isPlaying && instance.audioSource.clip == instance.pathDrawClip)
                    return;

                instance.audioSource.DOKill();
                instance.audioSource.Stop();
                instance.audioSource.clip = instance.pathDrawClip;
                instance.audioSource.volume = 0.6f;
                instance.audioSource.pitch = 1.3f;
                instance.audioSource.loop = false;
                instance.audioSource.Play();
                break;

            case "eraser_scratch":
                if (instance.audioSource.isPlaying && instance.audioSource.clip == instance.eraserScratchClip)
                    return;

                instance.audioSource.DOKill();
                instance.audioSource.Stop();
                instance.audioSource.clip = instance.eraserScratchClip;
                instance.audioSource.volume = 0.6f;
                instance.audioSource.pitch = 1.3f;
                instance.audioSource.loop = false;
                instance.audioSource.Play();
                break;
        }
    }


    void OnEnable()
    {
        GameEvents.OnMiddleMouseReleased.AddListener(FadeOutOnRelease);
    }

    void OnDisable()
    {
        GameEvents.OnMiddleMouseReleased.RemoveListener(FadeOutOnRelease);
    }

    private void FadeOutOnRelease()
    {
        FadeOut(0.4f); 
    }

    public static void FadeOut(float duration = 0.5f)
    {
        if (instance == null || instance.audioSource == null) return;

        instance.audioSource.DOKill();
        instance.audioSource.DOFade(0f, duration).OnComplete(() =>
        {
            instance.audioSource.Stop();
            instance.audioSource.volume = 1f;
        });
    }
}
