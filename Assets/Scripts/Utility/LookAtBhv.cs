using UnityEngine;

[ExecuteInEditMode]
public class LookAtBhv : CachedTransformBhv
{
    // Public fields
    public Transform target;
    public Vector3 offset;

    private void Update()
    {
        if (target == null || Application.isPlaying)
        {
            return;
        }

        this.Transform.LookAt(target.position + offset, Vector3.up);
    }
}
