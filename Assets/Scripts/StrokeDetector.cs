using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StrokeDetector : MonoBehaviour
{
    [Header("Hand References")]
    [SerializeField] private string rightHandPath = "Camera Rig/[BuildingBlock] Interaction/[BuildingBlock] Hand Interactions/RightHand";
    [SerializeField] private string leftHandPath = "Camera Rig/[BuildingBlock] Interaction/[BuildingBlock] Hand Interactions/LeftHand";

    [Header("Collider References")]
    [SerializeField] private GameObject frontTrigger;  // Changed to GameObject to get both collider and transform
    [SerializeField] private GameObject backTrigger;   // Changed to GameObject to get both collider and transform

    [Header("Stroke Detection Settings")]
    [SerializeField] private float strokeCooldownTime = 3.0f; // Time before another stroke can be triggered

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private Collider frontCollider;
    private Collider backCollider;

    public enum StrokeDirection
    {
        None,
        FrontToBack,
        BackToFront
    }

    private List<TriggerEvent> triggerSequence = new List<TriggerEvent>();
    private float lastSuccessfulStrokeTime = -999f;
    private HashSet<Collider> activeColliders = new HashSet<Collider>();

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

    public delegate void StrokeDetectedHandler(StrokeDirection direction);
    public event StrokeDetectedHandler OnStrokeDetected;

    private void Start()
    {
        // Get collider components from the trigger GameObjects
        if (frontTrigger != null)
        {
            frontCollider = frontTrigger.GetComponent<Collider>();
            if (frontCollider == null)
            {
                Debug.LogError("Front trigger has no Collider component!");
            }
        }
        else
        {
            Debug.LogError("Front trigger GameObject not assigned!");
        }

        if (backTrigger != null)
        {
            backCollider = backTrigger.GetComponent<Collider>();
            if (backCollider == null)
            {
                Debug.LogError("Back trigger has no Collider component!");
            }
        }
        else
        {
            Debug.LogError("Back trigger GameObject not assigned!");
        }

        // Ensure triggers are set up correctly
        if (frontCollider != null)
        {
            if (!frontCollider.isTrigger)
            {
                Debug.LogWarning("Front collider is not set as trigger!");
                frontCollider.isTrigger = true;
            }
        }

        if (backCollider != null)
        {
            if (!backCollider.isTrigger)
            {
                Debug.LogWarning("Back collider is not set as trigger!");
                backCollider.isTrigger = true;
            }
        }
    }

    public void HandleTriggerEnter(Collider triggerCollider, Collider other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Trigger Enter - Trigger: {triggerCollider.name}, Other: {other.name}");
        }

        // Basic hand validation
        string otherPath = GetGameObjectPath(other.gameObject);
        if (!otherPath.Contains(leftHandPath) && !otherPath.Contains(rightHandPath))
        {
            if (showDebugLogs)
            {
                Debug.Log($"Ignored non-hand collider: {otherPath}");
            }
            return;
        }

        // Only process front and back triggers
        if (triggerCollider != frontCollider && triggerCollider != backCollider)
        {
            if (showDebugLogs)
            {
                Debug.Log($"Ignored unknown trigger collider: {triggerCollider.name}");
            }
            return;
        }

        // Add to active colliders and sequence
        activeColliders.Add(triggerCollider);
        triggerSequence.Add(new TriggerEvent(triggerCollider, Time.time));

        if (showDebugLogs)
        {
            Debug.Log($"Added trigger to sequence: {triggerCollider.name}");
            Debug.Log($"Current sequence length: {triggerSequence.Count}");
        }

        // Check for stroke completion
        CheckForStroke();
    }

    public void HandleTriggerExit(Collider triggerCollider, Collider other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Trigger Exit - Trigger: {triggerCollider.name}, Other: {other.name}");
        }

        activeColliders.Remove(triggerCollider);
    }

    private void CheckForStroke()
    {
        if (triggerSequence.Count < 2) return;

        // Get first and last triggers
        Collider firstTrigger = triggerSequence[0].trigger;
        Collider lastTrigger = triggerSequence[triggerSequence.Count - 1].trigger;
        float timeDifference = triggerSequence[triggerSequence.Count - 1].timestamp - triggerSequence[0].timestamp;

        if (showDebugLogs)
        {
            Debug.Log($"Checking stroke - First: {firstTrigger.name}, Last: {lastTrigger.name}, Time: {timeDifference}");
        }

        // Determine stroke direction
        StrokeDirection direction = DetermineStrokeDirection(firstTrigger, lastTrigger);
        
        // Check if enough time has passed since last stroke
        if (direction != StrokeDirection.None && Time.time - lastSuccessfulStrokeTime >= strokeCooldownTime)
        {
            if (showDebugLogs)
            {
                Debug.Log($"Stroke detected: {direction}");
            }
            OnStrokeDetected?.Invoke(direction);
            lastSuccessfulStrokeTime = Time.time;
        }
        else if (showDebugLogs)
        {
            if (direction == StrokeDirection.None)
            {
                Debug.Log("No valid stroke direction detected");
            }
            else
            {
                Debug.Log($"Stroke ignored - cooldown remaining: {strokeCooldownTime - (Time.time - lastSuccessfulStrokeTime):F2}s");
            }
        }

        // Reset for next stroke
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
        if (showDebugLogs)
        {
            Debug.Log("Resetting stroke detection");
        }
        triggerSequence.Clear();
        activeColliders.Clear();
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