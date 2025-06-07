using UnityEngine;

public class RacketBhv : CachedRigidbodyBhvOLD
{
    public float hitRefractoryPeriod = 0.1f;

    private float _lastHitTime = -1f;
    [SerializeField]
    private bool _canHit = true;

    public bool CanHit()
    {
        return _canHit;
    }

    public void Hit()
    {
        _lastHitTime = Time.time;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        _canHit = Time.time - _lastHitTime >= hitRefractoryPeriod;
    }
}
