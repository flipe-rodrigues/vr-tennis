using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class OptiTrackXRAlignmentBhv : CachedTransformBhv
{
    // Public fields
    public Transform trackingSpaceTransform;
    public Transform mainCameraTransform;
    public Transform leftControllerTransform;
    public InputActionReference alignPositionActionReference;
    public InputActionReference alignRotationActionReference;
    public InputActionReference togglePassthroughActionReference;
    public XRInputValueReader<Vector2> fineTunePositionInput;
    public XRInputValueReader<Vector2> fineTuneRotationInput;
    [Min(0f)]
    public float invokeDelay = 1.0f;
    [Range(0f,1f)]
    public float moveSpeed = 0.1f;
    [Range(0f, 10f)]
    public float rotateSpeed = 0.1f;

    // Readonly fields
    [SerializeField, ReadOnly]
    private Vector3 _offsetPosition;
    [SerializeField, ReadOnly]
    private Vector3 _offsetRotationEuler;

    private void Start()
    {
        Invoke("TogglePassthrough", invokeDelay);
        Invoke("RecenterPose", invokeDelay);
        Invoke("AlignPosition", invokeDelay);
        Invoke("AlignRotation", invokeDelay);
    }

    private void Update()
    {
        if (OVRManager.instance == null)
        {
            return;
        }

        if (trackingSpaceTransform == null || mainCameraTransform == null)
        {
            return;
        }

        if (alignPositionActionReference.action.triggered || !OVRManager.instance.usePositionTracking)
        {
            this.AlignPosition();
        }

        if (alignRotationActionReference.action.triggered)
        {
            this.AlignRotation();
        }

        if (togglePassthroughActionReference.action.triggered)
        {
            this.TogglePassthrough();
        }

        Vector2 moveInput = fineTunePositionInput.ReadValue();
        if (moveInput.sqrMagnitude > 0)
        {
            Vector3 moveInputWorld = new Vector3(moveInput.x, 0f, moveInput.y);
            //Vector3 moveInputLocal = leftControllerTransform.InverseTransformDirection(moveInputWorld);
            _offsetPosition += moveInputWorld * moveSpeed * Time.deltaTime;
            this.AlignPosition();
        }

        Vector2 rotateInput = fineTuneRotationInput.ReadValue();
        if (rotateInput.sqrMagnitude > 0)
        {
            Vector3 eulers = new Vector3(0f, rotateInput.x, 0f) * rotateSpeed * Time.deltaTime;
            this.Transform.Rotate(eulers);
            this.AlignRotation();
            //OVRManager.instance.headPoseRelativeOffsetRotation = _offsetRotation;
        }
    }

    private void RecenterPose()
    {
        //OVRManager.display.RecenterPose();
    }

    private void AlignPosition()
    {
        if (OVRManager.instance == null)
        {
            return;
        }

        Vector3 position = this.Position;

        if (OVRManager.instance.usePositionTracking)
        {
            position = Vector3.ProjectOnPlane(position, Vector3.up);
        }

        trackingSpaceTransform.position = position + _offsetPosition;
        //OVRManager.instance.headPoseRelativeOffsetTranslation = positionOffset;
    }

    private void AlignRotation()
    {
        Vector3 hmdProjection = Vector3.ProjectOnPlane(this.Forward, Vector3.up).normalized;
        Vector3 cameraFloorProjection = Vector3.ProjectOnPlane(mainCameraTransform.forward, Vector3.up).normalized;
        Vector3 offsetFloorProjection = Vector3.ProjectOnPlane(trackingSpaceTransform.forward, Vector3.up).normalized;
        Quaternion rotation = Quaternion.FromToRotation(cameraFloorProjection, offsetFloorProjection);
        trackingSpaceTransform.forward = rotation * hmdProjection;
        //Vector3 currentOffsetForward = Quaternion.Euler(OVRManager.instance.headPoseRelativeOffsetRotation) * Vector3.forward;
        //Vector3 cameraFloorProjection = Vector3.ProjectOnPlane(mainCameraTransform.forward, Vector3.up).normalized;
        //Vector3 offsetFloorProjection = Vector3.ProjectOnPlane(currentOffsetForward, Vector3.up).normalized;
        //Quaternion rotation = Quaternion.FromToRotation(cameraFloorProjection, offsetFloorProjection);
        //OVRManager.instance.headPoseRelativeOffsetRotation = rotation.eulerAngles;
    }

    private void TogglePassthrough()
    {
        if (OVRManager.instance == null)
        {
            return;
        }

        OVRManager.instance.isInsightPassthroughEnabled = !OVRManager.instance.isInsightPassthroughEnabled;
    }
}
