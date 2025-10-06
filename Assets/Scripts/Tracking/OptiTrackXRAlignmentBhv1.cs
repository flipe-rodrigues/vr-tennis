using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class OptiTrackXRAlignmentBhv1 : CachedTransformBhv
{
    // Public fields
    public Transform cameraOffsetTransform;
    public Transform mainCameraTransform;
    public InputActionReference alignPositionActionReference;
    public InputActionReference alignRotationActionReference;
    public XRInputValueReader<Vector2> fineTunePositionInput;
    [Min(0f)]
    public float invokeDelay = 1.0f;
    public bool overridePosition;

    // Readonly fields
    [SerializeField, ReadOnly]
    private Vector3 _offset;
    public float moveSpeed = 0.1f;

    // Private fields
    private XROrigin _xrOrigin;
    private Camera _mainCamera;

    protected override void Awake()
    {
        base.Awake();

        _xrOrigin = FindFirstObjectByType<XROrigin>();
    }

    private void Start()
    {
        _mainCamera = Camera.main;

        Invoke("AlignPosition", invokeDelay);
        Invoke("AlignRotation", invokeDelay);
    }

    private void Update()
    {
        if (cameraOffsetTransform == null || mainCameraTransform == null)
        {
            return;
        }

        if (alignPositionActionReference.action.triggered)
        {
            //_xrOrigin.MoveCameraToWorldLocation(Vector3.zero);
            //_xrOrigin.MatchOriginUpCameraForward(Vector3.up, this.Forward);

            this.AlignPosition();

            //StartCoroutine(this.CaptureFrame("camera"));
        }

        if (alignRotationActionReference.action.triggered)
        {
            this.AlignRotation();

            //OVRManager.instance.isInsightPassthroughEnabled = true;
            //StartCoroutine(this.CaptureFrame("passthrough"));
            //OVRManager.instance.isInsightPassthroughEnabled = false;
        }

        Vector2 moveInput = fineTunePositionInput.ReadValue();
        if (moveInput.sqrMagnitude > 0)
        {
            Vector3 moveInputWorld = new Vector3(moveInput.x, 0f, moveInput.y);
            Vector3 moveInputCamera = mainCameraTransform.InverseTransformDirection(moveInputWorld);
            _offset += Vector3.ProjectOnPlane(moveInputCamera, Vector3.up) * moveSpeed * Time.deltaTime;
            this.Transform.localPosition = _offset;
            this.AlignPosition();
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

    private IEnumerator CaptureFrame(string filename)
    {
        yield return new WaitForEndOfFrame();
        RenderTexture rt = _mainCamera.targetTexture;
        if (rt == null)
        {
            rt = new RenderTexture(Screen.width, Screen.height, 24);
            _mainCamera.targetTexture = rt;
        }

        Texture2D frame = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        frame.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        frame.Apply();

        RenderTexture.active = null;
        _mainCamera.targetTexture = null;

        // Example: Save to file
        byte[] bytes = frame.EncodeToPNG();
        System.IO.File.WriteAllBytes(DataManager.Instance.SavePath + $"/{filename}.png", bytes);
        Debug.Log("Captured main camera frame.");
    }
}
