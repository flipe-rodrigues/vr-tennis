using Oculus.Interaction;
using UnityEngine;

public class FeedbackBhv : MonoBehaviour
{
    // Public fields
    public LayerMask layerMask;
    [Range(0, 1)]
    public float amplitudeModifier = .1f;

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & layerMask) != 0)
        {
            this.Play(collision);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & layerMask) != 0)
        {
            this.Play();
        }
    }

    protected virtual void Play() { }
    protected virtual void Play(Collision collision) { }
}
