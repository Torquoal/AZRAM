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
        float angle = Vector3.Angle(headDirection, directionToQoobo);

        // Update the debug text
        angleDebugText.text = $"Facing Angle: {angle:F1}°";

        // Draw debug ray if enabled
        if (showDebugRay)
        {
            Debug.DrawRay(arCamera.transform.position, headDirection * debugRayLength, debugRayColor);
        }
    }
} 