using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

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
    [SerializeField] private FaceController faceController;

    private bool isVisible = false;
    private Coroutine fadeCoroutine;
    private bool isInitialized = false;
    private bool isWakingUp = false;
    private bool wasHoveringDuringWakeUp = false;
    private GameObject hoveringObject = null;
    private bool wakeUpComplete = false;

    void Start()
    {
        // Wait a frame to ensure LightEmissionSphere is fully initialized
        StartCoroutine(InitializeLightSphere());
    }

    void Update()
    {
        // Check for R key using new Input System
        //if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        //{
        //    ResetCamera();
        //}
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
        if (isWakingUp && emotion != "peep")
        {
            Debug.Log("Cannot play sound during wake-up sequence");
            return;
        }
        
        int index = 0;
        string[] emotionArray = {"happy", "sad", "scared", "surprised", "angry", "peep"};

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
        if (isWakingUp)
        {
            Debug.Log("Cannot show light during wake-up sequence");
            return;
        }

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
        if (isWakingUp)
        {
            Debug.Log("Cannot show thought during wake-up sequence");
            return;
        }

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

    public void ResetCamera()
    {
        // Find the XR Origin or Camera Rig
        var xrOrigin = GameObject.Find("XR Origin");
        if (xrOrigin == null)
        {
            xrOrigin = GameObject.Find("Camera Rig"); // Fallback name
        }

        if (xrOrigin != null)
        {
            // Get the camera's forward direction but zero out the Y component
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector3 cameraForward = mainCamera.transform.forward;
                cameraForward.y = 0;
                cameraForward.Normalize();

                // Reset the XR Origin position
                Vector3 currentPos = xrOrigin.transform.position;
                xrOrigin.transform.position = new Vector3(0, currentPos.y, 0); // Keep current height

                // Reset the XR Origin rotation to face forward
                if (cameraForward != Vector3.zero)
                {
                    xrOrigin.transform.rotation = Quaternion.LookRotation(cameraForward);
                }
                else
                {
                    xrOrigin.transform.rotation = Quaternion.identity;
                }

                Debug.Log("Camera view reset");
            }
        }
        else
        {
            Debug.LogWarning("Could not find XR Origin or Camera Rig");
        }
    }

    // Wake up sequence methods
    public void StartWakeUpSequence()
    {
        if (isWakingUp) return; // Prevent multiple wake-up sequences
        StartCoroutine(WakeUpSequence());
    }

    private IEnumerator WakeUpSequence()
    {
        isWakingUp = true;
        wakeUpComplete = false;
        wasHoveringDuringWakeUp = false;
        hoveringObject = null;
        Debug.Log("Starting wake-up sequence");

        // Set neutral face at start
        if (faceController != null) faceController.SetFaceExpression("neutral");

        // Play initial wake up sound
        PlaySound("peep");

        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Play initial wake up sound
        PlaySound("peep");

        isWakingUp = false;
        wakeUpComplete = true;
        Debug.Log("Wake-up sequence complete");

        // Check if something was hovering during wake-up
        if (wasHoveringDuringWakeUp && hoveringObject != null)
        {
            // Trigger hover effect that was waiting
            ShowColouredLight("happy");
        }
    }

    // Call this from your hover detection script
    public void OnObjectHover(GameObject hoveredObject)
    {
        if (isWakingUp)
        {
            wasHoveringDuringWakeUp = true;
            hoveringObject = hoveredObject;
            return;
        }

        // Normal hover behavior
        ShowColouredLight("happy");
    }

    public bool IsWakeUpComplete()
    {
        return wakeUpComplete;
    }
}
