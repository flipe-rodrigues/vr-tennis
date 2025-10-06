using UnityEngine;

public class RacketTargetBhv : TargetBhv
{
    // Public fields
    [Range(0, 1f)]
    public float overlapThreshold = 0.9f;

    // Readonly fields
    [SerializeField, ReadOnly, Range(0f, 1f)]
    private float _overlapFraction;

    // Private fields
    private Timer _overlapTimer;
    private Timer _refractoryTimer;

    private void Start()
    {
        _overlapTimer = new Timer(base.acquisitionDelay);
        _refractoryTimer = new Timer(base.resetDelay);
    }

    private void Update()
    {
        if (!_refractoryTimer.IsExpired)
        {
            return;
        }

        _overlapFraction = this.OverlapFraction(this.MeshRenderer, TennisManager.Instance.Racket.MeshRenderer);

        if (_overlapFraction >= overlapThreshold)
        {
            if (!_overlapTimer.IsRunning)
            {
                _overlapTimer.Start();

                base.TryAcquireAt(this.Position, 1f);
            }
            else if (_overlapTimer.IsExpired)
            {
                _overlapTimer.Stop();
                _refractoryTimer.Start();
            }
        }
        else
        {
            if (_overlapTimer.IsRunning)
            {
                base.Reset();

                _overlapTimer.Stop();
            }
        }
    }

    private float OverlapFraction(MeshRenderer a, MeshRenderer b)
    {
        Bounds A = a.bounds;
        Bounds B = b.bounds;

        float overlap = this.GetBoundsOverlapVolume(A, B);
        float volume = A.size.x * A.size.y * A.size.z;

        return overlap / Mathf.Max(volume, 1e-6f);
    }

    private float GetBoundsOverlapVolume(Bounds a, Bounds b)
    {
        if (!a.Intersects(b))
        {
            return 0f;
        }

        float x = Mathf.Max(0, Mathf.Min(a.max.x, b.max.x) - Mathf.Max(a.min.x, b.min.x));
        float y = Mathf.Max(0, Mathf.Min(a.max.y, b.max.y) - Mathf.Max(a.min.y, b.min.y));
        float z = Mathf.Max(0, Mathf.Min(a.max.z, b.max.z) - Mathf.Max(a.min.z, b.min.z));

        return x * y * z;
    }
}
