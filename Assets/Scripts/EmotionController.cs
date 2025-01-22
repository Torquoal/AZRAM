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

    public enum Emotion
    {
        Happy,
        Sad,
        Angry,
        Scared,
        Surprised,
        Neutral
    }

    private Emotion currentEmotion = Emotion.Neutral;
    private bool hasInitialized = false;

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

        string emotionName = currentEmotion.ToString().ToLower();
        
        // Trigger all expression methods
        sceneController.ShowColouredLight(emotionName);
        sceneController.PlaySound(emotionName);
        sceneController.ShowThought(emotionName);
        sceneController.SetFaceExpression(emotionName);
        sceneController.TailsEmotion(emotionName);

        Debug.Log($"Expressing emotion: {currentEmotion}");
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
} 