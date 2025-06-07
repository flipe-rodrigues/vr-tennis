using UnityEngine;

public class TennisBallPhysics : MonoBehaviour
{
    [Header("Ball Properties")]
    public float linearDampingCoefficient = 0.508f;
    public float airDensity = 1.225f; // kg/m³ at sea level

    [Header("Court Properties")]
    public float courtFriction = 0.6f;
    public float courtRestitution = 0.73f; // Bounce coefficient
    public LayerMask courtLayer;

    [Header("Spin Settings")]
    public float spinDecayRate = 0.98f;
    public float magnusForceMultiplier = 0.1f;
    [ColorUsage(true, true)]
    public Color topSpinColor = Color.red;
    [ColorUsage(true, true)]
    public Color backSpinColor = Color.cyan;

    [Header("Audio Settings")]
    public AudioClip[] bounceClips;

    // Private fields
    private Transform _transform; 
    private Rigidbody _rigidbody;
    private MeshRenderer _meshRenderer;
    private AudioSource _audioSource; 
    public Vector3 spin; // Angular velocity for spin (x=sidespin, y=topspin/backspin, z=roll)
    private Vector3 _lastVelocity;
    private float _radius;
    private float _crossSectionalArea;
    [SerializeField]
    private bool _isInAir = true;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _rigidbody = GetComponent<Rigidbody>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        _radius = _transform.localScale.magnitude / 2f;
        _crossSectionalArea = Mathf.PI * _radius * _radius;

        _lastVelocity = _rigidbody.linearVelocity;
    }

    void FixedUpdate()
    {
        ApplyAirResistance();
        ApplyMagnusForce();
        UpdateSpinDecay();

        _lastVelocity = _rigidbody.linearVelocity;

        _transform.Rotate(spin * Time.fixedDeltaTime * Mathf.Rad2Deg);
    }

    //private void LateUpdate()
    //{
    //    Color spinColor = spin.z > 0 ? topSpinColor : backSpinColor;

    //    Color targetColor = Color.Lerp(Color.white, spinColor, Mathf.Abs(spin.z));

    //    _meshRenderer.material.color = targetColor;

    //    _transform.Rotate(spin * Time.fixedDeltaTime * Mathf.Rad2Deg);
    //}

    void ApplyAirResistance()
    {
        // Calculate drag force: F = 0.5 * ρ * v² * Cd * A
        float dragMagnitude = 0.5f * airDensity * _rigidbody.linearVelocity.sqrMagnitude * linearDampingCoefficient * _crossSectionalArea;

        Vector3 dragForce = -_rigidbody.linearVelocity.normalized * dragMagnitude;

        _rigidbody.AddForce(dragForce);
    }

    void ApplyMagnusForce()
    {
        // Magnus force = k × (ω × v) where ω is angular velocity, v is linear velocity
        Vector3 magnusForce = Vector3.Cross(spin, _rigidbody.linearVelocity) * magnusForceMultiplier * _rigidbody.mass;

        _rigidbody.AddForce(magnusForce);
    }

    void UpdateSpinDecay()
    {
        // Spin naturally decays over time
        spin *= Mathf.Pow(spinDecayRate, Time.fixedDeltaTime);
    }

    public void ApplyHit(Vector3 velocity, Vector3 appliedSpin, Vector3 contactPoint)
    {
        _rigidbody.linearVelocity = velocity;
        spin = appliedSpin;
        _isInAir = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & courtLayer) != 0)
        {
            _isInAir = false;

            HandleCourtBounce(collision);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        _isInAir = true;
    }

    void HandleCourtBounce(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;
        Vector3 incomingVelocity = _lastVelocity;

        // Calculate bounce velocity with spin effects
        Vector3 bounceVelocity = CalculateBounceWithSpin(incomingVelocity, normal);
        _rigidbody.linearVelocity = bounceVelocity;

        // Update spin after bounce
        UpdateSpinAfterBounce(normal);

        // Play bounce sound
        PlayBounceSound(incomingVelocity.magnitude);
    }

    Vector3 CalculateBounceWithSpin(Vector3 incomingVel, Vector3 normal)
    {
        // Separate velocity into normal and tangential components
        Vector3 normalVel = Vector3.Project(incomingVel, normal);
        Vector3 tangentialVel = incomingVel - normalVel;

        // Apply restitution to normal component
        Vector3 bounceNormal = -normalVel * courtRestitution;

        // Apply friction and spin effects to tangential component
        Vector3 bounceTangential = tangentialVel * (1f - courtFriction);

        // Topspin increases forward bounce, backspin decreases it
        float spinEffect = Vector3.Dot(spin, Vector3.Cross(normal, tangentialVel.normalized));
        bounceTangential += tangentialVel.normalized * spinEffect * 0.3f;

        // Modify bounce height based on spin
        //float topspinComponent = Vector3.Dot(_spin, Vector3.Cross(normal, Vector3.right));
        //bounceNormal *= (1f + topspinComponent * 0.2f);

        return bounceNormal + bounceTangential;
    }

    void UpdateSpinAfterBounce(Vector3 normal)
    {
        // Spin is reduced and modified during bounce
        spin *= Mathf.Pow(0.8f, Time.fixedDeltaTime); // General spin reduction

        // Convert some spin to linear velocity (realistic tennis ball behavior)
        Vector3 spinToVelocity = Vector3.Cross(spin, normal) * 0.1f;
        _rigidbody.linearVelocity += spinToVelocity;
    }

    void PlayBounceSound(float impactSpeed)
    {
        if (bounceClips.Length > 0 && _audioSource != null)
        {
            // Choose sound based on impact speed
            int clipIndex = Mathf.Clamp(Mathf.FloorToInt(impactSpeed / 10f), 0, bounceClips.Length - 1);
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.volume = Mathf.Clamp01(impactSpeed / 20f);
            _audioSource.PlayOneShot(bounceClips[clipIndex]);
        }
    }

    // Public methods for external access
    public Vector3 GetSpin() => spin;
    public bool IsInAir() => _isInAir;
    public float GetSpeed() => _rigidbody.linearVelocity.magnitude;

    // Method to predict ball trajectory (useful for AI or UI)
    public Vector3[] PredictTrajectory(int steps, float timeStep)
    {
        Vector3[] trajectory = new Vector3[steps];
        Vector3 pos = _transform.position;
        Vector3 vel = _rigidbody.linearVelocity;
        Vector3 currentSpin = spin;

        for (int i = 0; i < steps; i++)
        {
            trajectory[i] = pos;

            // Apply forces for prediction
            Vector3 gravity = Physics.gravity * timeStep;
            Vector3 drag = -vel.normalized * (0.5f * airDensity * vel.sqrMagnitude * linearDampingCoefficient * Mathf.PI * _radius * _radius / _rigidbody.mass) * timeStep;
            Vector3 magnus = Vector3.Cross(currentSpin, vel) * magnusForceMultiplier * timeStep;

            vel += gravity + drag + magnus;
            pos += vel * timeStep;
            currentSpin *= Mathf.Pow(spinDecayRate, timeStep / Time.fixedDeltaTime);
        }

        return trajectory;
    }
}