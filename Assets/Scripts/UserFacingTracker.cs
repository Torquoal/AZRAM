using UnityEngine;
using TMPro;

public class UserFacingTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject qooboMesh;
    [SerializeField] private TextMeshProUGUI angleDebugText;
    [SerializeField] private Camera arCamera;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugRay = true;
    [SerializeField] private float debugRayLength = 2f;
    [SerializeField] private Color debugRayColor = Color.yellow;

    [Header("Eye Contact Settings")]
    [SerializeField] private float requiredLookDuration = 5f;
    [SerializeField] private float angleThreshold = 10f;

    [SerializeField] private SceneController sceneController;

    private bool hasShown = false;
    private float angleToQoobo;
    private float sustainedLookTimer = 0f;
    private bool isLookingAtRobot = false;

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

        // Check if looking at robot within threshold
        bool currentlyLooking = angleToQoobo < angleThreshold;

        if (currentlyLooking && !isLookingAtRobot)
        {
            // Just started looking at robot
            isLookingAtRobot = true;
            sustainedLookTimer = 0f;
        }
        else if (!currentlyLooking && isLookingAtRobot)
        {
            // Stopped looking at robot
            isLookingAtRobot = false;
            sustainedLookTimer = 0f;
        }
        else if (currentlyLooking && isLookingAtRobot)
        {
            // Continue timing the look duration
            sustainedLookTimer += Time.deltaTime;

            // Check if we've reached the required duration
            if (sustainedLookTimer >= requiredLookDuration && sceneController.wakeUpComplete && !hasShown)
            {
                sceneController.ShowThought("looking");
                hasShown = true;
            }
        }

        // Update the debug text
        angleDebugText.text = $"Facing Angle: {angleToQoobo:F1}°\nLook Timer: {sustainedLookTimer:F1}s";

        // Draw debug ray if enabled
        if (showDebugRay)
        {
            Debug.DrawRay(arCamera.transform.position, headDirection * debugRayLength, debugRayColor);
        }
    }
} 