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

    protected override void Start()
    {
        base.Start();

        _overlapTimer = new Timer(base.hitDelay);
        _refractoryTimer = new Timer((base.hitDelay + base.resetDelay) * 2f);
    }

    private void Update()
    {
        if (_refractoryTimer.IsExpired)
        {
            _refractoryTimer.Stop();
        }
        else if (_refractoryTimer.IsRunning)
        {
            return;
        }

        _overlapFraction = this.OverlapFraction(this.MeshRenderer, TennisManager.Instance.Racket.Mesh.Renderer);

        if (_overlapFraction >= overlapThreshold)
        {
            if (!_overlapTimer.IsRunning)
            {
                base.TryHit();
                _overlapTimer.Start();

                TrackingManager.Instance.RecordTaskEvent(TaskEventType.TargetEnter);
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

                TrackingManager.Instance.RecordTaskEvent(TaskEventType.TargetExit);
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
