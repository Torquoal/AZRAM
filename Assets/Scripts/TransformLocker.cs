using UnityEngine;

public class TransformLocker : MonoBehaviour
{
    [Header("Lock Settings")]
    [SerializeField] public bool lockPosition = true;
    [SerializeField] public bool lockRotation = true;
    [SerializeField] public bool lockScale = true;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private void Start()
    {
        // Store initial transform values
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        initialScale = transform.localScale;
    }

    private void LateUpdate()
    {
        // Reset transform values after any potential changes
        if (lockPosition)
        {
            transform.localPosition = initialPosition;
        }
        
        if (lockRotation)
        {
            transform.localRotation = initialRotation;
        }
        
        if (lockScale)
        {
            transform.localScale = initialScale;
        }
    }

    // Public methods to change lock states
    public void SetPositionLock(bool locked) => lockPosition = locked;
    public void SetRotationLock(bool locked) => lockRotation = locked;
    public void SetScaleLock(bool locked) => lockScale = locked;

    // Method to update initial transform values if needed
    public void UpdateInitialTransform()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        initialScale = transform.localScale;
    }
} 