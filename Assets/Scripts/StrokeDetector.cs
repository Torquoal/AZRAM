using UnityEngine;
using System.Collections.Generic;

public class StrokeDetector : MonoBehaviour
{
    [Header("Hand References")]
    [SerializeField] private string leftHandPath = "Camera Rig/[BuildingBlock] Interaction/[BuildingBlock] Hand Interactions/LeftHand";
    [SerializeField] private string rightHandPath = "Camera Rig/[BuildingBlock] Interaction/[BuildingBlock] Hand Interactions/RightHand";

    [Header("Stroke Detection Settings")]
    [SerializeField] private float maxStrokeTime = 1.0f;
    [SerializeField] private float triggerResetTime = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    [Header("References")]
    [SerializeField] private SceneController sceneController;

    // These will be set by the StrokeTriggerDetector components
    private Collider frontCollider;
    private Collider backCollider;
    private Collider leftCollider;
    private Collider rightCollider;

    public enum StrokeDirection
    {
        None,
        FrontToBack,
        BackToFront,
        LeftToRight,
        RightToLeft
    }

    private enum StrokeType
    {
        None,
        FrontBack,
        LeftRight
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

    private void Update()
    {
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
        // Validate hand
        string otherPath = GetGameObjectPath(other.gameObject);
        bool isValidHand = otherPath.Contains(leftHandPath) || otherPath.Contains(rightHandPath);
        
        if (!isValidHand)
        {
            if (showDebugLogs)
                Debug.Log($"Rejected non-hand collider: {otherPath}");
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

        // If this is the first trigger in a sequence, set the stroke type
        if (currentStrokeType == StrokeType.None)
        {
            currentStrokeType = triggerType;
            if (showDebugLogs)
                Debug.Log($"Starting new {currentStrokeType} stroke sequence");
        }

        if (showDebugLogs)
        {
            Debug.Log($"\n=== Trigger Entered ===");
            Debug.Log($"Hand collider: {other.name} from {otherPath}");
            Debug.Log($"Triggered collider: {triggerCollider.name}");
            Debug.Log($"Stroke type: {currentStrokeType}");
        }

        // Add to sequence
        triggerSequence.Add(new TriggerEvent(triggerCollider, Time.time));
        lastTriggerTime = Time.time;

        if (showDebugLogs)
        {
            Debug.Log($"Added to sequence. Current length: {triggerSequence.Count}");
        }

        // Play feedback sound
        if (sceneController != null)
        {
            sceneController.PlaySound("peep");
        }

        // Check for stroke if we have enough triggers
        if (triggerSequence.Count >= 2 && !isProcessingStroke)
        {
            isProcessingStroke = true;
            CheckForStroke();
            isProcessingStroke = false;
        }
    }

    private StrokeType DetermineStrokeType(Collider triggerCollider)
    {
        if (triggerCollider == frontCollider || triggerCollider == backCollider)
            return StrokeType.FrontBack;
        if (triggerCollider == leftCollider || triggerCollider == rightCollider)
            return StrokeType.LeftRight;
        return StrokeType.None;
    }

    private void CheckForStroke()
    {
        if (triggerSequence.Count < 2) return;

        var firstTrigger = triggerSequence[0];
        var lastTrigger = triggerSequence[triggerSequence.Count - 1];
        float strokeTime = lastTrigger.timestamp - firstTrigger.timestamp;

        if (showDebugLogs)
        {
            Debug.Log($"\n=== Checking Stroke ===");
            Debug.Log($"First trigger: {firstTrigger.trigger.name}");
            Debug.Log($"Last trigger: {lastTrigger.trigger.name}");
            Debug.Log($"Stroke time: {strokeTime:F3}s");
        }

        if (strokeTime > maxStrokeTime)
        {
            if (showDebugLogs)
                Debug.Log($"Stroke too slow: {strokeTime:F3}s > {maxStrokeTime:F3}s");
            ResetStrokeDetection();
            return;
        }

        StrokeDirection direction = DetermineStrokeDirection(firstTrigger.trigger, lastTrigger.trigger);
        
        if (direction != StrokeDirection.None)
        {
            if (showDebugLogs)
                Debug.Log($"Valid stroke detected: {direction}");
            OnStrokeDetected?.Invoke(direction);
        }

        ResetStrokeDetection();
    }

    private StrokeDirection DetermineStrokeDirection(Collider firstTrigger, Collider lastTrigger)
    {
        // Front-back strokes
        if (firstTrigger == frontCollider && lastTrigger == backCollider)
            return StrokeDirection.FrontToBack;
        if (firstTrigger == backCollider && lastTrigger == frontCollider)
            return StrokeDirection.BackToFront;

        // Left-right strokes
        if (firstTrigger == leftCollider && lastTrigger == rightCollider)
            return StrokeDirection.LeftToRight;
        if (firstTrigger == rightCollider && lastTrigger == leftCollider)
            return StrokeDirection.RightToLeft;

        return StrokeDirection.None;
    }

    private void ResetStrokeDetection()
    {
        triggerSequence.Clear();
        isProcessingStroke = false;
        currentStrokeType = StrokeType.None;
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