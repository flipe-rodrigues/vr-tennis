using Oculus.Interaction;
using UnityEngine;

public class FeedbackBhv : CachedTransformBhv
{
    // Public fields
    public LayerMask layerMask;

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & layerMask) != 0)
        {
            this.Play(collision);
        }
    }

    public virtual void Play(float speed) { }
    protected virtual void Play(Collision collision) { }
}
