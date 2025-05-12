using UnityEngine;
using TMPro;

public class UserFacingTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject qooboMesh;
    [SerializeField] private TextMeshProUGUI angleDebugText;
    [SerializeField] private TextMeshProUGUI lastEventText;
    [SerializeField] private TextMeshProUGUI responseText;
    [SerializeField] private TextMeshProUGUI gaugeValuesText;
    [SerializeField] private Camera arCamera;
    [SerializeField] private EmotionModel emotionModel;
    [SerializeField] private EmotionController emotionController;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugRay = true;
    [SerializeField] private float debugRayLength = 2f;
    [SerializeField] private Color debugRayColor = Color.yellow;

    [Header("Eye Contact Settings")]
    [SerializeField] private float requiredLookDuration = 5f;
    [SerializeField] private float lookAwayDuration = 10f;  // Duration to track looking away
    [SerializeField] private float lookingAtThreshold;  // Angle threshold for looking at robot
    [SerializeField] private float lookingAwayThreshold;  // Angle threshold for looking away from robot

    [SerializeField] private SceneController sceneController;

    private bool hasShown = false;
    private bool hasPlayedLookAway = false;
    private float angleToQoobo;
    private float sustainedLookTimer = 0f;
    private float lookAwayTimer = 0f;
    private bool isLookingAtRobot = false;
    private bool isLookingAway = false;  // Explicitly track looking away state

    private void Start()
    {
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (qooboMesh == null || arCamera == null || angleDebugText == null) return;

        // Get the direction the camera is facing (forward vector)
        Vector3 headDirection = arCamera.transform.forward;

        // Get the direction from the camera to Qoobo
        Vector3 directionToQoobo = (qooboMesh.transform.position - arCamera.transform.position).normalized;

        // Calculate the angle between the two directions
        angleToQoobo = Vector3.Angle(headDirection, directionToQoobo);

        // Check looking states using both thresholds
        bool currentlyLookingAt = angleToQoobo < lookingAtThreshold;
        bool currentlyLookingAway = angleToQoobo > lookingAwayThreshold;

        if (currentlyLookingAt && !isLookingAtRobot)
        {
            // Just started looking at robot
            isLookingAtRobot = true;
            isLookingAway = false;
            sustainedLookTimer = 0f;
            lookAwayTimer = 0f;
            hasPlayedLookAway = false;
        }
        else if (!currentlyLookingAt && isLookingAtRobot)
        {
            // Stopped looking at robot
            isLookingAtRobot = false;
            sustainedLookTimer = 0f;
        }
        else if (currentlyLookingAt && isLookingAtRobot)
        {
            // Continue timing the look duration
            sustainedLookTimer += Time.deltaTime;

            // Check if we've reached the required duration
            if (sustainedLookTimer >= requiredLookDuration && sceneController.wakeUpComplete && !hasShown)
            {
                Debug.Log("UserFacingTracker Gaze on Qoobo Conditions Met");
                var response = emotionModel.CalculateEmotionalResponse("LookingTowards");
                emotionController.TryDisplayEmotion(response.EmotionToDisplay, response.TriggerEvent);
                hasShown = true;
            }
        }

        // Handle looking away state separately
        if (currentlyLookingAway && !isLookingAway)
        {
            // Just started looking away
            isLookingAway = true;
            lookAwayTimer = 0f;
        }
        else if (!currentlyLookingAway && isLookingAway)
        {
            // No longer looking away
            isLookingAway = false;
            lookAwayTimer = 0f;
            hasPlayedLookAway = false;
        }
        else if (currentlyLookingAway && isLookingAway)
        {
            // Continue timing the look away duration
            lookAwayTimer += Time.deltaTime;
            
            // Check if we've been looking away long enough
            if (lookAwayTimer >= lookAwayDuration && sceneController.wakeUpComplete && !hasPlayedLookAway)
            {
                Debug.Log($"User has been looking away (angle: {angleToQoobo:F1}°) for {lookAwayTimer:F1}s");
                var response = emotionModel.CalculateEmotionalResponse("LookingAway");
                emotionController.TryDisplayEmotion(response.EmotionToDisplay, response.TriggerEvent);
                hasPlayedLookAway = true;
            }
        }

        // Update debug texts
        angleDebugText.text = $"Angle: {angleToQoobo:F1}°\nLook Timer: {sustainedLookTimer:F1}s\nAway Timer: {lookAwayTimer:F1}s\nState: {(isLookingAtRobot ? "Looking At" : isLookingAway ? "Looking Away" : "Neutral")}";
        if (lastEventText != null && responseText != null && emotionModel != null)
        {
            lastEventText.text = $"Last Event: {emotionModel.LastTriggeredEvent}";
            responseText.text = $"Response: {emotionModel.LastDisplayString}";
            if (gaugeValuesText != null)
            {
                gaugeValuesText.text = $"T:{emotionModel.TouchGauge:F0} R:{emotionModel.RestGauge:F0} S:{emotionModel.SocialGauge:F0} H:{emotionModel.HungerGauge:F0}";
            }
        }

        // Draw debug ray if enabled
        if (showDebugRay)
        {
            Debug.DrawRay(arCamera.transform.position, headDirection * debugRayLength, debugRayColor);
        }
    }
} 