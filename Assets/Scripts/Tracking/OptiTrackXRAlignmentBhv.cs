using UnityEngine;
using UnityEngine.InputSystem;

public class OptiTrackXRAlignmentBhv : CachedTransformBhv
{
    public Transform cameraOffsetTransform;
    public Transform mainCameraTransform;
    public InputActionReference alignPositionActionReference;
    public InputActionReference alignRotationActionReference;

    private void Start()
    {
        this.AlignPosition();
        this.AlignRotation();
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
