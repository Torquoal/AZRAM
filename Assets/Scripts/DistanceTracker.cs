using UnityEngine;
using TMPro;

public class DistanceTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject qooboMesh;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private Canvas worldSpaceCanvas;

    [Header("Settings")]
    [SerializeField] private bool showDebugText = true;
    [SerializeField] private string distanceFormat = "Proxemity: {0:F2}m";
    [SerializeField] private float updateInterval = 0.05f; // Increased update frequency

    private Camera mainCamera;
    private float currentDistance;
    private float timeSinceLastUpdate;

    private void Start()
    {
        // Get the main camera reference
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            enabled = false;
            return;
        }

        if (qooboMesh == null)
        {
            Debug.LogError("QooboMesh reference not set!");
            enabled = false;
            return;
        }

        // Ensure the canvas faces the camera
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
        }
        else
        {
            Debug.LogWarning("World Space Canvas not assigned!");
        }

        UpdateDistanceDisplay();
    }

    private void LateUpdate() // Changed to LateUpdate to ensure camera position is final
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Try to get camera again if lost
            if (mainCamera == null) return;
        }

        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= updateInterval)
        {
            UpdateDistanceDisplay();
            timeSinceLastUpdate = 0f;
        }
    }

    private void UpdateDistanceDisplay()
    {
        if (mainCamera != null && qooboMesh != null)
        {
            // Calculate distance between camera and Qoobo
            Vector3 cameraPosition = mainCamera.transform.position;
            Vector3 qooboPosition = qooboMesh.transform.position;
            
            currentDistance = Vector3.Distance(cameraPosition, qooboPosition);

            // Log positions for debugging
            if (showDebugText)
            {
                Debug.Log($"Camera pos: {cameraPosition}, Qoobo pos: {qooboPosition}, Distance: {currentDistance}");
            }

            // Update UI text if available - removed showDebugText check here
            if (distanceText != null)
            {
                distanceText.text = string.Format(distanceFormat, currentDistance);
            }

            // Make canvas face camera
            if (worldSpaceCanvas != null)
            {
                Vector3 lookDirection = mainCamera.transform.position - worldSpaceCanvas.transform.position;
                lookDirection.y = 0; // Optional: keep text vertical
                if (lookDirection != Vector3.zero)
                {
                    worldSpaceCanvas.transform.rotation = Quaternion.LookRotation(-lookDirection); // Negative to face camera
                }
            }
        }
    }

    // Public accessor for distance
    public float GetCurrentDistance()
    {
        UpdateDistanceDisplay(); // Force update when getting distance
        return currentDistance;
    }

    // Toggle debug text visibility
    public void SetDebugTextVisibility(bool visible)
    {
        showDebugText = visible;
        if (distanceText != null)
        {
            distanceText.enabled = visible;
        }
    }
} 