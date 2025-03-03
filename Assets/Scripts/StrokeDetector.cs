using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StrokeDetector : MonoBehaviour
{
    [Header("Hand References")]
    [SerializeField] private string leftHandPath = "Camera Rig/[BuildingBlock] Interaction/[BuildingBlock] Hand Interactions/LeftHand";
    [SerializeField] private string rightHandPath = "Camera Rig/[BuildingBlock] Interaction/[BuildingBlock] Hand Interactions/RightHand";

    [Header("Stroke Detection Settings")]
    [SerializeField] private float maxStrokeTime = 2.0f;
    [SerializeField] private float triggerResetTime = 1.0f;
    [SerializeField] private float strokeCooldownTime = 3.0f; // Time before another stroke can be triggered

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    [Header("References")]
    [SerializeField] private SceneController sceneController;

    [Header("Hold Detection Settings")]
    [SerializeField] private float holdDuration = 2.0f; // Duration in seconds to detect a hold

    // These will be set by the StrokeTriggerDetector components
    private Collider frontCollider;
    private Collider backCollider;
    private Collider leftCollider;
    private Collider rightCollider;
    private Collider topCollider;  // New top collider

    public enum StrokeDirection
    {
        None,
        FrontToBack,
        BackToFront,
        HoldLeft,
        HoldRight,
        HoldTop     // New top hold direction
    }

    private enum StrokeType
    {
        None,
        FrontBack
    }

    private class TriggerEvent
    {
        public Collider trigger;
        public float timestamp;

        public TriggerEvent(Collider t, float time)
        {
            trigger = t;
            timestamp = time;
        }
    }

    private List<TriggerEvent> triggerSequence = new List<TriggerEvent>();
    private float lastTriggerTime;
    private bool isProcessingStroke = false;
    private StrokeType currentStrokeType = StrokeType.None;
    private HashSet<Collider> activeColliders = new HashSet<Collider>(); // Track currently active collisions

    private float lastSuccessfulStrokeTime = -999f; // Initialize to allow first stroke immediately
    private Collider expectedNextTrigger = null; // The only valid next trigger in sequence

    private float leftHoldStartTime = -1f;
    private float rightHoldStartTime = -1f;
    private float topHoldStartTime = -1f;  // New top hold timer
    private bool leftHoldTriggered = false;
    private bool rightHoldTriggered = false;
    private bool topHoldTriggered = false;  // New top hold flag

    public delegate void StrokeDetectedHandler(StrokeDirection direction);
    public event StrokeDetectedHandler OnStrokeDetected;

    private void Start()
    {
        if (sceneController == null)
        {
            sceneController = FindObjectOfType<SceneController>();
            if (sceneController == null)
                Debug.LogWarning("No SceneController found for sound feedback");
        }
    }

    public void SetFrontTrigger(Collider col)
    {
        frontCollider = col;
        if (showDebugLogs)
            Debug.Log($"Front trigger set to: {col.name}");
    }

    public void SetBackTrigger(Collider col)
    {
        backCollider = col;
        if (showDebugLogs)
            Debug.Log($"Back trigger set to: {col.name}");
    }

    public void SetLeftTrigger(Collider col)
    {
        leftCollider = col;
        if (showDebugLogs)
            Debug.Log($"Left trigger set to: {col.name}");
    }

    public void SetRightTrigger(Collider col)
    {
        rightCollider = col;
        if (showDebugLogs)
            Debug.Log($"Right trigger set to: {col.name}");
    }

    public void SetTopTrigger(Collider col)
    {
        topCollider = col;
        if (showDebugLogs)
            Debug.Log($"Top trigger set to: {col.name}");
    }

    private void Update()
    {
        // Check for holds
        if (activeColliders.Count == 1)
        {
            Collider activeCollider = activeColliders.First();
            
            // Check left hold
            if (activeCollider == leftCollider)
            {
                if (leftHoldStartTime < 0)
                {
                    leftHoldStartTime = Time.time;
                    if (showDebugLogs)
                        Debug.Log("Started left hold timer");
                }
                else if (!leftHoldTriggered && Time.time - leftHoldStartTime >= holdDuration)
                {
                    if (showDebugLogs)
                        Debug.Log("Left hold detected!");
                    OnStrokeDetected?.Invoke(StrokeDirection.HoldLeft);
                    leftHoldTriggered = true;
                }
            }
            // Check right hold
            else if (activeCollider == rightCollider)
            {
                if (rightHoldStartTime < 0)
                {
                    rightHoldStartTime = Time.time;
                    if (showDebugLogs)
                        Debug.Log("Started right hold timer");
                }
                else if (!rightHoldTriggered && Time.time - rightHoldStartTime >= holdDuration)
                {
                    if (showDebugLogs)
                        Debug.Log("Right hold detected!");
                    OnStrokeDetected?.Invoke(StrokeDirection.HoldRight);
                    rightHoldTriggered = true;
                }
            }
            // Check top hold
            else if (activeCollider == topCollider)
            {
                if (topHoldStartTime < 0)
                {
                    topHoldStartTime = Time.time;
                    if (showDebugLogs)
                        Debug.Log("Started top hold timer");
                }
                else if (!topHoldTriggered && Time.time - topHoldStartTime >= holdDuration)
                {
                    if (showDebugLogs)
                        Debug.Log("Top hold detected!");
                    OnStrokeDetected?.Invoke(StrokeDirection.HoldTop);
                    topHoldTriggered = true;
                }
            }
        }

        // Reset if no new triggers for a while
        if (triggerSequence.Count > 0 && Time.time - lastTriggerTime > triggerResetTime)
        {
            if (showDebugLogs)
                Debug.Log($"Resetting stroke sequence - timeout ({Time.time - lastTriggerTime:F2}s since last trigger)");
            ResetStrokeDetection();
        }
    }

    public void HandleTriggerEnter(Collider triggerCollider, Collider other)
    {
        // Check if we're still in cooldown from last successful stroke
        if (Time.time - lastSuccessfulStrokeTime < strokeCooldownTime)
        {
            if (showDebugLogs)
                Debug.Log($"Ignored trigger - in cooldown period ({strokeCooldownTime - (Time.time - lastSuccessfulStrokeTime):F2}s remaining)");
            return;
        }

        // Validate hand
        string otherPath = GetGameObjectPath(other.gameObject);
        bool isValidHand = otherPath.Contains(leftHandPath) || otherPath.Contains(rightHandPath);
        
        if (!isValidHand)
        {
            if (showDebugLogs)
                Debug.Log($"Rejected non-hand collider: {otherPath}");
            return;
        }

        // Add to active colliders
        activeColliders.Add(triggerCollider);

        // For left, right, and top colliders, we only care about holds
        if (triggerCollider == leftCollider || triggerCollider == rightCollider || triggerCollider == topCollider)
        {
            if (showDebugLogs)
                Debug.Log($"Hold collider entered: {triggerCollider.name} - checking for hold");
            return;
        }

        // If we have multiple active collisions, ignore this trigger
        if (activeColliders.Count > 1)
        {
            if (showDebugLogs)
                Debug.Log($"Ignored trigger due to multiple active collisions ({activeColliders.Count})");
            return;
        }

        // If we have an expected next trigger and this isn't it, ignore
        if (expectedNextTrigger != null && triggerCollider != expectedNextTrigger)
        {
            if (showDebugLogs)
                Debug.Log($"Ignored unexpected trigger {triggerCollider.name} - expected {expectedNextTrigger.name}");
            return;
        }

        // Determine stroke type for this trigger
        StrokeType triggerType = DetermineStrokeType(triggerCollider);

        // If we're already processing a different type of stroke, ignore this trigger
        if (currentStrokeType != StrokeType.None && triggerType != currentStrokeType)
        {
            if (showDebugLogs)
                Debug.Log($"Ignored {triggerType} trigger while processing {currentStrokeType} stroke");
            return;
        }

        // If this is the first trigger in a sequence, set the stroke type and expected next trigger
        if (currentStrokeType == StrokeType.None)
        {
            currentStrokeType = triggerType;
            expectedNextTrigger = DetermineExpectedNextTrigger(triggerCollider);
            if (showDebugLogs)
            {
                Debug.Log($"Starting new {currentStrokeType} stroke sequence");
                Debug.Log($"Expecting next trigger: {expectedNextTrigger.name}");
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"\n=== Trigger Entered ===");
            Debug.Log($"Hand collider: {other.name} from {otherPath}");
            Debug.Log($"Triggered collider: {triggerCollider.name}");
            Debug.Log($"Active collisions: {activeColliders.Count}");
            Debug.Log($"Stroke type: {currentStrokeType}");
        }

        // Add to sequence
        triggerSequence.Add(new TriggerEvent(triggerCollider, Time.time));
        lastTriggerTime = Time.time;

        // Play feedback sound
        //if (sceneController != null)
        //{
        //    sceneController.PlaySound("peep");
        //}

        // Check for stroke if we have enough triggers
        if (triggerSequence.Count >= 2 && !isProcessingStroke)
        {
            isProcessingStroke = true;
            CheckForStroke();
            isProcessingStroke = false;
        }
    }

    public void HandleTriggerExit(Collider triggerCollider, Collider other)
    {
        // Remove from active colliders
        activeColliders.Remove(triggerCollider);
        
        // Reset hold timers and flags when the respective colliders exit
        if (triggerCollider == leftCollider)
        {
            leftHoldStartTime = -1f;
            leftHoldTriggered = false;
            if (showDebugLogs)
                Debug.Log("Reset left hold");
        }
        else if (triggerCollider == rightCollider)
        {
            rightHoldStartTime = -1f;
            rightHoldTriggered = false;
            if (showDebugLogs)
                Debug.Log("Reset right hold");
        }
        else if (triggerCollider == topCollider)
        {
            topHoldStartTime = -1f;
            topHoldTriggered = false;
            if (showDebugLogs)
                Debug.Log("Reset top hold");
        }
        
        if (showDebugLogs)
            Debug.Log($"Trigger exit: {triggerCollider.name}, Active collisions: {activeColliders.Count}");
    }

    private StrokeType DetermineStrokeType(Collider triggerCollider)
    {
        if (triggerCollider == frontCollider || triggerCollider == backCollider)
        {
            return StrokeType.FrontBack;
        }
        return StrokeType.None;
    }

    private Collider DetermineExpectedNextTrigger(Collider currentTrigger)
    {
        if (currentTrigger == frontCollider)
            return backCollider;
        if (currentTrigger == backCollider)
            return frontCollider;
        return null;
    }

    private void CheckForStroke()
    {
        if (triggerSequence.Count < 2) return;

        Collider firstTrigger = triggerSequence[0].trigger;
        Collider lastTrigger = triggerSequence[triggerSequence.Count - 1].trigger;

        // Only process if we have front/back triggers
        if ((firstTrigger != frontCollider && firstTrigger != backCollider) ||
            (lastTrigger != frontCollider && lastTrigger != backCollider))
        {
            return;
        }

        StrokeDirection direction = DetermineStrokeDirection(firstTrigger, lastTrigger);
        
        if (direction != StrokeDirection.None)
        {
            if (Time.time - lastSuccessfulStrokeTime >= strokeCooldownTime)
            {
                OnStrokeDetected?.Invoke(direction);
                lastSuccessfulStrokeTime = Time.time;
                if (showDebugLogs)
                    Debug.Log($"Stroke detected: {direction}");
            }
            else if (showDebugLogs)
            {
                Debug.Log($"Stroke ignored - in cooldown ({strokeCooldownTime - (Time.time - lastSuccessfulStrokeTime):F2}s remaining)");
            }
        }

        ResetStrokeDetection();
    }

    private StrokeDirection DetermineStrokeDirection(Collider firstTrigger, Collider lastTrigger)
    {
        if (firstTrigger == frontCollider && lastTrigger == backCollider)
        {
            return StrokeDirection.FrontToBack;
        }
        else if (firstTrigger == backCollider && lastTrigger == frontCollider)
        {
            return StrokeDirection.BackToFront;
        }
        
        return StrokeDirection.None;
    }

    private void ResetStrokeDetection()
    {
        triggerSequence.Clear();
        activeColliders.Clear();
        isProcessingStroke = false;
        currentStrokeType = StrokeType.None;
        expectedNextTrigger = null;
        
        // Reset hold states
        leftHoldStartTime = -1f;
        rightHoldStartTime = -1f;
        topHoldStartTime = -1f;  // Reset top hold timer
        leftHoldTriggered = false;
        rightHoldTriggered = false;
        topHoldTriggered = false;  // Reset top hold flag
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
} 