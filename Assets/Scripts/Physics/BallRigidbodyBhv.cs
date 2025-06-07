using System.Collections;
using UnityEngine;

public class BallRigidbodyBhv : CachedRigidbodyBhv
{
    // Public properties
    public float Radius => _radius;

    // Public fields
    public float airDensity = 1.225f; // kg/m³ at sea level

    // Read only fields
    [SerializeField, ReadOnly]
    private float _dragCoefficient;
    [SerializeField, ReadOnly]
    private float _liftCoefficient;
    [SerializeField, ReadOnly]
    private float _spinDecayRate;

    // Private fields
    private float _radius;
    private float _crossSectionalArea;
    private float _V;
    private float _W;

    protected override void Start()
    {
        base.Start();

        _radius = transform.localScale.magnitude / 2f;
        _crossSectionalArea = Mathf.PI * _radius * _radius;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        _V = this.LinearVelocity.magnitude;
        _W = this.AngularVelocity.magnitude;

        UpdateDrag();
        UpdateLift();
        UpdateBuoyancy();
        UpdateSpin();
    }

    private void UpdateDrag()
    {
        if (_V == 0)
        {
            return;
        }

        // From Robinson & Robinson 2018
        _dragCoefficient = 0.6204f - 9.76e-4f * (_V - 50) + (1.027e-4f - 2.24e-6f * (_V - 50)) * _W;

        // Calculate drag force: Fd = (1/2) * ρ * A * Cd * V * v
        Vector3 dragForce = -0.5f * airDensity * _crossSectionalArea * _dragCoefficient * _V * this.LinearVelocity;

        this.AddForce(dragForce);
    }

    private void UpdateLift()
    {
        if (_W == 0)
        {
            return;
        }

        // From Robinson & Robinson 2018
        _liftCoefficient = (4.68e-4f - 2.0984e-5f * (_V - 50)) * _W;

        // Calculate lift force: Fl = (1/2) * ρ * A * Cl * V * (w x v) / W
        Vector3 liftForce = 0.5f * airDensity * _crossSectionalArea * _liftCoefficient * _V * Vector3.Cross(this.AngularVelocity, this.LinearVelocity) / _W;

        this.AddForce(liftForce);
    }

    private void UpdateBuoyancy()
    {
        // Calculate buoyancy force: Fb = (4/3) * π * r³ * ρ * g
        Vector3 buoyancyForce = 4 / 3 * Mathf.PI * Mathf.Pow(_radius, 3) * airDensity * Physics.gravity;

        this.AddForce(buoyancyForce);
    }

    private void UpdateSpin()
    {
        if (_V == 0)
        {
            return;
        }

        // From Robinson & Robinson 2018
        _spinDecayRate = Mathf.Exp(-Time.fixedDeltaTime / (164 / _V));

        this.AngularVelocity *= _spinDecayRate;
    }
}
