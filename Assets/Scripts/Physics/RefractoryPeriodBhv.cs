using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RefractoryPeriodBhv : MonoBehaviour
{
    // Public fields
    public LayerMask layerMask;
    public float refractoryPeriod = 0.1f;

    // Private fields
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & layerMask) != 0)
        {
            StartCoroutine(this.RefractoryPeriodCoroutine());
        }
    }

    private IEnumerator RefractoryPeriodCoroutine()
    {
        _collider.enabled = false;

        yield return new WaitForSeconds(0.1f);

        _collider.enabled = true;
    }
}
