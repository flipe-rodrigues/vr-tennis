using UnityEngine;

public class PhysicalBallBhv : MonoBehaviour
{
    [Header("Ball Properties")]
    public float dragCoefficient = 0.508f;
    public float airDensity = 1.225f; // kg/m³ at sea level

    [Header("Hard Court Properties")]
    public float courtFriction = 0.6f;
    public float courtRestitution = 0.73f; // Bounce coefficient
    public LayerMask courtLayer = 1;

    [Header("Spin Settings")]
    public float spinDecayRate = 0.98f;
    public float magnusForceMultiplier = 0.1f;

    [Header("Audio Settings")]
    public AudioClip[] bounceClips;

    // Private fields
    private Transform _transform;
    private Rigidbody _rigidbody;
    private AudioSource _audioSource;
    private Vector3 _lastVelocity;
    private float _scaledSpinDecayRate;
    private float _radius;
    private float _crossSectionalArea;
    private bool _isInAir = true;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _rigidbody = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _scaledSpinDecayRate = Mathf.Pow(spinDecayRate, Time.fixedDeltaTime);

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
    }

    void ApplyAirResistance()
    {
        // Calculate drag force: F = 0.5 * ρ * v² * Cd * A
        float dragMagnitude = 0.5f * airDensity * _rigidbody.linearVelocity.sqrMagnitude * dragCoefficient * _crossSectionalArea;
        Vector3 dragForce = -_rigidbody.linearVelocity.normalized * dragMagnitude;
        _rigidbody.AddForce(dragForce);
    }

    void ApplyMagnusForce()
    {
        // Magnus force = k × (ω × v) where ω is angular velocity, v is linear velocity
        Vector3 magnusForce = Vector3.Cross(_rigidbody.angularVelocity, _rigidbody.linearVelocity) * magnusForceMultiplier * _rigidbody.mass;
        _rigidbody.AddForce(magnusForce);
    }

    void UpdateSpinDecay()
    {
        // Spin naturally decays over time due to air resistance
        _rigidbody.angularVelocity *= _scaledSpinDecayRate;
    }

    public void ApplyHit(Vector3 velocity, Vector3 appliedSpin, Vector3 contactPoint)
    {
        _rigidbody.linearVelocity = velocity;
        _rigidbody.angularVelocity = appliedSpin;
        _isInAir = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & courtLayer) != 0)
        {
            HandleCourtBounce(collision);
        }
    }

    void HandleCourtBounce(Collision collision)
    {
        _isInAir = false;

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
        return CalculateRealisticBounceVelocity(incomingVel, normal, courtRestitution, courtFriction, _rigidbody.angularVelocity);
    }

    Vector3 CalculateRealisticBounceVelocity(Vector3 velocity, Vector3 normal, float restitution, float friction, Vector3 angularVelocity)
    {
        // Step 1: Separate incoming velocity into normal and tangential components
        Vector3 normalVelocity = Vector3.Project(velocity, normal);
        Vector3 tangentialVelocity = velocity - normalVelocity;

        // Step 2: Apply coefficient of restitution to normal component
        // This determines how much the ball bounces back
        Vector3 finalNormalVelocity = -normalVelocity * restitution;

        // Step 3: Apply friction to tangential component
        // Surface friction reduces sliding motion
        Vector3 finalTangentialVelocity = tangentialVelocity * (1f - friction);

        // Step 4: Calculate spin effects on bounce (Magnus effect during contact)
        Vector3 tangentDirection = tangentialVelocity.normalized;

        // Topspin component: how much the ball is spinning forward/backward
        float topspinComponent = Vector3.Dot(angularVelocity, Vector3.Cross(normal, tangentDirection));

        // Sidespin component: how much the ball is spinning left/right
        float sidespinComponent = Vector3.Dot(angularVelocity, Vector3.up);

        // Step 5: Apply topspin/backspin effects
        // Topspin makes the ball grab the surface and accelerate forward
        // Backspin makes the ball slip and can even reverse direction
        float spinTransferEfficiency = friction * 0.35f; // 35% of spin transfers to linear motion
        finalTangentialVelocity += tangentDirection * topspinComponent * spinTransferEfficiency;

        // Step 6: Apply sidespin effects (lateral deflection)
        Vector3 sideDirection = Vector3.Cross(normal, tangentDirection).normalized;
        finalTangentialVelocity += sideDirection * sidespinComponent * friction * 0.25f;

        // Step 7: Spin affects bounce height
        // Topspin reduces bounce height, backspin increases it
        float heightModifier = 1f + (topspinComponent * 0.15f);
        finalNormalVelocity *= Mathf.Clamp(heightModifier, 0.7f, 1.3f);

        // Step 8: Ball deformation effects
        // Faster impacts compress the ball more, affecting the bounce
        float impactSpeed = velocity.magnitude;
        float deformationEffect = CalculateBallDeformation(impactSpeed);
        //finalNormalVelocity *= deformationEffect;

        return finalNormalVelocity + finalTangentialVelocity;
    }

    float CalculateBallDeformation(float impactSpeed)
    {
        // Tennis balls deform more at higher impact speeds
        // This reduces the effective coefficient of restitution
        float deformation = Mathf.Clamp01(impactSpeed / 30f); // Normalize to 0-1
        return 1f - (deformation * 0.1f); // Max 10% reduction in bounce
    }

    void UpdateSpinAfterBounce(Vector3 normal)
    {
        Vector3 newAngularVelocity = CalculateSpinAfterBounce(_rigidbody.angularVelocity, _lastVelocity, normal, courtFriction);
        _rigidbody.angularVelocity = newAngularVelocity;
    }

    Vector3 CalculateSpinAfterBounce(Vector3 initialSpin, Vector3 initialVelocity, Vector3 normal, float friction)
    {
        // Step 1: Natural spin decay during bounce
        // Some spin is always lost due to friction and ball deformation
        float spinRetentionFactor = 0.75f + (friction * 0.15f); // Higher friction preserves more spin
        Vector3 retainedSpin = initialSpin * spinRetentionFactor;

        // Step 2: Conversion between linear motion and spin
        Vector3 tangentialVelocity = initialVelocity - Vector3.Project(initialVelocity, normal);

        // Step 3: Skidding creates new spin
        // When the ball slides across the surface, it generates spin
        Vector3 skiddingSpin = Vector3.Cross(tangentialVelocity.normalized, normal) * friction * 0.8f;
        Vector3 finalSpin = retainedSpin + skiddingSpin;

        // Step 4: Opposing spin is reduced more
        // If the ball's spin opposes its motion, friction reduces it more
        Vector3 tangentDirection = tangentialVelocity.normalized;
        float spinMotionAlignment = Vector3.Dot(initialSpin.normalized, tangentDirection);
        if (spinMotionAlignment < -0.5f) // Spin opposes motion
        {
            finalSpin *= 0.9f; // Additional reduction for opposing spin
        }

        // Step 5: Physical limits
        // Spin cannot exceed realistic values for a tennis ball
        finalSpin = Vector3.ClampMagnitude(finalSpin, 25f); // Max ~400 RPM realistic for tennis

        // Step 6: Energy conservation
        // Total rotational energy cannot increase during bounce
        float initialSpinEnergy = initialSpin.sqrMagnitude * 0.5f;
        float maxAllowedSpinEnergy = initialSpinEnergy * 0.8f; // Can lose energy but not gain
        float finalSpinEnergy = finalSpin.sqrMagnitude * 0.5f;

        if (finalSpinEnergy > maxAllowedSpinEnergy)
        {
            float energyRatio = Mathf.Sqrt(maxAllowedSpinEnergy / finalSpinEnergy);
            finalSpin *= energyRatio;
        }

        return finalSpin;
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
    public Vector3 GetSpin() => _rigidbody.angularVelocity;
    public bool IsInAir() => _isInAir;
    public float GetSpeed() => _rigidbody.linearVelocity.magnitude;

    /// <summary>
    /// Predicts the ball's trajectory for visualization or AI purposes
    /// </summary>
    public Vector3[] PredictTrajectory(int steps, float timeStep)
    {
        Vector3[] trajectory = new Vector3[steps];
        Vector3 position = _transform.position;
        Vector3 velocity = _rigidbody.linearVelocity;
        Vector3 angularVelocity = _rigidbody.angularVelocity;

        for (int i = 0; i < steps; i++)
        {
            trajectory[i] = position;

            // Apply gravity
            Vector3 gravity = Physics.gravity * timeStep;

            // Apply air resistance
            float dragMagnitude = 0.5f * airDensity * velocity.sqrMagnitude * dragCoefficient * _crossSectionalArea;
            Vector3 drag = -velocity.normalized * dragMagnitude * timeStep / _rigidbody.mass;

            // Apply Magnus force (spin effect)
            Vector3 magnus = Vector3.Cross(angularVelocity, velocity) * magnusForceMultiplier * timeStep;

            // Update velocity and position
            velocity += gravity + drag + magnus;
            position += velocity * timeStep;

            // Spin decays over time
            angularVelocity *= Mathf.Pow(spinDecayRate, timeStep / Time.fixedDeltaTime);
        }

        return trajectory;
    }
}