using UnityEngine;
using System.Collections;

public class SceneController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private LightEmissionSphere lightSphere;
    [SerializeField] private AudioSource qooboSpeaker;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] beepSounds;  // Drag your beep sound files here

    [Header("Transition Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float defaultTransparency = 1f;

    [Header("References")]
    [SerializeField] private ThoughtBubbleController thoughtBubble;

    private bool isVisible = false;
    private Coroutine fadeCoroutine;
    private bool isInitialized = false;

    void Start()
    {
        // Wait a frame to ensure LightEmissionSphere is fully initialized
        StartCoroutine(InitializeLightSphere());
    }

    private IEnumerator InitializeLightSphere()
    {
        // Wait for two frames to ensure LightEmissionSphere has completed its setup
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (lightSphere != null)
        {
            lightSphere.SetTransparency(0f);
            isInitialized = true;
        }
    }

    // Light control methods
    [ContextMenu("Show Red Light")]
    public void ShowRedLight()
    {
        if (!isInitialized) return;
        if (lightSphere != null)
        {
            lightSphere.SetColorRed();
            lightSphere.SetIntensity(1f);  // Set full intensity
            lightSphere.SetTransparency(1f);  // Set full transparency
            ShowLightSphere();
        }
    }

    [ContextMenu("Show Blue Light")]
    public void ShowBlueLight()
    {
        if (!isInitialized) return;
        if (lightSphere != null)
        {
            lightSphere.SetColorBlue();
            lightSphere.SetIntensity(1f);
            lightSphere.SetTransparency(1f);
            ShowLightSphere();
        }
    }

    [ContextMenu("Show Green Light")]   
    public void ShowGreenLight()
    {
        if (!isInitialized) return;
        if (lightSphere != null)
        {
            lightSphere.SetColorGreen();
            lightSphere.SetIntensity(1f);
            lightSphere.SetTransparency(1f);
            ShowLightSphere();
        }
    }

    [ContextMenu("Show Yellow Light")]
    public void ShowYellowLight()
    {
        if (!isInitialized) return;
        if (lightSphere != null)
        {
            lightSphere.SetColorYellow();
            lightSphere.SetIntensity(1f);
            lightSphere.SetTransparency(1f);
            ShowLightSphere();
        }
    }

    [ContextMenu("Show Purple Light")]
    public void ShowPurpleLight()
    {
        if (!isInitialized) return;
        if (lightSphere != null)
        {
            lightSphere.SetColorPurple();
            lightSphere.SetIntensity(1f);
            lightSphere.SetTransparency(1f);
            ShowLightSphere();
        }
    }

    [ContextMenu("Show Pink Light")]    
    public void ShowPinkLight()
    {
        if (!isInitialized) return;
        if (lightSphere != null)
        {
            lightSphere.SetColorPink();
            lightSphere.SetIntensity(1f);
            lightSphere.SetTransparency(1f);
            ShowLightSphere();
        }
    }

    [ContextMenu("Show Grey Light")]
    public void ShowGreyLight()
    {
        if (!isInitialized) return;
        if (lightSphere != null)
        {
            lightSphere.SetColorGrey();
            lightSphere.SetIntensity(1f);
            lightSphere.SetTransparency(1f);
            ShowLightSphere();
        }
    }

    // Visibility control methods
    public void ShowLightSphere()
    {
        if (!isInitialized) return;
        if (!isVisible)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeLight(0f, defaultTransparency, fadeInDuration));
            isVisible = true;
        }
    }

    [ContextMenu("Hide Light")]
    public void HideLightSphere()
    {
        if (!isInitialized) return;
        if (isVisible)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeLight(lightSphere.CurrentColor.a, 0f, fadeOutDuration));
            isVisible = false;
        }
    }

    private IEnumerator FadeLight(float startTransparency, float endTransparency, float duration)
    {
        if (!isInitialized) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentTransparency = Mathf.Lerp(startTransparency, endTransparency, elapsedTime / duration);
            lightSphere.SetTransparency(currentTransparency);
            yield return null;
        }
        
        lightSphere.SetTransparency(endTransparency);
    }

    // Audio control methods
    public void PlaySound(string emotion)
    {
        
        int index = 0;
        string[] emotionArray = {"happy", "sad", "scared", "surprised", "angry"};

        for (int i = 0; i < emotionArray.Length; i++)
        {
            if (emotionArray[i] == emotion)
            {
                index = i;
            }
        }   

        if (qooboSpeaker != null && beepSounds != null && index < beepSounds.Length && beepSounds[index] != null)
        {
            qooboSpeaker.clip = beepSounds[index];
            qooboSpeaker.Play();
        }
    }

    // Example Specific Audio control methods
    [ContextMenu("Play Happy Sound")]
    public void PlayHappySound()
    {
        {
            PlaySound("happy");
        }
    }


    // Additional utility methods for the light sphere
    public void SetLightIntensity(float intensity)
    {
        if (!isInitialized) return;
        if (lightSphere != null)
            lightSphere.SetIntensity(intensity);
    }

    // Emotion to light color mapping
    public void ShowColouredLight(string type)
    {
        if (!isInitialized) return;
        
        switch (type.ToLower())
        {
            case "happy":
                ShowPinkLight();
                break;
            case "sad":
                ShowBlueLight();
                break;
            case "surprised":
                ShowYellowLight();
                break;
            case "scared":
                ShowGreyLight();
                break;
            case "angry":
                ShowRedLight();
                break;
            default:
                Debug.LogWarning($"Unknown colour type: {type}");
                break;
        }
    }

    public void SetLightTransparency(float transparency)
    {
        if (!isInitialized) return;
        if (lightSphere != null)
            lightSphere.SetTransparency(transparency);
    }

    public void EnableSizeFluctuation(bool enable)
    {
        if (!isInitialized) return;
        if (lightSphere != null)
            lightSphere.SetSizeFluctuation(enable);
    }

    public void SetLightScale(float scale)
    {
        if (!isInitialized) return;
        if (lightSphere != null)
            lightSphere.SetScale(scale);
    }

    // Thought bubble methods
    public void ShowThought(string emotion)
    {
        if (thoughtBubble != null)
        {
            thoughtBubble.ShowThought(emotion);
        }
    }

    public void HideThought()
    {
        if (thoughtBubble != null)
        {
            thoughtBubble.HideThought();
        }
    }

    // Convenience methods
    [ContextMenu("Show Happy Thought")]
    public void ShowHappyThought() => ShowThought("happy");

    [ContextMenu("Show Sad Thought")]
    public void ShowSadThought() => ShowThought("sad");

    [ContextMenu("Show Angry Thought")]
    public void ShowAngryThought() => ShowThought("angry");
}
