using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.InputSystem.XR;

// TODO:
// Cleanup after debugging..
// Angular velocity in collider bhv

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class RacketRigidbodyBhv : CachedRigidbodyBhv
{
    // Public properties
    public Vector3 HitVelocity => _hitVelocity;
    public Vector3 HitContactNormal => _hitContactNormal;
    public Vector3 SmoothLinearVelocity => _smoothLinearVelocity;
    public Vector3 SmoothAngularVelocity => _smoothAngularVelocity;
    public bool IsInRefractoryPeriod => _racketCollider.enabled == false;

    // Public fields
    public XRController inputController;
    public RacketAnchorBhv anchorTransform;
    public float apparentNormalRestitution = .4f;
    public float apparentTangentialRestitution = .65f;
    public float apparentSpinRestitution = .4f;
    public float spinToTangentialConversion = .3f;
    public float tangentialToSpinConversion = .58f;
    [Min(0f)]
    public float refractoryPeriod = 0.05f;
    [Min(0.001f)]
    public float smoothingTimeConstant = 0.01f;
    public UnityEvent<float> onRacketHit = new UnityEvent<float>();

    // Read only fields
    [SerializeField, ReadOnly]
    private float _smoothingRate;
    [SerializeField, ReadOnly]
    private int _approxNumFrames;

    // Private fields
    private RacketColliderBhv _racketCollider;
    private Vector3 _smoothContactNormal;
    private Vector3 _smoothLinearVelocity;
    private Vector3 _smoothAngularVelocity;
    private Vector3 _hitVelocity;
    private Vector3 _hitContactNormal;

    private void OnValidate()
    {
        if (anchorTransform != null && anchorTransform.anchoredRigidbody != this)
        {
            anchorTransform.anchoredRigidbody = this;
        }

        _smoothingRate = smoothingTimeConstant.TauToLambda();
        _approxNumFrames = Mathf.RoundToInt(smoothingTimeConstant / Time.fixedDeltaTime);
    }

    protected override void Awake()
    {
        base.Awake();

        _racketCollider = GetComponentInChildren<RacketColliderBhv>();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            this.MoveTransform();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        this.MoveRigidbody();

        _smoothContactNormal = Vector3.Lerp(_smoothContactNormal, this.GetContactNormal(), _smoothingRate);
        _smoothLinearVelocity = Vector3.Lerp(_smoothLinearVelocity, this.LinearVelocity, _smoothingRate);
        _smoothAngularVelocity = Vector3.Lerp(_smoothAngularVelocity, this.AngularVelocity, _smoothingRate);
    }

    public void MoveTransform()
    {
        if (anchorTransform == null)
        {
            return;
        }

        this.Position = anchorTransform.Position;
        this.Rotation = anchorTransform.Rotation;
    }

    private void MoveRigidbody()
    {
        if (anchorTransform == null)
        {
            return;
        }

        this.Move(anchorTransform.Position, anchorTransform.Rotation);
    }
    
    private void OnTriggerStay(Collider other)
    {
        Vector3 closestPointOnStrings = this.Position + Vector3.ProjectOnPlane(TennisManager.Instance.RelativePosition, this.Forward);

        if (Vector3.Dot(_smoothContactNormal, TennisManager.Instance.RelativePosition) < 0)
        {
            _racketCollider.StartRefractoryPeriod();

            float relativeSpeed = (_smoothLinearVelocity - TennisManager.Instance.Ball.LinearVelocity).magnitude;

            _hitVelocity = this.GetVelocityAtContactPoint();
            _hitContactNormal = _smoothContactNormal;

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
        Vector3 v_ball_normal_i = Vector3.Project(v_ball_i, _smoothContactNormal);
        Vector3 v_ball_tangential_i = v_ball_i - v_ball_normal_i;

        Vector3 v_racket_normal_i = Vector3.Project(v_racket_i, _smoothContactNormal);
        Vector3 v_racket_tangential_i = v_racket_i - v_racket_normal_i;

        // Apply restitution to normal component
        Vector3 v_ball_normal_f = 
            (1 + apparentNormalRestitution) * v_racket_normal_i + apparentNormalRestitution * -v_ball_normal_i;

        // Apply friction and spin effects to tangential component
        Vector3 v_ball_tangential_f = 
            apparentTangentialRestitution * (v_racket_tangential_i + v_ball_tangential_i) +
            spinToTangentialConversion * ball.Radius * Vector3.Cross(w_ball_i, _smoothContactNormal);

        // Calculate final velocity
        Vector3 v_ball_f = v_ball_normal_f + v_ball_tangential_f;
        Vector3 v_ball_y_f = Vector3.Project(v_ball_f, Vector3.up);
        Vector3 v_ball_x_f = v_ball_f - v_ball_y_f;
        v_ball_f = v_ball_y_f + v_ball_x_f;

        // Calculate the final angular velocity of the ball
        Vector3 w_ball_f = 
            apparentSpinRestitution * w_ball_i +
            tangentialToSpinConversion * Vector3.Cross(_smoothContactNormal, v_ball_tangential_i - v_racket_tangential_i) / ball.Radius;

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
        // can be simplified !!!!!!!!!!!!!!!!!! (adding and subtracting...)
        Vector3 contactPoint = this.Position + Vector3.ProjectOnPlane(TennisManager.Instance.RelativePosition, this.Forward);
        Vector3 relativePosition = contactPoint - this.Position;
        Vector3 tangentialVelocity = Vector3.Cross(_smoothAngularVelocity, relativePosition);

        return _smoothLinearVelocity + tangentialVelocity;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(this.Position, this.Position + _smoothContactNormal * 0.5f);

        Gizmos.color = Color.magenta;

        Gizmos.DrawLine(this.Position, this.Position + _hitVelocity * 0.025f);

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
