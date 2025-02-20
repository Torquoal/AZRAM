using UnityEngine;
using TMPro;
using System.Collections;

public class EmotionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SceneController sceneController;
    [SerializeField] private EmotionModel emotionModel;
    [SerializeField] private TextMeshProUGUI emotionText;

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
    private bool isShowingEmotionalDisplay = false;

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

        hasInitialized = true;
        UpdateEmotionDisplay();
    }

    private void Update()
    {
        // Only update passive expression if not showing an emotional display
        if (!isShowingEmotionalDisplay && Time.time - lastPassiveUpdateTime >= passiveUpdateInterval)
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

            case "lethargic":
                sceneController.SetFaceExpression("sad");
                break;

            case "aggressive":
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

    public void DisplayEmotion(string displayString)
    {
        if (!hasInitialized || !sceneController.IsWakeUpComplete())
        {
            Debug.Log("Cannot display emotion - system not initialized or wake-up not complete");
            return;
        }

        // Check cooldown
        if (Time.time - lastDisplayTime < displayCooldown)
        {
            if (showDebugText)
                Debug.Log($"Display in cooldown. Remaining: {displayCooldown - (Time.time - lastDisplayTime):F2}s");
            return;
        }

        if (showDebugText)
            Debug.Log($"Displaying emotion: {displayString}");

        currentDisplayString = displayString;
        isShowingEmotionalDisplay = true;
        
        // Cancel any pending reset
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        // Display the emotion through different modalities based on the display string
        switch (displayString.ToLower())
        {
            case "excited":
                sceneController.ShowColouredLight("happy");
                sceneController.PlaySound("happy");
                sceneController.ShowThought("exclamation");
                sceneController.SetFaceExpression("happy");
                sceneController.TailsEmotion("happy");
                break;

            case "happy":
                sceneController.ShowColouredLight("happy");
                sceneController.PlaySound("happy");
                sceneController.ShowThought("happy");
                sceneController.SetFaceExpression("happy");
                sceneController.TailsEmotion("happy");
                break;

            case "relaxed":
                sceneController.ShowColouredLight("happy");
                sceneController.PlaySound("happy");
                sceneController.ShowThought("happy");
                sceneController.SetFaceExpression("happy");
                sceneController.TailsEmotion("happy");
                break;

            case "shocked":
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

            case "alert":
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
                sceneController.TailsEmotion("neutral");
                break;

            case "sad":
                sceneController.ShowColouredLight("sad");
                sceneController.PlaySound("sad");
                sceneController.ShowThought("sad");
                sceneController.SetFaceExpression("sad");
                sceneController.TailsEmotion("sad");
                break;

            case "crying":
                sceneController.ShowColouredLight("sad");
                sceneController.PlaySound("sad");
                sceneController.ShowThought("sad");
                sceneController.SetFaceExpression("sad");
                sceneController.TailsEmotion("sad");
                break;

            case "tired":
                sceneController.ShowColouredLight("sad");
                sceneController.PlaySound("sad");
                sceneController.ShowThought("tired");
                sceneController.SetFaceExpression("sad");
                sceneController.TailsEmotion("sad");
                break;

            case "sleepy":
                sceneController.ShowColouredLight("sad");
                sceneController.PlaySound("sad");
                sceneController.ShowThought("sleep");
                sceneController.SetFaceExpression("sad");
                sceneController.TailsEmotion("sad");
                break;

            default:
                Debug.LogWarning($"Unknown display string: {displayString}, defaulting to neutral");
                sceneController.HideLightSphere();
                sceneController.HideThought();
                UpdatePassiveExpression(); // Return to mood-based expression
                sceneController.TailsEmotion("neutral");
                break;
        }

        // Update display time and start auto-reset timer
        lastDisplayTime = Time.time;
        UpdateEmotionDisplay();

        if (displayString.ToLower() != "neutral")
        {
            resetCoroutine = StartCoroutine(AutoResetDisplay());
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

        // Reset to neutral state
        isShowingEmotionalDisplay = false;
        DisplayEmotion("neutral");
        Debug.Log("Auto-reset to neutral display state");
    }

    public string GetCurrentDisplayString()
    {
        return currentDisplayString;
    }
} 