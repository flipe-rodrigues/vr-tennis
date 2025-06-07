using UnityEngine;

public class SetTransformHideFlags : MonoBehaviour
{
    public new HideFlags hideFlags = HideFlags.None;

    private void OnValidate()
    {
        transform.hideFlags = hideFlags;
    }
}
