using UnityEngine;
using UnityEngine.InputSystem;

public class AnchorBhv : CachedTransformBhv
{
    public Transform anchoredTransform;
    public InputActionReference overlapActionReference;
    public InputActionReference alignActionReference;

    private void Update()
    {
        if (overlapActionReference.action.triggered && anchoredTransform != null)
        {
            anchoredTransform.position = this.Position;
        }

        if (alignActionReference.action.triggered && anchoredTransform != null)
        {
            anchoredTransform.forward = Vector3.ProjectOnPlane(this.Forward, Vector3.up).normalized;
        }
    }
}
