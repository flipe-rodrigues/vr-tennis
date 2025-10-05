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
        float alpha = Vector3.Angle(cameraOffsetTransform.forward, mainCameraTransform.forward);
        Vector3 forward = Vector3.ProjectOnPlane(this.Forward, Vector3.up).normalized;
        cameraOffsetTransform.forward = Quaternion.AngleAxis(alpha, Vector3.up) * forward;
    }
}
