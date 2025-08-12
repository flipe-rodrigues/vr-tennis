using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AnchorToBhv : MonoBehaviour
{
    public Transform anchorTransform;
    [Range(0, 5)]
    public float invokeDelay = 1f;
    [Range(60, 500)]
    public float updateRate = 250f;

    private float _updateInterval;
    private WaitForSeconds _waitForSeconds;

    private void OnValidate()
    {
        _updateInterval = 1f / updateRate;

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

        StartCoroutine(this.UpdateCoroutine());
    }

    private IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            this.transform.position = anchorTransform.position;

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
