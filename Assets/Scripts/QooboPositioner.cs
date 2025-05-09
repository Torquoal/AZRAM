using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class QooboPositioner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject qooboMesh;
    [SerializeField] private SceneController sceneController;
    
    [Header("Settings")]
    [SerializeField] private float handHeightOffset = -0.1f; // Offset BELOW hand position (negative value)
    [SerializeField] private float handForwardOffset = 0.2f; // Offset FORWARD from hand position
    [SerializeField] private float pinchThreshold = 0.02f; // How close fingers need to be for pinch
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool isPositioned = false;
    private bool isRepositioning = false;
    private bool hasPinchPositioned = false; // New flag to track if pinch positioning has been used
    private XRHandSubsystem handSubsystem;

    void Start()
    {
        if (qooboMesh == null)
        {
            Debug.LogError("QooboMesh reference not set in QooboPositioner!");
            enabled = false;
            return;
        }

        if (sceneController == null)
        {
            Debug.LogError("SceneController reference not set in QooboPositioner!");
            enabled = false;
            return;
        }

        // Get the hand tracking subsystem
        var handSubsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(handSubsystems);
        if (handSubsystems.Count > 0)
        {
            handSubsystem = handSubsystems[0];
            if (showDebugLogs) Debug.Log("Hand tracking subsystem initialized successfully");
        }
        else
        {
            Debug.LogError("No hand tracking subsystem found!");
            enabled = false;
        }
    }

    void Update()
    {
        if (handSubsystem == null) return;

        // Check if both hands are tracked
        bool leftHandTracked = handSubsystem.leftHand.isTracked;
        bool rightHandTracked = handSubsystem.rightHand.isTracked;

        if (showDebugLogs)
        {
            Debug.Log($"Current states - isPositioned: {isPositioned}, isRepositioning: {isRepositioning}");
            Debug.Log($"Hand tracking - Left: {leftHandTracked}, Right: {rightHandTracked}");
        }

        // Check for Space key using new Input System
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && rightHandTracked)
        {
            Debug.Log("Space key pressed - Starting repositioning");
            isRepositioning = true;
            isPositioned = false;
            UpdateQooboPosition();
            // Reset states after space key positioning
            isRepositioning = false;
            isPositioned = false;
        }

        if (leftHandTracked && rightHandTracked)
        {
            // Get left hand pinch gesture
            XRHandJoint leftThumbTip = handSubsystem.leftHand.GetJoint(XRHandJointID.ThumbTip);
            XRHandJoint leftIndexTip = handSubsystem.leftHand.GetJoint(XRHandJointID.IndexTip);
            
            // Get positions from joints
            Vector3 leftThumbPos = leftThumbTip.TryGetPose(out Pose thumbPose) ? thumbPose.position : Vector3.zero;
            Vector3 leftIndexPos = leftIndexTip.TryGetPose(out Pose indexPose) ? indexPose.position : Vector3.zero;
            
            float pinchDistance = Vector3.Distance(leftThumbPos, leftIndexPos);
            bool isPinching = pinchDistance < pinchThreshold;
            
            if (showDebugLogs)
            {
                Debug.Log($"Pinch distance: {pinchDistance}, Threshold: {pinchThreshold}, IsPinching: {isPinching}");
            }

            if (isPinching && !hasPinchPositioned) // Only allow pinch if it hasn't been used before
            {
                if (!isPositioned || isRepositioning)
                {
                    // Only allow pinch repositioning if wake-up is not complete
                    if (!sceneController.IsWakeUpComplete())
                    {
                        Debug.Log($"Pinch detected - Updating position (isPositioned: {isPositioned}, isRepositioning: {isRepositioning})");
                        UpdateQooboPosition();
                        hasPinchPositioned = true; // Set the flag after first successful pinch positioning
                        Debug.Log("Pinch positioning has been used - further positioning only available via spacebar");
                    }
                    else
                    {
                        Debug.Log("Pinch detected but wake-up sequence is complete - ignoring");
                    }
                }
                else
                {
                    Debug.Log("Pinch detected but position is locked");
                }
            }
            else if (isPinching && hasPinchPositioned)
            {
                Debug.Log("Pinch detected but pinch positioning has already been used - use spacebar to reposition");
            }
        }
    }

    public void UpdateQooboPosition()
    {
        Debug.Log($"UpdateQooboPosition called - States before update: isPositioned: {isPositioned}, isRepositioning: {isRepositioning}");

        if (handSubsystem == null || !handSubsystem.rightHand.isTracked) 
        {
            Debug.LogWarning("Cannot update position - right hand not tracked");
            return;
        }

        // Get right hand palm position and rotation
        XRHandJoint rightPalm = handSubsystem.rightHand.GetJoint(XRHandJointID.Palm);
        if (!rightPalm.TryGetPose(out Pose palmPose))
        {
            Debug.LogWarning("Cannot update position - failed to get palm pose");
            return;
        }

        Vector3 rightPalmPosition = palmPose.position;
        Quaternion rightPalmRotation = palmPose.rotation;

        if (rightPalmPosition == Vector3.zero)
        {
            Debug.LogWarning("Cannot update position - invalid palm position");
            return;
        }

        // Calculate the position below and forward of the palm based on palm's orientation
        Vector3 downDirection = rightPalmRotation * Vector3.down;
        Vector3 forwardDirection = rightPalmRotation * Vector3.forward;
        
        // Apply both vertical and forward offsets
        Vector3 targetPos = rightPalmPosition + (downDirection * Mathf.Abs(handHeightOffset)) + 
                                              (forwardDirection * handForwardOffset);
        
        Vector3 oldPosition = qooboMesh.transform.position;
        
        // Instant position update
        qooboMesh.transform.position = targetPos;

        // Instant rotation update
        Vector3 palmForward = rightPalmRotation * Vector3.forward;
        palmForward.y = 0; // Zero out vertical component
        if (palmForward != Vector3.zero)
        {
            qooboMesh.transform.rotation = Quaternion.LookRotation(palmForward, Vector3.up);
        }

        Debug.Log($"Position updated - Old: {oldPosition}, New: {targetPos}, Movement delta: {Vector3.Distance(oldPosition, targetPos)}");
        
        // Notify SceneController to start wake up sequence
        sceneController.StartWakeUpSequence();
        
        Debug.Log($"UpdateQooboPosition complete - States after update: isPositioned: {isPositioned}, isRepositioning: {isRepositioning}");
    }

    public bool IsPositioned()
    {
        return isPositioned && !isRepositioning;
    }
} 