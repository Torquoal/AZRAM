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

    private bool isWakingUp = false;
    private bool wasHoveringDuringWakeUp = false;
    private GameObject hoveringObject = null;
    private bool wakeUpComplete = false;

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
                ShowColouredLight("happy"); 
                ShowThought("happy");
                SetFaceExpression("happy");
                break;
            case StrokeDetector.StrokeDirection.BackToFront:
                ShowColouredLight("angry"); 
                ShowThought("angry");
                SetFaceExpression("angry");
                break;
            case StrokeDetector.StrokeDirection.LeftToRight:
                ShowColouredLight("surprised"); 
                ShowThought("surprised");
                SetFaceExpression("surprised");
                break;
            case StrokeDetector.StrokeDirection.RightToLeft:
                ShowColouredLight("sad"); 
                ShowThought("sad");
                SetFaceExpression("sad");
                break;
            default:
                Debug.Log($"Unhandled stroke direction: {direction}");
                break;
        }

        Debug.Log($"Stroke detected: {direction}");
    }

    void Update()
    {
 
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
        wasHoveringDuringWakeUp = false;
        hoveringObject = null;
        Debug.Log("Starting wake-up sequence");

        // Set neutral face at start (invisible)
        if (faceController != null)
        {
            faceController.SetFaceExpression("neutral");
            faceController.SetFaceVisibility(0f); // Ensure face starts invisible
        }

        // Play initial wake up sound
        PlaySound("peep");

        // Start face fade in
        if (faceController != null)
        {
            faceController.StartFadeIn();
        }

        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Play second wake up sound
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

        // Express happiness when hovered
        if (emotionController != null)
        {
            emotionController.SetHappy();
        }
        else
        {
            // Fallback to old behavior if emotionController not set
            ShowColouredLight("happy");
        }
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
