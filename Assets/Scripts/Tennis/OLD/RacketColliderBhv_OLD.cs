using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class RacketColliderBhv_OLD : CachedTransformBhv
{
    // Protected properties
    protected Collider Collider => _collider;

    // Private properties
    private MeshRenderer MeshRenderer => _meshRenderer == null ? GetComponent<MeshRenderer>() : _meshRenderer;

    // Public fields
    public bool displayAsMesh;

    // Private fields
    private RacketBhv_OLD _racketBhv;
    private MeshRenderer _meshRenderer;
    private Collider _collider;

    protected virtual void OnValidate()
    {
        this.MeshRenderer.enabled = displayAsMesh;
    }

    protected override void Awake()
    {
        base.Awake();

        _racketBhv = GetComponentInParent<RacketBhv_OLD>();

        _meshRenderer = GetComponent<MeshRenderer>();

        _collider = GetComponent<Collider>();
    }

    protected virtual void Start()
    {
        this.OnValidate();
    }

    private void OnTriggerEnter(Collider other)
    {
        _racketBhv.OnTriggerStay(other);
    }

    private void OnTriggerStay(Collider other)
    {
        _racketBhv.OnTriggerStay(other);
    }

    private void OnTriggerExit(Collider other)
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

        while (timer < _racketBhv.refractoryPeriod)
        {
            timer += Time.fixedDeltaTime;

            yield return ApplicationManager.waitForFixedUpdateInstance;
        }

        _meshRenderer.enabled = displayAsMesh;

        _collider.enabled = true;
    }
}
