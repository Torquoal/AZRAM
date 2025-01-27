using UnityEngine;

public class StrokeTriggerDetector : MonoBehaviour
{
    public enum TriggerType
    {
        Front,
        Back,
        Left,
        Right
    }

    [SerializeField] private StrokeDetector mainDetector;
    [SerializeField] private TriggerType triggerType;

    private void Start()
    {
        if (mainDetector == null)
        {
            mainDetector = GetComponentInParent<StrokeDetector>();
            if (mainDetector == null)
            {
                Debug.LogError($"No StrokeDetector found for {gameObject.name}!");
                return;
            }
        }

        // Register this trigger with the main detector based on its type
        Collider col = GetComponent<Collider>();
        switch (triggerType)
        {
            case TriggerType.Front:
                mainDetector.SetFrontTrigger(col);
                break;
            case TriggerType.Back:
                mainDetector.SetBackTrigger(col);
                break;
            case TriggerType.Left:
                mainDetector.SetLeftTrigger(col);
                break;
            case TriggerType.Right:
                mainDetector.SetRightTrigger(col);
                break;
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