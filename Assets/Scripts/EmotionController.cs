using UnityEngine;
using TMPro;
using System.Collections;

public class EmotionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SceneController sceneController;
    [SerializeField] private TextMeshProUGUI emotionText;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugText = true;
    [SerializeField] private string emotionFormat = "Emotion: {0}";

    [Header("Emotion Control")]
    [SerializeField] private Emotion manualEmotion = Emotion.Neutral;
    [SerializeField] private float initialEmotionDelay = 1f;
    [SerializeField] private float emotionCooldown = 2f;  // Cooldown period before another emotion can be expressed
    [SerializeField] private float emotionDuration = 10f;  // Duration before returning to neutral

    private Emotion currentEmotion = Emotion.Neutral;
    private bool hasInitialized = false;
    private float lastEmotionTime = -999f;  // Track when the last emotion was expressed
    private Coroutine resetCoroutine;  // Track the auto-reset coroutine

    public enum Emotion
    {
        Happy,
        Sad,
        Angry,
        Scared,
        Surprised,
        Neutral
    }

    private void Start()
    {
        if (sceneController == null)
        {
            Debug.LogError("SceneController reference not set in EmotionController!");
            enabled = false;
            return;
        }

        // Set the current emotion without expressing it
        currentEmotion = manualEmotion;
        UpdateEmotionDisplay();

        // Wait before expressing the initial emotion
        StartCoroutine(InitialEmotionDelay());
    }

    private IEnumerator InitialEmotionDelay()
    {
        yield return new WaitForSeconds(initialEmotionDelay);
        hasInitialized = true;
        
        // Only express emotion if wake-up is complete
        if (sceneController.IsWakeUpComplete())
        {
            ExpressEmotion();
        }
    }

    private void OnValidate()
    {
        // Only update during play mode and after initialization
        if (Application.isPlaying && hasInitialized && manualEmotion != currentEmotion)
        {
            SetEmotion(manualEmotion);
        }
    }

    public void SetEmotion(Emotion emotion)
    {
        // Check if we're in cooldown
        if (Time.time - lastEmotionTime < emotionCooldown)
        {
            Debug.Log($"Emotion in cooldown. Remaining: {emotionCooldown - (Time.time - lastEmotionTime):F2}s");
            return;
        }

        currentEmotion = emotion;
        manualEmotion = emotion; // Keep inspector value in sync
        
        // Only express emotion if initialized and wake-up is complete
        if (hasInitialized && sceneController.IsWakeUpComplete())
        {
            ExpressEmotion();
        }
        UpdateEmotionDisplay();
    }

    public void SetEmotion(string emotionName)
    {
        if (System.Enum.TryParse(emotionName, true, out Emotion emotion))
        {
            SetEmotion(emotion);
        }
        else
        {
            Debug.LogWarning($"Invalid emotion name: {emotionName}");
        }
    }

    private void UpdateEmotionDisplay()
    {
        if (emotionText != null && showDebugText)
        {
            emotionText.text = string.Format(emotionFormat, currentEmotion.ToString());
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

    [ContextMenu("Express Emotion")]
    public void ExpressEmotion()
    {
        if (sceneController == null) return;

        // Cancel any pending reset
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        string emotionName = currentEmotion.ToString().ToLower();
        
        // Trigger all expression methods
        sceneController.ShowColouredLight(emotionName);
        sceneController.PlaySound(emotionName);
        sceneController.ShowThought(emotionName);
        sceneController.SetFaceExpression(emotionName);
        sceneController.TailsEmotion(emotionName);

        // Update last emotion time
        lastEmotionTime = Time.time;

        // Start auto-reset timer if not neutral
        if (currentEmotion != Emotion.Neutral)
        {
            resetCoroutine = StartCoroutine(AutoResetEmotion());
        }

        Debug.Log($"Expressing emotion: {currentEmotion}");
    }

    private IEnumerator AutoResetEmotion()
    {
        yield return new WaitForSeconds(emotionDuration);

        // Reset to neutral state
        sceneController.HideLightSphere();
        sceneController.HideThought();
        sceneController.SetFaceExpression("neutral");
        sceneController.TailsEmotion("neutral");

        // Update internal state
        currentEmotion = Emotion.Neutral;
        manualEmotion = Emotion.Neutral;
        UpdateEmotionDisplay();

        Debug.Log("Auto-reset to neutral state");
    }

    public Emotion GetCurrentEmotion()
    {
        return currentEmotion;
    }

    // Convenience methods for setting emotions

    [ContextMenu("Set Happy")]
    public void SetHappy() => SetEmotion(Emotion.Happy);

    [ContextMenu("Set Sad")]
    public void SetSad() => SetEmotion(Emotion.Sad);

    [ContextMenu("Set Angry")]
    public void SetAngry() => SetEmotion(Emotion.Angry);

    [ContextMenu("Set Scared")]
    public void SetScared() => SetEmotion(Emotion.Scared);

    [ContextMenu("Set Surprised")]
    public void SetSurprised() => SetEmotion(Emotion.Surprised);

    [ContextMenu("Set Neutral")]
    public void SetNeutral() => SetEmotion(Emotion.Neutral);

    // Context menu items for testing in Unity Editor
    [ContextMenu("Express Happy")]
    private void ExpressHappy() => SetHappy();

    [ContextMenu("Express Sad")]
    private void ExpressSad() => SetSad();

    [ContextMenu("Express Angry")]
    private void ExpressAngry() => SetAngry();

    [ContextMenu("Express Scared")]
    private void ExpressScared() => SetScared();

    [ContextMenu("Express Surprised")]
    private void ExpressSurprised() => SetSurprised();

    [ContextMenu("Express Neutral")]
    private void ExpressNeutral() => SetNeutral();

    // Add this method to handle voice-detected emotions
    public void HandleVoiceEmotion(string emotion)
    {
        // Convert the emotion string to our enum
        if (System.Enum.TryParse<Emotion>(emotion, true, out Emotion detectedEmotion))
        {
            SetEmotion(detectedEmotion);
            ExpressEmotion();
        }
        else
        {
            Debug.LogWarning($"EmotionController: Couldn't parse emotion string: {emotion}");
        }
    }
} 