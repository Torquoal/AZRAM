using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StrokeTriggerDetector : MonoBehaviour
{
    [SerializeField] private StrokeDetector mainDetector;

    private void Start()
    {
        if (mainDetector == null)
        {
            mainDetector = GetComponentInParent<StrokeDetector>();
            if (mainDetector == null)
            {
                Debug.LogError($"No StrokeDetector found for {gameObject.name}!");
                enabled = false;
                return;
            }
        }

        // Ensure collider is set as trigger
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (mainDetector != null)
        {
            mainDetector.HandleTriggerEnter(GetComponent<Collider>(), other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (mainDetector != null)
        {
            mainDetector.HandleTriggerExit(GetComponent<Collider>(), other);
        }
    }
} 