using UnityEngine;

public class RacketAnchorBhv : CachedTransformBhv
{
    // Public fields
    public RacketRigidbodyBhv_OLD anchoredRigidbody;

    private void OnValidate()
    {
        if (anchoredRigidbody != null && anchoredRigidbody.anchorTransform != this)
        {
            anchoredRigidbody.anchorTransform = this;
        }
    }
}
