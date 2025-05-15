using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StrokeDetector : MonoBehaviour
{
    [Header("Hand References")]
    [SerializeField] private string rightHandPath = "Camera Rig/[BuildingBlock] Interaction/[BuildingBlock] Hand Interactions/RightHand";
    [SerializeField] private string leftHandPath = "Camera Rig/[BuildingBlock] Interaction/[BuildingBlock] Hand Interactions/LeftHand";

    [Header("Collider References")]
    [SerializeField] private GameObject frontTrigger;
    [SerializeField] private GameObject backTrigger;
    [SerializeField] private GameObject topTrigger;
    [SerializeField] private GameObject leftTrigger;   // Added left side trigger
    [SerializeField] private GameObject rightTrigger;  // Added right side trigger

    [Header("Stroke Detection Settings")]
    [SerializeField] private float strokeCooldownTime = 3.0f;
    [SerializeField] private float maxTimeBetweenTriggers = 2.0f;
    [SerializeField] private int maxSequenceLength = 5;

    [Header("Hold Detection Settings")]
    [SerializeField] private float requiredHoldTime = 5.0f;  // Time required to trigger hold event
    [SerializeField] private float holdEventCooldown = 10.0f;  // Cooldown between hold events

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private Collider frontCollider;
    private Collider backCollider;
    private Collider topCollider;
    private Collider leftCollider;   // Added left collider
    private Collider rightCollider;  // Added right collider

    private float leftHoldStartTime = -1f;   // Track when left hold started
    private float rightHoldStartTime = -1f;  // Track when right hold started
    private float lastHoldEventTime = -999f; // Track last hold event

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

        public override string ToString()
        {
            string triggerName = "Unknown";
            if (trigger != null)
            {
                triggerName = trigger.gameObject.name;
            }
            return $"{triggerName} @ {timestamp:F2}s";
        }
    }

    public delegate void StrokeDetectedHandler(StrokeDirection direction);
    public event StrokeDetectedHandler OnStrokeDetected;

    public delegate void HoldDetectedHandler();
    public event HoldDetectedHandler OnHoldDetected;

    private void Start()
    {
        Debug.Log("StrokeDetector: Starting collider setup...");
        
        // Get collider components
        if (frontTrigger != null)
        {
            frontCollider = frontTrigger.GetComponent<Collider>();
            Debug.Log($"Front trigger found: {frontTrigger.name}, Has Collider: {frontCollider != null}, Is Active: {frontTrigger.activeInHierarchy}");
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
            Debug.Log($"Back trigger found: {backTrigger.name}, Has Collider: {backCollider != null}, Is Active: {backTrigger.activeInHierarchy}");
            if (backCollider == null)
            {
                Debug.LogError("Back trigger has no Collider component!");
            }
        }
        else
        {
            Debug.LogError("Back trigger GameObject not assigned!");
        }

        if (topTrigger != null)
        {
            topCollider = topTrigger.GetComponent<Collider>();
            Debug.Log($"Top trigger found: {topTrigger.name}, Has Collider: {topCollider != null}, Is Active: {topTrigger.activeInHierarchy}");
            if (topCollider == null)
            {
                Debug.LogError("Top trigger has no Collider component!");
            }
        }
        else
        {
            Debug.LogError("Top trigger GameObject not assigned!");
        }

        if (leftTrigger != null)
        {
            leftCollider = leftTrigger.GetComponent<Collider>();
            Debug.Log($"Left trigger found: {leftTrigger.name}, Has Collider: {leftCollider != null}, Is Active: {leftTrigger.activeInHierarchy}");
            if (leftCollider == null)
            {
                Debug.LogError("Left trigger has no Collider component!");
            }
        }
        else
        {
            Debug.LogError("Left trigger GameObject not assigned!");
        }

        if (rightTrigger != null)
        {
            rightCollider = rightTrigger.GetComponent<Collider>();
            Debug.Log($"Right trigger found: {rightTrigger.name}, Has Collider: {rightCollider != null}, Is Active: {rightTrigger.activeInHierarchy}");
            if (rightCollider == null)
            {
                Debug.LogError("Right trigger has no Collider component!");
            }
        }
        else
        {
            Debug.LogError("Right trigger GameObject not assigned!");
        }

        // Log collider sizes and positions
        LogColliderDetails(frontCollider, "Front");
        LogColliderDetails(backCollider, "Back");
        LogColliderDetails(topCollider, "Top");
        LogColliderDetails(leftCollider, "Left");
        LogColliderDetails(rightCollider, "Right");

        // Ensure triggers are set up correctly
        SetupTrigger(frontCollider, "Front");
        SetupTrigger(backCollider, "Back");
        SetupTrigger(topCollider, "Top");
        SetupTrigger(leftCollider, "Left");
        SetupTrigger(rightCollider, "Right");
        
        Debug.Log("StrokeDetector: Collider setup complete");
    }

    private void Update()
    {
        // Check for hold completion on both sides
        CheckHoldCompletion();
    }

    private void CheckHoldCompletion()
    {
        float currentTime = Time.time;

        // Check if either hold has completed
        if (leftHoldStartTime > 0 && currentTime - leftHoldStartTime >= requiredHoldTime)
        {
            TriggerHoldEvent();
            leftHoldStartTime = -1f;
        }
        else if (rightHoldStartTime > 0 && currentTime - rightHoldStartTime >= requiredHoldTime)
        {
            TriggerHoldEvent();
            rightHoldStartTime = -1f;
        }
    }

    private void TriggerHoldEvent()
    {
        float currentTime = Time.time;
        if (currentTime - lastHoldEventTime >= holdEventCooldown)
        {
            if (showDebugLogs)
            {
                Debug.Log("Hold event triggered!");
            }
            OnHoldDetected?.Invoke();
            lastHoldEventTime = currentTime;
        }
        else if (showDebugLogs)
        {
            float remainingCooldown = holdEventCooldown - (currentTime - lastHoldEventTime);
            Debug.Log($"Hold event ignored - cooldown remaining: {remainingCooldown:F2}s");
        }
    }

    private void LogColliderDetails(Collider col, string name)
    {
        if (col != null)
        {
            if (col is BoxCollider box)
            {
                Debug.Log($"{name} Collider Details - Size: {box.size}, Center: {box.center}, World Position: {col.transform.position}");
            }
            else if (col is SphereCollider sphere)
            {
                Debug.Log($"{name} Collider Details - Radius: {sphere.radius}, Center: {sphere.center}, World Position: {col.transform.position}");
            }
            else if (col is CapsuleCollider capsule)
            {
                Debug.Log($"{name} Collider Details - Radius: {capsule.radius}, Height: {capsule.height}, Center: {capsule.center}, World Position: {col.transform.position}");
            }
        }
    }

    private void SetupTrigger(Collider col, string name)
    {
        if (col != null)
        {
            if (!col.isTrigger)
            {
                Debug.LogWarning($"{name} collider is not set as trigger - fixing!");
                col.isTrigger = true;
            }
            
            // Log the layer information
            Debug.Log($"{name} Collider Layer: {LayerMask.LayerToName(col.gameObject.layer)} ({col.gameObject.layer})");
            
            // Verify the collider is enabled
            if (!col.enabled)
            {
                Debug.LogWarning($"{name} collider is disabled - enabling!");
                col.enabled = true;
            }
        }
    }

    public void HandleTriggerEnter(Collider triggerCollider, Collider other)
    {
        if (showDebugLogs)
        {
            string triggerType = "Unknown";
            if (triggerCollider == frontCollider) triggerType = "Front";
            else if (triggerCollider == backCollider) triggerType = "Back";
            else if (triggerCollider == topCollider) triggerType = "Top";
            else if (triggerCollider == leftCollider) triggerType = "Left";
            else if (triggerCollider == rightCollider) triggerType = "Right";
            
            Debug.Log($"Trigger Enter - Type: {triggerType}, Name: {triggerCollider.name}, Other: {other.name}, Other Path: {GetGameObjectPath(other.gameObject)}");
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

        // Handle side collider holds
        if (triggerCollider == leftCollider && leftHoldStartTime < 0)
        {
            leftHoldStartTime = Time.time;
            if (showDebugLogs)
                Debug.Log("Started left side hold");
            return;
        }
        else if (triggerCollider == rightCollider && rightHoldStartTime < 0)
        {
            rightHoldStartTime = Time.time;
            if (showDebugLogs)
                Debug.Log("Started right side hold");
            return;
        }

        // Only process stroke triggers
        if (triggerCollider != frontCollider && triggerCollider != backCollider && triggerCollider != topCollider)
        {
            if (showDebugLogs)
            {
                Debug.Log($"Ignored non-stroke trigger collider: {triggerCollider.name}");
            }
            return;
        }

        // Add to active colliders and sequence
        if (!activeColliders.Contains(triggerCollider))
        {
            activeColliders.Add(triggerCollider);
            triggerSequence.Add(new TriggerEvent(triggerCollider, Time.time));

            // Limit sequence length
            if (triggerSequence.Count > maxSequenceLength)
            {
                triggerSequence.RemoveAt(0);
            }

            if (showDebugLogs)
            {
                Debug.Log($"Added trigger to sequence: {triggerCollider.name}");
                Debug.Log($"Current sequence: {string.Join(" -> ", triggerSequence)}");
                Debug.Log($"Active colliders: {string.Join(", ", activeColliders.Select(c => GetColliderType(c)))}");
            }

            // Check for stroke completion
            CheckForStroke();
        }
    }

    private string GetColliderType(Collider col)
    {
        if (col == frontCollider) return "Front";
        if (col == backCollider) return "Back";
        if (col == topCollider) return "Top";
        if (col == leftCollider) return "Left";
        if (col == rightCollider) return "Right";
        return "Unknown";
    }

    public void HandleTriggerExit(Collider triggerCollider, Collider other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Trigger Exit - Trigger: {triggerCollider.name}, Other: {other.name}");
        }

        // Reset hold timers if exiting side colliders
        if (triggerCollider == leftCollider)
        {
            leftHoldStartTime = -1f;
            if (showDebugLogs)
                Debug.Log("Cancelled left side hold");
        }
        else if (triggerCollider == rightCollider)
        {
            rightHoldStartTime = -1f;
            if (showDebugLogs)
                Debug.Log("Cancelled right side hold");
        }

        activeColliders.Remove(triggerCollider);
    }

    private void CheckForStroke()
    {
        // Remove old events from the sequence
        float currentTime = Time.time;
        triggerSequence.RemoveAll(evt => currentTime - evt.timestamp > maxTimeBetweenTriggers);

        if (triggerSequence.Count < 3)
        {
            if (showDebugLogs)
                Debug.Log($"Not enough triggers in sequence (count: {triggerSequence.Count})");
            return;
        }

        // Look for valid sequences in the last several triggers
        for (int i = 0; i <= triggerSequence.Count - 3; i++)
        {
            var threeTriggersToCheck = triggerSequence.Skip(i).Take(3).ToList();
            
            // Verify timing between these three triggers
            if (threeTriggersToCheck[2].timestamp - threeTriggersToCheck[0].timestamp <= maxTimeBetweenTriggers)
            {
                StrokeDirection direction = DetermineStrokeDirection(threeTriggersToCheck);
                if (direction != StrokeDirection.None)
                {
                    // Check cooldown
                    if (Time.time - lastSuccessfulStrokeTime >= strokeCooldownTime)
                    {
                        if (showDebugLogs)
                        {
                            Debug.Log($"Valid stroke detected: {direction}");
                            Debug.Log($"Sequence that triggered: {string.Join(" -> ", threeTriggersToCheck)}");
                        }
                        OnStrokeDetected?.Invoke(direction);
                        lastSuccessfulStrokeTime = Time.time;
                        ResetStrokeDetection();
                        return;
                    }
                    else if (showDebugLogs)
                    {
                        float remainingCooldown = strokeCooldownTime - (Time.time - lastSuccessfulStrokeTime);
                        Debug.Log($"Stroke ignored - cooldown remaining: {remainingCooldown:F2}s");
                    }
                }
            }
        }
    }

    private StrokeDirection DetermineStrokeDirection(List<TriggerEvent> threeTriggersToCheck)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Checking direction for sequence: {string.Join(" -> ", threeTriggersToCheck)}");
        }

        // Check for Front -> Top -> Back sequence
        if (threeTriggersToCheck[0].trigger == frontCollider && 
            threeTriggersToCheck[1].trigger == topCollider && 
            threeTriggersToCheck[2].trigger == backCollider)
        {
            return StrokeDirection.FrontToBack;
        }
        // Check for Back -> Top -> Front sequence
        else if (threeTriggersToCheck[0].trigger == backCollider && 
                 threeTriggersToCheck[1].trigger == topCollider && 
                 threeTriggersToCheck[2].trigger == frontCollider)
        {
            return StrokeDirection.BackToFront;
        }

        if (showDebugLogs)
        {
            Debug.Log("Sequence did not match any valid stroke pattern");
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