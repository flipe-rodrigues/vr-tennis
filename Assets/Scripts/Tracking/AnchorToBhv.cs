using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AnchorToBhv : CachedTransformBhv
{
    public Transform anchorTransform;
    [Range(0, 5)]
    public float invokeDelay = 1f;
    public bool overridePosition;
    
    private float _updateInterval;
    private WaitForSeconds _waitForSeconds;

    private void OnValidate()
    {
        _updateInterval = 1f / ApplicationManager.Instance.targetPhysicsRate;

        _waitForSeconds = new WaitForSeconds(_updateInterval);

        this.AlignTransforms();
    }

    private void Start()
    {
        if (anchorTransform == null)
        {
            return;
        }

        this.OnValidate();

        Invoke(nameof(AlignTransforms), invokeDelay);

        if (overridePosition)
        {
            StartCoroutine(this.UpdateCoroutine());
        }
    }

    private IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            this.Position = anchorTransform.position;

            yield return _waitForSeconds;
        }
    }

    [ContextMenu("Align Transforms")]
    public void AlignTransforms()
    {
        if (anchorTransform == null)
        {
            return;
        }

        Vector3 forward = Vector3.ProjectOnPlane(anchorTransform.forward, Vector3.up).normalized;

        this.transform.forward = forward;
    }
}
