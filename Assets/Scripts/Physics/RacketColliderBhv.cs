using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class RacketColliderBhv : CachedTransformBhv
{
    // Private properties
    private MeshRenderer MeshRenderer => _meshRenderer == null ? GetComponent<MeshRenderer>() : _meshRenderer;

    // Public fields
    public bool displayAsMesh;

    // Private fields
    private RacketBhv _racketBhv;
    private MeshRenderer _meshRenderer;
    private Collider _collider;

    private void OnValidate()
    {
        this.MeshRenderer.enabled = displayAsMesh;
    }

    protected override void Awake()
    {
        base.Awake();

        _racketBhv = GetComponentInParent<RacketBhv>();

        _meshRenderer = GetComponent<MeshRenderer>();

        _collider = GetComponent<Collider>();
    }

    private void Start()
    {
        this.OnValidate();
    }

    private void OnTriggerStay(Collider other)
    {
        _racketBhv.OnTriggerStay(other);
    }

    public void StartRefractoryPeriod()
    {
        StartCoroutine(this.RefractoryPeriodCoroutine());
    }

    private IEnumerator RefractoryPeriodCoroutine()
    {
        _meshRenderer.enabled = false;

        _collider.enabled = false;

        float timer = 0f;

        while (timer < TennisManager.Instance.Racket.refractoryPeriod)
        {
            timer += Time.fixedDeltaTime;

            yield return ApplicationManager.waitForFixedUpdateInstance;
        }

        _meshRenderer.enabled = displayAsMesh;

        _collider.enabled = true;
    }
}
