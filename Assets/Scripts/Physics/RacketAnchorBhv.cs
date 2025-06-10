using UnityEngine;

public class RacketAnchorBhv : CachedTransformBhv
{
    // Public fields
    public RacketRigidbodyBhv anchoredRigidbody;

    private void OnValidate()
    {
        if (anchoredRigidbody != null && anchoredRigidbody.anchorTransform != this)
        {
            anchoredRigidbody.anchorTransform = this;
        }
    }

    private void LateUpdate()
    {
        Physics.SyncTransforms();
    }
}
