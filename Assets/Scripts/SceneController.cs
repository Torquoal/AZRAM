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


    // *** USER INPUT INTERACTIONS ***
    /*
    ** TOUCH CONTROL
    */

    [ContextMenu("Test Touch Control")]
    private void TestTouchControl()
    {
        HandleStrokeDetected(StrokeDetector.StrokeDirection.FrontToBack);
    }

    void Start()
    {
        // Subscribe to stroke events
        if (strokeDetector != null)
        {
            strokeDetector.OnStrokeDetected += HandleStrokeDetected;
            strokeDetector.OnHoldDetected += HandleHoldDetected;  // Subscribe to hold events
        }
    }
    void OnDestroy()
    {
        // Unsubscribe from stroke events
        if (strokeDetector != null)
        {
            strokeDetector.OnStrokeDetected -= HandleStrokeDetected;
            strokeDetector.OnHoldDetected -= HandleHoldDetected;  // Unsubscribe from hold events
        }
    }
    private void HandleStrokeDetected(StrokeDetector.StrokeDirection direction)
    {
        if (isWakingUp) return;
        
        // If asleep, don't respond to normal strokes
        if (emotionModel.IsAsleep)
        {
            Debug.Log("Stroke detected but robot is asleep");
            return;
        }

        switch (direction)
        {
            case StrokeDetector.StrokeDirection.FrontToBack:
                Debug.Log("Stroke detected: FrontToBack");
                var response = emotionModel.CalculateEmotionalResponse("StrokeFrontToBack");
                emotionController.TryDisplayEmotion(response.EmotionToDisplay, response.TriggerEvent);
                break;
            case StrokeDetector.StrokeDirection.BackToFront:
                Debug.Log("Stroke detected: BackToFront");
                response = emotionModel.CalculateEmotionalResponse("StrokeBackToFront");
                emotionController.TryDisplayEmotion(response.EmotionToDisplay, response.TriggerEvent);
                break;
            //case StrokeDetector.StrokeDirection.HoldLeft:
            //    Debug.Log("Stroke detected: HoldLeft");
            //    break;
            //case StrokeDetector.StrokeDirection.HoldRight:
            //    Debug.Log("Stroke detected: HoldRight");
            //    break;
            //case StrokeDetector.StrokeDirection.HoldTop:
            //    Debug.Log("Stroke detected: HoldTop");  
            //    break;
            default:
                Debug.Log($"Unhandled stroke direction: {direction}");
                break;
        }
    }

    private void HandleHoldDetected()
    {
        if (isWakingUp) return;
        
        // If asleep, don't respond to holds
        if (emotionModel.IsAsleep)
        {
            Debug.Log("Hold detected but robot is asleep");
            return;
        }

        Debug.Log("Hold detected - calculating emotional response");
        var response = emotionModel.CalculateEmotionalResponse("BeingHeld");
        emotionController.TryDisplayEmotion(response.EmotionToDisplay, response.TriggerEvent);
    }
    /*
    ** DISTANCE CONTROL
    */
    void Update()
    {
        // Only perform checks if wake-up is complete
        if (!wakeUpComplete) return;

        // If asleep, skip distance checking
        if (emotionModel.IsAsleep) return;

        // Check distance at regular intervals
        if (Time.time - lastDistanceCheckTime >= distanceCheckInterval)
        {
            lastDistanceCheckTime = Time.time;
            float distance = GetDistanceToPlayer();
            
            // Show sadness if too far away
            if (distance > maxDistance && !isShowingSadness && !isWakingUp)
            {
                var response = emotionModel.CalculateEmotionalResponse("TooFarAway");
                emotionController.TryDisplayEmotion(response.EmotionToDisplay, response.TriggerEvent);
                isShowingSadness = true;
            }
            // Triggers when user is back in range
            else if (distance <= maxDistance && isShowingSadness)
            {
                isShowingSadness = false;
            }
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

    /*
    ** VOICE CONTROL - See VoiceDetector.cs
    */

    /*
    ** GAZE CONTROL - See UserFacingTracker.cs
    */



    // *** ROBOT DISPLAY MODALITIES ***
    /*
    ** SOUND CONTROL
    */

    public void PlaySound(string emotion)
    {
        if (isWakingUp && emotion != "peep")
        {
            Debug.Log("Cannot play sound during wake-up sequence");
            return;
        }

        // If asleep, only allow sleep-related sounds
        if (emotionModel.IsAsleep && emotion != "sleep")
        {
            return;
        }
        
        if (audioController != null)
        {
            audioController.PlaySound(emotion);
        }
    }  
    /*
    ** LIGHT CONTROL
    */ 
    public void ShowColouredLight(string emotion)
    {
        if (isWakingUp || emotionModel.IsAsleep)
        {
            Debug.Log("Cannot show light during wake-up sequence or while asleep");
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
    /*
    ** TAIL CONTROL
    */
    public void TailsEmotion(string emotion)
    {
        if (tailAnimations != null)
        {
            tailAnimations.PlayTailAnimation(emotion);
        }
    }
    /*
    ** THOUGHT BUBBLE CONTROL
    */
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
    /*
    ** WAKE UP SEQUENCE CONTROL
    */
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
            thoughtBubble.ShowThought("sun");
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
    /*
    ** FACE CONTROL
    */
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


    // *** UTILITY METHODS ***
    /*
    ** CAMERA CONTROL
    */

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
    /*
    ** DEBUG CONTROL
    */  

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
