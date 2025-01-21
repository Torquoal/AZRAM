using UnityEngine;

public class FaceController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeshFilter faceMeshFilter;
    [SerializeField] private MeshRenderer faceMeshRenderer;
    [SerializeField] private Material defaultFaceMaterial;

    [Header("Face Materials")]
    [SerializeField] private Material happyFaceMaterial;
    [SerializeField] private Material sadFaceMaterial;
    [SerializeField] private Material angryFaceMaterial;
    [SerializeField] private Material scaredFaceMaterial;
    [SerializeField] private Material surprisedFaceMaterial;
    [SerializeField] private Material neutralFaceMaterial;

    [Header("Face Display Settings")]
    [SerializeField] private float faceOffset = 0.01f; // Distance in front of Qoobo mesh
    [SerializeField] private float faceDiameter = 0.214f; // Diameter of the Qoobo from top view
    [SerializeField] private float faceHeight = 0.15f; // Vertical size of the face
    [SerializeField] private float curvatureAngle = 60f; // How much the face curves around
    [SerializeField] private int curveResolution = 20; // Number of segments for the curve
    [SerializeField] private float scaleFactor = 2f; // Overall scale multiplier

    private void Start()
    {
        if (faceMeshFilter == null)
        {
            Debug.LogError("Face Mesh Filter not assigned!");
            return;
        }

        // Rotate the face object 180 degrees around Y axis
        transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        CreateCurvedFaceMesh();
        SetFaceExpression("neutral"); // Start with neutral expression
        
        Debug.Log($"Face mesh created with settings: Scale={scaleFactor}, Height={faceHeight}, Diameter={faceDiameter}, Curvature={curvatureAngle}");
    }

    private void CreateCurvedFaceMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "FaceMesh";

        // Calculate dimensions
        float radius = (faceDiameter * 0.5f) * scaleFactor;
        float halfHeight = (faceHeight * 0.5f) * scaleFactor;
        float angleStep = curvatureAngle / (curveResolution - 1);
        float startAngle = -curvatureAngle * 0.5f;

        // Create vertices
        Vector3[] vertices = new Vector3[curveResolution * 2];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[(curveResolution - 1) * 6];

        Debug.Log($"Creating mesh with radius={radius}, halfHeight={halfHeight}");

        for (int i = 0; i < curveResolution; i++)
        {
            float angle = (startAngle + i * angleStep) * Mathf.Deg2Rad;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius - radius; // Offset to place mesh in front

            // Top vertex
            vertices[i * 2] = new Vector3(x, halfHeight, z + faceOffset);
            uvs[i * 2] = new Vector2((float)i / (curveResolution - 1), 1);

            // Bottom vertex
            vertices[i * 2 + 1] = new Vector3(x, -halfHeight, z + faceOffset);
            uvs[i * 2 + 1] = new Vector2((float)i / (curveResolution - 1), 0);

            // Create triangles
            if (i < curveResolution - 1)
            {
                int baseIndex = i * 6;
                int vertIndex = i * 2;

                // First triangle
                triangles[baseIndex] = vertIndex;
                triangles[baseIndex + 1] = vertIndex + 2;
                triangles[baseIndex + 2] = vertIndex + 1;

                // Second triangle
                triangles[baseIndex + 3] = vertIndex + 2;
                triangles[baseIndex + 4] = vertIndex + 3;
                triangles[baseIndex + 5] = vertIndex + 1;
            }
        }

        // Assign mesh data
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        faceMeshFilter.mesh = mesh;
        
        // Verify mesh creation
        Debug.Log($"Mesh created with {vertices.Length} vertices, bounds: {mesh.bounds}");
    }

    public void SetFaceExpression(string expression)
    {
        Material targetMaterial = null;

        switch (expression.ToLower())
        {
            case "happy":
                targetMaterial = happyFaceMaterial;
                break;
            case "sad":
                targetMaterial = sadFaceMaterial;
                break;
            case "angry":
                targetMaterial = angryFaceMaterial;
                break;
            case "scared":
                targetMaterial = scaredFaceMaterial;
                break;
            case "surprised":
                targetMaterial = surprisedFaceMaterial;
                break;
            case "neutral":
                targetMaterial = neutralFaceMaterial;
                break;
            default:
                targetMaterial = defaultFaceMaterial;
                Debug.LogWarning($"Unknown expression: {expression}, using default face");
                break;
        }

        if (targetMaterial != null && faceMeshRenderer != null)
        {
            faceMeshRenderer.material = targetMaterial;
            Debug.Log($"Set face material to {expression}");
        }
        else
        {
            Debug.LogError($"Failed to set face material. Material: {(targetMaterial == null ? "null" : "valid")}, Renderer: {(faceMeshRenderer == null ? "null" : "valid")}");
        }
    }

    private void SetFaceToHappy(){
        SetFaceExpression("happy");
    }

    // Public method to update the face offset at runtime if needed
    public void UpdateFaceOffset(float newOffset)
    {
        faceOffset = newOffset;
        CreateCurvedFaceMesh();
        Debug.Log($"Updated face offset to {newOffset}");
    }

    // Public method to update the curvature at runtime if needed
    public void UpdateCurvature(float newAngle)
    {
        curvatureAngle = newAngle;
        CreateCurvedFaceMesh();
        Debug.Log($"Updated curvature angle to {newAngle}");
    }

    // Method to update scale at runtime
    public void UpdateScale(float newScale)
    {
        scaleFactor = newScale;
        CreateCurvedFaceMesh();
        Debug.Log($"Updated scale factor to {newScale}");
    }

    // Editor-only validation
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            CreateCurvedFaceMesh();
            Debug.Log("Mesh updated due to parameter change in inspector");
        }
    }
} 