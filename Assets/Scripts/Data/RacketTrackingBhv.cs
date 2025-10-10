using UnityEngine;

public class RacketTrackingBhv : TrackingBhv
{
    // Public properties
    public override Vector3 Position => _racket.Position;
    public override Quaternion Rotation => _racket.Rotation;

    // Private fields
    private RacketBhv _racket;

    protected override void Awake()
    {
        base.Awake();
        _racket = this.GetComponent<RacketBhv>();
    }
}
