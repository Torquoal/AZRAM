using UnityEngine;
using TMPro;
using System.Collections;

public class EmotionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SceneController sceneController;
    [SerializeField] private EmotionModel emotionModel;
    [SerializeField] private TextMeshProUGUI emotionText;
    [SerializeField] private VoiceDetector voiceDetector;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugText = true;
    [SerializeField] private string emotionFormat = "Emotion: {0}";

    [Header("Display Settings")]
    [SerializeField] private float displayDuration = 10f;  // Duration before returning to neutral
    [SerializeField] private float displayCooldown = 5f;   // Cooldown period before another display can be shown
    [SerializeField] private float passiveUpdateInterval = 5f; // How often to update passive expression

    private bool hasInitialized = false;
    private float lastDisplayTime = -999f;
    private float lastPassiveUpdateTime = 0f;
    private Coroutine resetCoroutine;
    private string currentDisplayString = "Neutral";
    private string currentTriggerEvent = "";
    private bool isShowingEmotionalDisplay = false;

    // Helper property to calculate time since last display
    private float TimeSinceLastDisplay => Time.time - lastDisplayTime;

    private void Start()
    {
        if (sceneController == null)
        {
            Debug.LogError("SceneController reference not set in EmotionController!");
            enabled = false;
            return;
        }

        if (emotionModel == null)
        {
            Debug.LogError("EmotionModel reference not set in EmotionController!");
            enabled = false;
            return;
        }

        // Subscribe to loud sound events
        if (voiceDetector != null)
        {
            voiceDetector.OnLoudSoundDetected += HandleLoudSound;
        }
        else
        {
            Debug.LogWarning("VoiceDetector reference not set - loud sound detection disabled");
        }

        hasInitialized = true;
        UpdateEmotionDisplay();
    }

    private void OnDestroy()
    {
        if (voiceDetector != null)
        {
            voiceDetector.OnLoudSoundDetected -= HandleLoudSound;
        }
    }

    private void HandleLoudSound()
    {
        // Get emotional response from model
        EmotionModel.EmotionalResponseResult response = emotionModel.CalculateEmotionalResponse("loudnoise");

        // Wake up if asleep
        if (emotionModel.IsAsleep)
        {
            emotionModel.WakeUp();
        }

        // Display the emotion with the original trigger event
        DisplayEmotionInternal(response.EmotionToDisplay, "loudnoise");
    }

    private void Update()
    {
        // Only update passive expression if not showing an emotional display and not asleep
        if (!isShowingEmotionalDisplay && !emotionModel.IsAsleep && Time.time - lastPassiveUpdateTime >= passiveUpdateInterval)
        {
            UpdatePassiveExpression();
            lastPassiveUpdateTime = Time.time;
        }
    }

    private void UpdatePassiveExpression()
    {
        if (!hasInitialized || !sceneController.IsWakeUpComplete()) return;

        string currentMood = emotionModel.GetCurrentMood();
        
        if (showDebugText)
            Debug.Log($"Updating passive expression based on mood: {currentMood}");

        // Set passive expression based on mood
        switch (currentMood.ToLower())
        {
            case "excited":
                sceneController.SetFaceExpression("happy");
                break;

            case "happy":
                sceneController.SetFaceExpression("happy");
                break;

            case "relaxed":
                sceneController.SetFaceExpression("happy");
                break;

            case "energetic":
                sceneController.SetFaceExpression("surprised");
                break;

            case "neutral":
                sceneController.SetFaceExpression("neutral");
                break;

            case "tired":
                sceneController.SetFaceExpression("sad");
                break;

            case "annoyed":
                sceneController.SetFaceExpression("angry");
                break;

            case "sad":
                sceneController.SetFaceExpression("sad");
                break;

            case "gloomy":
                sceneController.SetFaceExpression("sad");
                break;

            

            default:
                sceneController.SetFaceExpression("neutral");
                break;
        }
    }

    private void DisplaySleepState()
    {
        if (showDebugText)
            Debug.Log("Displaying sleep state");

        // Stop any existing auto-reset coroutine
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        currentDisplayString = "sleep";
        sceneController.SetFaceExpression("sleepy");
        sceneController.ShowThought("sleep");
        sceneController.HideLightSphere();
    }

    private void ApplySpecialDisplayOverrides(string triggerEvent)
    {
        if (showDebugText)
            Debug.Log($"Checking special display overrides for: {triggerEvent}");

        // Stop any existing thought bubble reset coroutine
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        // Handle special display overrides
        switch (triggerEvent.ToLower())
        {
            case "hungerneeded":
                sceneController.ShowThought("hungry");
                break;
            case "socialneeded":
                sceneController.ShowThought("social");
                sceneController.ShowColouredLight("sad");
                break;
            case "touchneeded":
                sceneController.ShowThought("hand");
                break;
            case "restneeded":
                sceneController.ShowThought("sleep");
                break;
            case "hungerfulfilled":
                sceneController.ShowThought("heart");
                break;
            case "socialfulfilled":
                sceneController.ShowThought("heart");
                break;
            case "touchfulfilled":
                sceneController.ShowThought("hand");
                break;
            case "restfulfilled":
                sceneController.ShowThought("sun");
                break;
            case "hungerunfulfilled":
                sceneController.ShowThought("hungry");
                break;
            case "socialunfulfilled":
                sceneController.ShowThought("sad");
                break;
            case "touchunfulfilled":
                sceneController.ShowThought("hand");
                break;
            case "foodheard":
                sceneController.ShowThought("hungry");
                break;
            case "greetingheard":
                sceneController.ShowThought("heart");
                break;
            case "nameheard":
                sceneController.ShowThought("exclamation");
                break;
            case "loudnoise":
                sceneController.PlaySound("surprised");
                sceneController.ShowThought("exclamation");
                sceneController.SetFaceExpression("shocked");
                sceneController.TailsEmotion("surprised");
                break;
            case "happyheard":
                sceneController.ShowThought("happy");
                break;
            case "sadheard":
                sceneController.ShowThought("sad");
                break;
            case "angryheard":
                sceneController.ShowThought("angry");
                break;
            case "farewellheard":
                sceneController.ShowThought("sad");
                break;
            case "praiseheard":
                sceneController.ShowThought("heart");
                break;
            case "touchheard":
                sceneController.ShowThought("hand");
                break;
            case "lookingaway":
                sceneController.ShowThought("eyes");
                break;
            case "lookingtowards":
                sceneController.ShowThought("eyes");
                break;
        }

        // Only start auto-reset if not asleep
        if (!emotionModel.IsAsleep)
        {
            resetCoroutine = StartCoroutine(AutoResetThoughtBubble());
        }
    }

    private IEnumerator AutoResetThoughtBubble()
    {
        yield return new WaitForSeconds(10f); // Wait for 10 seconds
        sceneController.HideThought();
        resetCoroutine = null;
    }

    public void DisplayEmotion(string displayString, string triggerEvent)
    {
        if (!hasInitialized || !sceneController.IsWakeUpComplete())
        {
            Debug.Log("Cannot display emotion - system not initialized or wake-up not complete");
            return;
        }

        if (showDebugText)
            Debug.Log($"Emotion Controller: Displaying emotion: {displayString} from trigger: {triggerEvent}");

        currentDisplayString = displayString;
        currentTriggerEvent = triggerEvent;
        
        // Cancel any pending reset if we're going to sleep
        if (displayString.ToLower() == "sleep" || emotionModel.IsAsleep)
        {
            if (resetCoroutine != null)
            {
                StopCoroutine(resetCoroutine);
                resetCoroutine = null;
            }
        }

        // Handle the emotion display
        switch (displayString.ToLower())
        {
            case "sleep":
                //sceneController.SetFaceExpression("sleepy");
                sceneController.ShowThought("sleep");
                sceneController.HideLightSphere();
                break;
            
            // Regular emotional states
            case "excited":
                sceneController.ShowColouredLight("happy");
                sceneController.PlaySound("surprised");
                sceneController.ShowThought("exclamation");
                sceneController.SetFaceExpression("happy");
                sceneController.TailsEmotion("happy");
                break;

            case "happy":
                //sceneController.ShowColouredLight("happy");
                sceneController.PlaySound("happy");
                sceneController.ShowThought("happy");
                sceneController.SetFaceExpression("happy");
                sceneController.TailsEmotion("happy");
                break;

            case "relaxed":
                sceneController.ShowThought("sleep");
                sceneController.SetFaceExpression("happy");
                sceneController.TailsEmotion("happy");
                sceneController.PlaySound("happy");
                break;

            case "surprised":
                sceneController.ShowColouredLight("surprised");
                sceneController.PlaySound("surprised");
                sceneController.ShowThought("surprised");
                sceneController.SetFaceExpression("surprised");
                sceneController.TailsEmotion("surprised");
                break;

            case "energetic":
                sceneController.ShowColouredLight("surprised");
                sceneController.PlaySound("surprised");
                sceneController.ShowThought("surprised");
                sceneController.SetFaceExpression("surprised");
                sceneController.TailsEmotion("surprised");
                break;

            case "annoyed":
                sceneController.ShowColouredLight("angry");
                sceneController.PlaySound("angry");
                sceneController.ShowThought("angry");
                sceneController.SetFaceExpression("angry");
                sceneController.TailsEmotion("angry");
                break;

            case "angry":
                sceneController.ShowColouredLight("angry");
                sceneController.PlaySound("angry");
                sceneController.ShowThought("angry");
                sceneController.SetFaceExpression("angry");
                sceneController.TailsEmotion("angry");
                break;

            case "tense":
                sceneController.ShowColouredLight("surprised");
                sceneController.PlaySound("surprised");
                sceneController.ShowThought("surprised");
                sceneController.SetFaceExpression("surprised");
                sceneController.TailsEmotion("surprised");
                break;

            case "neutral":
                sceneController.HideLightSphere();
                sceneController.HideThought();
                UpdatePassiveExpression(); // Return to mood-based expression
                sceneController.TailsEmotion("happy");
                break;

            case "sad":
                sceneController.PlaySound("sad");
                sceneController.ShowThought("sad");
                sceneController.SetFaceExpression("sad");
                sceneController.TailsEmotion("sad");
                break;

            case "miserable":
                sceneController.ShowColouredLight("sad");
                sceneController.PlaySound("sad");
                sceneController.ShowThought("sad");
                sceneController.SetFaceExpression("sad");
                sceneController.TailsEmotion("sad");
                break;

            case "tired":
                sceneController.ShowThought("tired");
                sceneController.SetFaceExpression("neutral");
                sceneController.TailsEmotion("sad");
                sceneController.PlaySound("sad");
                break;

            case "gloomy":
                sceneController.ShowThought("sleep");
                sceneController.SetFaceExpression("neutral");
                sceneController.TailsEmotion("sad");
                sceneController.PlaySound("sad");
                break;


            default:
                Debug.LogWarning($"Unknown display string: {displayString}, defaulting to neutral");
                sceneController.HideLightSphere();
                sceneController.HideThought();
                UpdatePassiveExpression(); // Return to mood-based expression
                sceneController.TailsEmotion("happy");
                break;
        }

        // Then apply any special overrides based on the trigger event
        if (!string.IsNullOrEmpty(triggerEvent))
        {
            ApplySpecialDisplayOverrides(triggerEvent);
        }

        // Update display time
        UpdateEmotionDisplay();

        // Only start the auto-reset for non-sleep emotions and when not asleep
        if (displayString.ToLower() != "neutral" && displayString.ToLower() != "sleep" && !emotionModel.IsAsleep)
        {
            resetCoroutine = StartCoroutine(AutoResetDisplay());
        }
        else if (displayString.ToLower() == "neutral")
        {
            // For neutral, we clear the emotional display state immediately
            isShowingEmotionalDisplay = false;
        }
    }

    private void UpdateEmotionDisplay()
    {
        if (emotionText != null)
        {
            emotionText.text = string.Format(emotionFormat, currentDisplayString);
        }
    }

    public void SetDebugTextVisibility(bool visible)
    {
        showDebugText = visible;
        if (emotionText != null)
        {
            emotionText.enabled = visible;
        }
    }

    private IEnumerator AutoResetDisplay()
    {
        yield return new WaitForSeconds(displayDuration);

        // Reset display state and transition to neutral
        isShowingEmotionalDisplay = false;
        lastDisplayTime = Time.time;
        
        // Hide light sphere when resetting
        sceneController.HideLightSphere();
        
        // Reset to neutral state through TryDisplayEmotion to respect cooldown
        TryDisplayEmotion("neutral", "", true);  // Use bypassCooldown=true for neutral
        Debug.Log("Auto-reset to neutral display state");
    }

    public string GetCurrentDisplayString()
    {
        return currentDisplayString;
    }

    public bool TryDisplayEmotion(string displayString, string triggerEvent, bool bypassCooldown = false)
    {
        if (!hasInitialized || !sceneController.IsWakeUpComplete())
        {
            Debug.Log("Cannot display emotion - system not initialized or wake-up not complete");
            return false;
        }

        // If asleep, only allow special wake-up events
        if (emotionModel.IsAsleep && displayString != "wake")
        {
            if (showDebugText)
                Debug.Log("Cannot display emotion - robot is asleep");
            return false;
        }

        // Always allow neutral emotions with bypass, and check cooldown for others
        if (!bypassCooldown && displayString.ToLower() != "neutral" && !CanDisplayEmotion())
        {
            float remainingCooldown = displayCooldown - (Time.time - lastDisplayTime);
            if (showDebugText && remainingCooldown > 0)
                Debug.Log($"Cannot display emotion - in cooldown. Cooldown remaining: {Mathf.Max(0, remainingCooldown):F2}s");
            else if (showDebugText)
                Debug.Log("Cannot display emotion - already showing an emotion");
            return false;
        }

        DisplayEmotionInternal(displayString, triggerEvent);
        return true;
    }

    private bool CanDisplayEmotion()
    {
        return !isShowingEmotionalDisplay && Time.time - lastDisplayTime >= displayCooldown;
    }

    public void DisplayEmotionInternal(string displayString, string triggerEvent = "")
    {
        if (showDebugText)
            Debug.Log($"DisplayEmotionInternal called with emotion: {displayString}, trigger: {triggerEvent}");
            
        if (displayString.ToLower() != "neutral")
        {
            isShowingEmotionalDisplay = true;
            lastDisplayTime = Time.time;
        }
        DisplayEmotion(displayString, triggerEvent);
    }
} 