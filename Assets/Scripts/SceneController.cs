using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class SceneController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private LightEmissionSphere lightSphere;
    [SerializeField] private AudioController audioController;
    [SerializeField] private ThoughtBubbleController thoughtBubble;
    [SerializeField] private FaceController faceController;
    [SerializeField] private DistanceTracker distanceTracker;
    [SerializeField] private EmotionController emotionController;
    [SerializeField] private TailAnimations tailAnimations;
    [SerializeField] private StrokeDetector strokeDetector;
    [SerializeField] private EmotionModel emotionModel;

    [Header("Distance Settings")]
    [SerializeField] private float maxDistance = 2.0f; // Maximum distance in meters before showing sadness
    [SerializeField] private float distanceCheckInterval = 0.5f; // How often to check distance
    private float lastDistanceCheckTime = 0f;
    private bool isShowingSadness = false;

    private bool isWakingUp = false;
    public bool wakeUpComplete = false;


    void Start()
    {
        // Subscribe to stroke events
        if (strokeDetector != null)
        {
            strokeDetector.OnStrokeDetected += HandleStrokeDetected;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from stroke events
        if (strokeDetector != null)
        {
            strokeDetector.OnStrokeDetected -= HandleStrokeDetected;
        }
    }

    private void HandleStrokeDetected(StrokeDetector.StrokeDirection direction)
    {
        if (isWakingUp) return;

        // Play happy sound and show happy expression
        PlaySound("happy");
        
        // Show appropriate light color based on stroke direction
        switch (direction)
        {
            case StrokeDetector.StrokeDirection.FrontToBack:
                emotionModel.CalculateEmotionalResponse("StrokeFrontToBack");
                emotionController.SetEmotion("happy");
                emotionController.ExpressEmotion();
                break;
            case StrokeDetector.StrokeDirection.BackToFront:
                emotionController.SetEmotion("angry");
                emotionController.ExpressEmotion();
                break;
            case StrokeDetector.StrokeDirection.HoldLeft:
                ShowColouredLight("surprised");
                break;
            case StrokeDetector.StrokeDirection.HoldRight:
                ShowThought("heart");
                break;
            case StrokeDetector.StrokeDirection.HoldTop:
                emotionController.SetEmotion("happy");  
                emotionController.ExpressEmotion();
                break;
            default:
                Debug.Log($"Unhandled stroke direction: {direction}");
                break;
        }

        Debug.Log($"Stroke detected: {direction}");
    }

    void Update()
    {
        // Only perform checks if wake-up is complete
        if (!wakeUpComplete) return;

        // Check distance at regular intervals
        if (Time.time - lastDistanceCheckTime >= distanceCheckInterval)
        {
            lastDistanceCheckTime = Time.time;
            float distance = GetDistanceToPlayer();
            
            // Show sadness if too far away
            if (distance > maxDistance && !isShowingSadness && !isWakingUp)
            {
                emotionController.SetEmotion("sad");
                emotionController.ExpressEmotion();
                isShowingSadness = true;
            }
            // Reset when back in range
            else if (distance <= maxDistance && isShowingSadness)
            {
                emotionController.SetEmotion("neutral");
                emotionController.ExpressEmotion();
                isShowingSadness = false;
            }
        }

    }


    // Audio control methods
    public void PlaySound(string emotion)
    {
        if (isWakingUp && emotion != "peep")
        {
            Debug.Log("Cannot play sound during wake-up sequence");
            return;
        }
        
        if (audioController != null)
        {
            audioController.PlaySound(emotion);
        }
    }

    // Emotion to light color mapping
    public void ShowColouredLight(string emotion)
    {
        if (isWakingUp)
        {
            Debug.Log("Cannot show light during wake-up sequence");
            return;
        }

        if (lightSphere == null) return;
        
        lightSphere.SetEmotionColor(emotion);
        lightSphere.Show();
    }

    public void HideLightSphere()
    {
        if (lightSphere != null)
            lightSphere.Hide();
    }

    public void SetLightTransparency(float transparency)
    {
        if (lightSphere != null)
            lightSphere.SetTransparency(transparency);
    }

    public void EnableSizeFluctuation(bool enable)
    {
        if (lightSphere != null)
            lightSphere.SetSizeFluctuation(enable);
    }

    public void SetLightScale(float scale)
    {
        if (lightSphere != null)
            lightSphere.SetScale(scale);
    }

    public void TailsEmotion(string emotion)
    {
        if (tailAnimations != null)
        {
            tailAnimations.PlayTailAnimation(emotion);
        }
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
        Debug.Log("Starting wake-up sequence");

        // Show sleep thought bubble
        if (thoughtBubble != null)
        {
            thoughtBubble.ShowThought("sleep");
            yield return new WaitForSeconds(4f);  // Show sleep thought for a moment
        }

        // Set neutral face at start (invisible)
        if (faceController != null)
        {
            faceController.SetFaceExpression("neutral");
            faceController.SetFaceVisibility(0f); // Ensure face starts invisible
        }

        // Play initial wake up sound
        PlaySound("peep");

        // Hide thought bubble as face starts to appear
        if (thoughtBubble != null)
        {
            thoughtBubble.HideThought();
        }

        // Start face fade in
        if (faceController != null)
        {
            faceController.StartFadeIn();
        }

        yield return new WaitForSeconds(2f);

        // Play second wake up sound
        PlaySound("peep");

        isWakingUp = false;
        wakeUpComplete = true;
        Debug.Log("Wake-up sequence complete");

      
    }

   

    public bool IsWakeUpComplete()
    {
        return wakeUpComplete;
    }

    // Face control methods
    public void SetFaceExpression(string expression)
    {
        if (isWakingUp)
        {
            Debug.Log("Cannot change face expression during wake-up sequence");
            return;
        }

        if (faceController != null)
        {
            faceController.SetFaceExpression(expression);
        }
    }

    // Distance tracking methods
    public float GetDistanceToPlayer()
    {
        if (distanceTracker != null)
        {
            return distanceTracker.GetCurrentDistance();
        }
        return -1f; // Invalid distance
    }

    public void SetDistanceDebugVisible(bool visible)
    {
        if (distanceTracker != null)
        {
            distanceTracker.SetDebugTextVisibility(visible);
        }
    }

    public void SetEmotionDebugVisible(bool visible)
    {
        if (emotionController != null)
        {
            emotionController.SetDebugTextVisibility(visible);
        }
    }

    // Set all debug displays visible/invisible
    public void SetAllDebugVisible(bool visible)
    {
        SetDistanceDebugVisible(visible);
        SetEmotionDebugVisible(visible);
    }
}
