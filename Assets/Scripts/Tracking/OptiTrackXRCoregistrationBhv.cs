using UnityEngine;
using UnityEngine.InputSystem;

public class OptiTrackXRCoregistrationBhv : CachedTransformBhv
{
    public Transform cameraOffsetTransform;
    public Transform mainCameraTransform;
    public InputActionReference overlapActionReference;
    public InputActionReference alignActionReference;

    private void Update()
    {
        if (overlapActionReference.action.triggered && cameraOffsetTransform != null)
        {
            cameraOffsetTransform.position = this.Position;
        }

        if (alignActionReference.action.triggered && cameraOffsetTransform != null)
        {
            cameraOffsetTransform.forward = Vector3.ProjectOnPlane(this.Forward, Vector3.up).normalized;
        }
    }
}
