using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class RacketColliderBhv : MonoBehaviour
{
    // Public properties
    public Vector3 ContactNormal => _contactNormal;

    // Public fields
    [Header("Bounce Settings:")]
    public float apparentNormalRestitution = .4f;
    public float apparentTangentialRestitution = .65f;
    public float apparentSpinRestitution = .4f;
    public float spinToTangentialConversion = .3f;
    public float tangentialToSpinConversion = .58f;
    public UnityEvent<float> onRacketBounce = new UnityEvent<float>();
    [Header("Refractory Period Settings:")]
    [Min(0f)]
    public float refractoryPeriod = 0.05f;
    [Header("Dynamic Rescaling Settings:")]
    public Vector3 scaleModifier = Vector3.one;
    [Range(1, 10)]
    public float maxScaleFactor = 5f;
    public bool rescaleDynamically;
    [Header("Debugging:")]
    public bool displayAsMesh;

    // Private fields
    private RacketBhv _racketBhv;
    private Transform _transform;
    private BoxCollider _collider;
    private MeshRenderer _meshRenderer;
    private Transform _meshTransform;
    private Vector3 _contactNormal;
    private Vector3 _defaultScale;

    private void OnValidate()
    {
        this.GetComponentInChildren<MeshRenderer>().enabled = displayAsMesh;
    }

    private void Awake()
    {
        _racketBhv = this.GetComponentInParent<RacketBhv>();
        _transform = this.GetComponent<Transform>();
        _collider = this.GetComponent<BoxCollider>();
        _meshRenderer = this.GetComponentInChildren<MeshRenderer>();
        _meshTransform = _meshRenderer.GetComponent<Transform>();
    }

    private void Start()
    {
        _defaultScale = _collider.size;
    }

    private void FixedUpdate()
    {
        if (rescaleDynamically)
        {
            this.RescaleColliderDynamically();
        }
    }

    private void RescaleColliderDynamically()
    {
        Vector3 localVelocity = _transform.InverseTransformDirection(_racketBhv.LinearVelocity);
        Vector3 deltaScale = localVelocity.ElementWiseMultiplication(scaleModifier).Abs();
        _collider.size = (_defaultScale + deltaScale).ClampBetween(_defaultScale, _defaultScale * maxScaleFactor);
        _meshTransform.localScale = _collider.size;
    }

    private void OnTriggerEnter(Collider other)
    {
        this.HandleImpendingBounce();
    }

    private void OnTriggerStay(Collider other)
    {
        this.HandleImpendingBounce();
    }

    private void OnTriggerExit(Collider other)
    {
        this.HandleImpendingBounce();
    }

    private void HandleImpendingBounce()
    {
        this.UpdateContactNormal();

        if (Vector3.Dot(_contactNormal, TennisManager.Instance.RelativePosition) < 0)
        {
            this.StartRefractoryPeriod();
            this.Bounce(TennisManager.Instance.Ball);
            onRacketBounce?.Invoke(TennisManager.Instance.RelativeVelocity.magnitude);
            TennisManager.Instance.Ball.WasJustHit = true;
            TrackingManager.Instance.RecordTaskEvent(TaskEventType.RacketHit);
        }
    }

    private void UpdateContactNormal()
    {
        _contactNormal = (_racketBhv.Forward * Vector3.Dot(_racketBhv.Forward, TennisManager.Instance.RelativeVelocity)).normalized;
    }

    private void Bounce(BallBhv ball)
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

    private Vector3 GetVelocityAtContactPoint()
    {
        Vector3 relativePosition = Vector3.ProjectOnPlane(TennisManager.Instance.RelativePosition, _racketBhv.Forward);
        Vector3 tangentialVelocity = Vector3.Cross(_racketBhv.AngularVelocity, relativePosition);

        return _racketBhv.LinearVelocity + tangentialVelocity;
    }

    private void StartRefractoryPeriod()
    {
        StartCoroutine(this.RefractoryPeriodCoroutine());
    }

    private IEnumerator RefractoryPeriodCoroutine()
    {
        _meshRenderer.enabled = false;
        _collider.enabled = false;

        float timer = 0f;
        while (timer < refractoryPeriod)
        {
            timer += Time.fixedDeltaTime;
            yield return ApplicationManager.waitForFixedUpdateInstance;
        }

        _meshRenderer.enabled = displayAsMesh;
        _collider.enabled = true;
    }
}
