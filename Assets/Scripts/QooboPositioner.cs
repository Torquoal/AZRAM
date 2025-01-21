using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class QooboPositioner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject qooboMesh;
    
    [Header("Settings")]
    [SerializeField] private float positionSmoothTime = 0.1f; // Smoothing time for position updates
    [SerializeField] private float handHeightOffset = 0.1f; // Offset above hand position
    [SerializeField] private float pinchThreshold = 0.02f; // How close fingers need to be for pinch
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool isPositioned = false;
    private bool isRepositioning = false;
    private Vector3 velocityRef;
    private XRHandSubsystem handSubsystem;

    void Start()
    {
        if (qooboMesh == null)
        {
            Debug.LogError("QooboMesh reference not set in QooboPositioner!");
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

        // For testing in editor with new Input System
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (showDebugLogs) Debug.Log("Reposition key pressed");
            ToggleRepositioning();
            return;
        }

        // Check if both hands are tracked
        bool leftHandTracked = handSubsystem.leftHand.isTracked;
        bool rightHandTracked = handSubsystem.rightHand.isTracked;

        if (leftHandTracked && rightHandTracked)
        {
            // Get left hand pinch gesture
            XRHandJoint leftThumbTip = handSubsystem.leftHand.GetJoint(XRHandJointID.ThumbTip);
            XRHandJoint leftIndexTip = handSubsystem.leftHand.GetJoint(XRHandJointID.IndexTip);
            
            // Get positions from joints
            Vector3 leftThumbPos = leftThumbTip.TryGetPose(out Pose thumbPose) ? thumbPose.position : Vector3.zero;
            Vector3 leftIndexPos = leftIndexTip.TryGetPose(out Pose indexPose) ? indexPose.position : Vector3.zero;
            
            bool isPinching = Vector3.Distance(leftThumbPos, leftIndexPos) < pinchThreshold;
            if (isPinching && showDebugLogs)
            {
                Debug.Log("Pinch detected");
            }

            // Get right hand palm position for positioning
            XRHandJoint rightPalm = handSubsystem.rightHand.GetJoint(XRHandJointID.Palm);
            Vector3 rightPalmPosition = rightPalm.TryGetPose(out Pose palmPose) ? palmPose.position : Vector3.zero;

            if (isPinching && rightPalmPosition != Vector3.zero)
            {
                if (!isPositioned || isRepositioning)
                {
                    if (showDebugLogs) Debug.Log($"Positioning Qoobo at right palm position: {rightPalmPosition}");
                    UpdateQooboPosition(rightPalmPosition);
                }
            }
            else if (isRepositioning)
            {
                // Lock in position when pinch is released
                if (showDebugLogs) Debug.Log("Position locked");
                isRepositioning = false;
                isPositioned = true;
            }
        }
        else
        {
            if (showDebugLogs && (!leftHandTracked || !rightHandTracked))
            {
                Debug.Log($"Hand tracking status - Left: {leftHandTracked}, Right: {rightHandTracked}");
            }
        }
    }

    private void UpdateQooboPosition(Vector3 targetPos)
    {
        // Add height offset and smooth the movement
        targetPos.y += handHeightOffset;
        
        qooboMesh.transform.position = Vector3.SmoothDamp(
            qooboMesh.transform.position,
            targetPos,
            ref velocityRef,
            positionSmoothTime
        );
    }

    // Public method to start repositioning
    public void StartRepositioning()
    {
        ToggleRepositioning();
    }

    private void ToggleRepositioning()
    {
        isRepositioning = !isRepositioning;
        if (isRepositioning)
        {
            if (showDebugLogs) Debug.Log("Entering repositioning mode");
            isPositioned = false;
        }
    }

    // Public method to get positioning status
    public bool IsPositioned()
    {
        return isPositioned && !isRepositioning;
    }
} 