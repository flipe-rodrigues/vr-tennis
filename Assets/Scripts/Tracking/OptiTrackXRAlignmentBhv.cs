using UnityEngine;
using UnityEngine.InputSystem;

public class OptiTrackXRAlignmentBhv : CachedTransformBhv
{
    // Public fields
    public Transform cameraOffsetTransform;
    public Transform mainCameraTransform;
    public InputActionReference alignPositionActionReference;
    public InputActionReference alignRotationActionReference;
    [Min(0f)]
    public float invokeDelay = 1.0f;

    private void Start()
    {
        Invoke("AlignPosition", invokeDelay);
        Invoke("AlignRotation", invokeDelay);
    }

    private void Update()
    {
        if (alignPositionActionReference.action.triggered && cameraOffsetTransform != null)
        {
            this.AlignPosition();
        }

        if (alignRotationActionReference.action.triggered && cameraOffsetTransform != null)
        {
            this.AlignRotation();
        }
    }

    private void AlignPosition()
    {
        cameraOffsetTransform.position = this.Position;
    }

    private void AlignRotation()
    {
        Vector3 hmdProjection = Vector3.ProjectOnPlane(this.Forward, Vector3.up).normalized;
        Vector3 cameraFloorProjection = Vector3.ProjectOnPlane(mainCameraTransform.forward, Vector3.up).normalized;
        Vector3 offsetFloorProjection = Vector3.ProjectOnPlane(cameraOffsetTransform.forward, Vector3.up).normalized;
        Quaternion rotation = Quaternion.FromToRotation(cameraFloorProjection, offsetFloorProjection);
        cameraOffsetTransform.forward = rotation * hmdProjection;
    }
}
