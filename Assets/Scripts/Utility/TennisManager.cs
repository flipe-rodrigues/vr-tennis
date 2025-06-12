using UnityEngine;

public class TennisManager : Singleton<TennisManager>
{
    // Public properties
    public BallRigidbodyBhv Ball { get {return _ball; } set { _ball = value; } }
    public RacketRigidbodyBhv Racket => _racket;
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
    private BallRigidbodyBhv _ball;
    [SerializeField, ReadOnly]
    private RacketRigidbodyBhv _racket;

    private void OnValidate()
    {
        if (_ball == null)
        {
            _ball = FindFirstObjectByType<BallRigidbodyBhv>();
        }
        if (_racket == null)
        {
            _racket = FindFirstObjectByType<RacketRigidbodyBhv>();
        }
    }

    protected override void Awake()
    {
        base.Awake();

        this.OnValidate();
    }
}
