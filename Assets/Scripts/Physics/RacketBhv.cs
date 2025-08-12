using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.XR;

[ExecuteInEditMode]
public class RacketBhv : CachedTransformBhv
{
    // Public properties
    public Vector3 HitVelocity => _hitLinearVelocity;
    public Vector3 HitContactNormal => _hitContactNormal;
    public Vector3 LinearVelocity => _linearVelocity;
    public Vector3 AngularVelocity => _angularVelocity;
    public bool IsInRefractoryPeriod => _racketCollider.enabled == false;

    // Public fields
    public XRController inputController;
    public float apparentNormalRestitution = .4f;
    public float apparentTangentialRestitution = .65f;
    public float apparentSpinRestitution = .4f;
    public float spinToTangentialConversion = .3f;
    public float tangentialToSpinConversion = .58f;
    [Min(0f)]
    public float refractoryPeriod = 0.05f;
    [Min(0.001f)]
    public float smoothingTimeConstantVelocity = 0.01f;
    [Min(0.001f)]
    public float smoothingTimeConstantNormal = 0.005f;
    public UnityEvent<float> onRacketHit = new UnityEvent<float>();

    // Private fields
    private RacketColliderBhv _racketCollider;
    private Vector3 _contactNormal;
    private Vector3 _linearVelocity;
    private Vector3 _angularVelocity;
    private Vector3 _hitLinearVelocity;
    private Vector3 _hitContactNormal;
    private Vector3 _previousPosition;
    private Quaternion _previousRotation;

    protected override void Awake()
    {
        base.Awake();

        _racketCollider = GetComponentInChildren<RacketColliderBhv>();
    }

    private void FixedUpdate()
    {
        this.UpdateLinearVelocity();

        this.UpdateAngularVelocity();

        //_racketCollider.CheckForCollision();

        //if (true)
        //{
        //    this.OnTriggerStay(new Collider());
        //}
    }

    private void UpdateLinearVelocity()
    {
        _linearVelocity = (this.Position - _previousPosition) / Time.fixedDeltaTime;

        _previousPosition = this.Position;
    }

    private void UpdateAngularVelocity()
    {
        Quaternion deltaRotation = this.Rotation * Quaternion.Inverse(_previousRotation);

        deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 axis);

        _angularVelocity = axis * (angleInDegrees * Mathf.Deg2Rad) / Time.fixedDeltaTime;

        _previousRotation = this.Rotation;
    }

    private void OnTriggerStay(Collider other)
    {
        _contactNormal = this.GetContactNormal();

        if (Vector3.Dot(_contactNormal, TennisManager.Instance.RelativePosition) < 0)
        {
            _racketCollider.StartRefractoryPeriod();

            float relativeSpeed = (_linearVelocity - TennisManager.Instance.Ball.LinearVelocity).magnitude;

            _hitLinearVelocity = this.GetVelocityAtContactPoint();

            _hitContactNormal = _contactNormal;

            this.Hit(TennisManager.Instance.Ball);

            onRacketHit?.Invoke(relativeSpeed);

            TennisManager.Instance.Ball.WasJustHit = true;

            TrackingManager.Instance.RecordTaskEvent(TaskEventType.RacketHit);
        }
    }

    private void Hit(BallRigidbodyBhv ball)
    {
        // Following Cross 2005
        Vector3 v_racket_i = this.GetVelocityAtContactPoint();
        Vector3 v_ball_i = ball.LinearVelocity;
        Vector3 w_ball_i = ball.AngularVelocity;

        // Separate initial velocity into normal and tangential components
        Vector3 v_ball_normal_i = Vector3.Project(v_ball_i, _contactNormal);
        Vector3 v_ball_tangential_i = v_ball_i - v_ball_normal_i;

        Vector3 v_racket_normal_i = Vector3.Project(v_racket_i, _contactNormal);
        Vector3 v_racket_tangential_i = v_racket_i - v_racket_normal_i;

        // Apply restitution to normal component
        Vector3 v_ball_normal_f = 
            (1 + apparentNormalRestitution) * v_racket_normal_i + apparentNormalRestitution * -v_ball_normal_i;

        // Apply friction and spin effects to tangential component
        Vector3 v_ball_tangential_f = 
            apparentTangentialRestitution * (v_racket_tangential_i + v_ball_tangential_i) +
            spinToTangentialConversion * ball.radius * Vector3.Cross(w_ball_i, _contactNormal);

        // Calculate final velocity
        Vector3 v_ball_f = v_ball_normal_f + v_ball_tangential_f;
        Vector3 v_ball_y_f = Vector3.Project(v_ball_f, Vector3.up);
        Vector3 v_ball_x_f = v_ball_f - v_ball_y_f;
        v_ball_f = v_ball_y_f + v_ball_x_f;

        // Calculate the final angular velocity of the ball
        Vector3 w_ball_f = 
            apparentSpinRestitution * w_ball_i +
            tangentialToSpinConversion * Vector3.Cross(_contactNormal, v_ball_tangential_i - v_racket_tangential_i) / ball.radius;

        // Apply the final velocities to the ball
        ball.LinearVelocity = v_ball_f;
        ball.AngularVelocity = w_ball_f;
    }

    private Vector3 GetContactNormal()
    {
        return (this.Forward * Vector3.Dot(this.Forward, TennisManager.Instance.RelativeVelocity)).normalized;
    }

    private Vector3 GetVelocityAtContactPoint()
    {
        Vector3 relativePosition = Vector3.ProjectOnPlane(TennisManager.Instance.RelativePosition, this.Forward);
        Vector3 tangentialVelocity = Vector3.Cross(_angularVelocity, relativePosition);

        return _linearVelocity + tangentialVelocity;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(this.Position, this.Position + _contactNormal * 0.5f);

        Gizmos.color = Color.magenta;

        Gizmos.DrawLine(this.Position, this.Position + _hitLinearVelocity * 0.025f);

        if (TennisManager.Instance.Ball != null)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(this.Position, TennisManager.Instance.Ball.Position);
        }

        Gizmos.color = Color.blue;

        Gizmos.DrawLine(this.Position, this.Position + this.Forward * .25f);

        if (TennisManager.Instance.Ball != null)
        {
            Gizmos.color = Color.gray;

            Gizmos.DrawLine(this.Position, this.Position + TennisManager.Instance.RelativeVelocity.normalized * 0.5f);
        }
    }
}
