using UnityEngine;

public class TennisManager : Singleton<TennisManager>
{
    // Public properties
    public BallBhv Ball { get {return _ball; } set { _ball = value; } }
    public RacketBhv Racket => _racket;
    public Vector3 RelativePosition
    {
        get 
        { 
            if (_ball == null || _racket == null)
            {
                return Vector3.zero;
            }

            return _ball.Position - _racket.Position;
        }
    }
    public Vector3 RelativeVelocity 
    {
        get 
        { 
            if (_ball == null || _racket == null)
            {
                return Vector3.zero;
            }

            return _racket.LinearVelocity - _ball.LinearVelocity;
        }
    }

    // Read only fields
    [SerializeField, ReadOnly]
    private BallBhv _ball;
    [SerializeField, ReadOnly]
    private RacketBhv _racket;

    protected override void OnValidate()
    {
        base.OnValidate();

        if (_ball == null)
        {
            _ball = FindFirstObjectByType<BallBhv>();
        }
        if (_racket == null)
        {
            _racket = FindFirstObjectByType<RacketBhv>();
        }
    }

    protected override void Awake()
    {
        base.Awake();

        this.OnValidate();
    }
}
