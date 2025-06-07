using Oculus.Haptics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO:
// double hits still! how?!
// only play racket sound / haptics if Hit happened!!
// Cleanup after debugging..
// This should be absorved into RacketRigidbodyBhv
// Angular velocity in collider bhv

public class RacketStringsBhv : RacketRigidbodyBhv
{
    // Public properties
    public Vector3 SmoothLinearVelocity => _smoothLinearVelocity;
    public Vector3 SmoothAngularVelocity => _smoothAngularVelocity;

    // Public fields
    public float apparentNormalRestitution = .4f;
    public float apparentTangentialRestitution = .65f;
    public float apparentSpinRestitution = .4f;
    public float spinToTangentialConversion = .3f;
    public float tangentialToSpinConversion = .58f;
    public float refractoryPeriod = 0.05f;
    public float smoothingTimeConstant = 0.005f;

    // Read only fields
    [SerializeField, ReadOnly]
    private float _smoothingRate;

    // Private fields
    private Collider _collider;
    private List<Vector3> _contactPoints = new List<Vector3>();
    private List<Vector3> _contactVelocities = new List<Vector3>();
    private Vector3 _contactNormal;
    private Vector3 _smoothLinearVelocity;
    private Vector3 _smoothAngularVelocity;

    private void OnValidate()
    {
        _smoothingRate = Mathf.Exp(-Time.fixedDeltaTime / smoothingTimeConstant);
    }

    protected override void Awake()
    {
        base.Awake();

        _collider = GetComponentInChildren<Collider>();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        _smoothLinearVelocity = Vector3.Lerp(_smoothLinearVelocity, this.LinearVelocity, _smoothingRate);
        _smoothAngularVelocity = Vector3.Lerp(_smoothAngularVelocity, this.AngularVelocity, _smoothingRate);
    }

    private void OnTriggerEnter(Collider other)
    {
        _contactPoints.Clear();
        _contactVelocities.Clear();

        _contactNormal = (this.Forward * Vector3.Dot(this.Forward, TennisManager.Instance.RelativeVelocity)).normalized;

        this.OnTriggerStay(other);
    }

    private void OnTriggerStay(Collider other)
    {
        Vector3 closestPointOnStrings = this.Position + Vector3.ProjectOnPlane(TennisManager.Instance.RelativePosition, this.Forward);

        _contactPoints.Add(_collider.ClosestPoint(closestPointOnStrings));
        _contactVelocities.Add(this.GetVelocityAtPoint(closestPointOnStrings));

        if (Vector3.Dot(_contactNormal, TennisManager.Instance.RelativePosition) < 0) // TennisManager.Instance.Ball.Radius / 2f)
        {
            StartCoroutine(this.RefractoryPeriodCoroutine());

            this.Hit();

            // Play haptic feedback..
            // Play racket audio..
        }
    }

    private void Hit()
    {
        Vector3 v_racket_i = this.GetVelocityAtContactPoint();
        Vector3 v_ball_i = TennisManager.Instance.Ball.LinearVelocity;
        Vector3 w_ball_i = TennisManager.Instance.Ball.AngularVelocity;

        // Separate initial velocity into normal and tangential components
        Vector3 v_ball_normal_i = Vector3.Project(v_ball_i, _contactNormal);
        Vector3 v_ball_tangential_i = v_ball_i - v_ball_normal_i;

        Vector3 v_racket_normal_i = Vector3.Project(v_racket_i, _contactNormal);
        Vector3 v_racket_tangential_i = v_racket_i - v_racket_normal_i;

        // Apply restitution to normal component
        Vector3 v_ball_normal_f = (1 + apparentNormalRestitution) * v_racket_normal_i + apparentNormalRestitution * -v_ball_normal_i;

        // Apply friction and spin effects to tangential component
        Vector3 v_ball_tangential_f = apparentTangentialRestitution * (v_ball_tangential_i + v_racket_tangential_i) +
            spinToTangentialConversion * TennisManager.Instance.Ball.Radius * Vector3.Cross(w_ball_i, _contactNormal);

        // Calculate final velocity
        Vector3 v_ball_f = v_ball_normal_f + v_ball_tangential_f;
        Vector3 v_ball_y_f = Vector3.Project(v_ball_f, Vector3.up);
        Vector3 v_ball_x_f = v_ball_f - v_ball_y_f;
        v_ball_f = v_ball_y_f + v_ball_x_f;

        // Calculate the final angular velocity of the ball
        Vector3 w_ball_f = apparentSpinRestitution * w_ball_i +
            tangentialToSpinConversion * Vector3.Cross(_contactNormal, v_ball_tangential_i - v_racket_tangential_i) / TennisManager.Instance.Ball.Radius;

        // Apply the final velocities to the ball
        TennisManager.Instance.Ball.LinearVelocity = v_ball_f;
        TennisManager.Instance.Ball.AngularVelocity = w_ball_f;
    }

    private Vector3 GetVelocityAtPoint(Vector3 pointOnStrings)
    {
        Vector3 relativePosition = pointOnStrings - this.Position;
        Vector3 tangentialVelocity = Vector3.Cross(this.AngularVelocity, relativePosition);

        return this.LinearVelocity + tangentialVelocity;
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

        Gizmos.DrawLine(this.Position, this.Position + _contactNormal * 0.5f);

        Gizmos.color = Color.green;

        Gizmos.DrawLine(this.Position, this.Position + _contactNormal * 0.25f);

        Gizmos.color = Color.magenta;

        Gizmos.DrawLine(this.Position, this.Position + _smoothLinearVelocity * 0.01f);

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

        int index = 0;
        foreach (var contactPoint in _contactPoints)
        {
            Gizmos.color = Color.Lerp(Color.white, Color.black, (float)index / (_contactPoints.Count - 1f));
            Gizmos.DrawSphere(contactPoint, .025f);
            Gizmos.DrawLine(contactPoint, contactPoint + _contactVelocities[index] * 0.01f);
            index++;
        }
    }

    public IEnumerator RefractoryPeriodCoroutine()
    {
        _collider.enabled = false;

        yield return new WaitForSeconds(refractoryPeriod);

        _collider.enabled = true;
    }
}
