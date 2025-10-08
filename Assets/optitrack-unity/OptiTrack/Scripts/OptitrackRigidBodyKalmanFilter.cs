using System;
using UnityEngine;

public class OptitrackRigidBodyKalmanFilter : OptitrackRigidBody
{
    // Public properties
    public Vector3 FilteredPosition => _filteredPosition;
    public Vector3 FilteredLinearVelocity => _filteredLinearVelocity;

    // Public fields
    [Header("Kalman Filter Settings:")]
    [Range(0f, 1f)] 
    public float alphaPosition = 0.5f;
    [Range(0f, 1f)] 
    public float betaPosition = 0.05f;
    [Range(0f, 1f)] 
    public float alphaRotation = 0.5f;
    [Range(0f, 1f)] 
    public float betaRotation = 0.05f;
    public bool applyPositionToTransform = false;
    public bool applyRotationToTransform = false;

    // Readonly fields
    [SerializeField, ReadOnly]
    private Vector3 _filteredPosition;
    [SerializeField, ReadOnly]
    private Vector3 _filteredLinearVelocity;
    [SerializeField, ReadOnly]
    private Vector3 _filteredAngularVelocity;
    [SerializeField, ReadOnly]
    private Quaternion _filteredRotation;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();


        //this.UpdateRotation(measuredRotation);

        //// --- ROTATION α-β FILTER ---
        //float dt = Time.fixedDeltaTime;

        //// Predict orientation forward by integrating angular velocity
        //float predictedDeltaAngle = _angularVelocity.magnitude * dt * Mathf.Rad2Deg;
        //Quaternion predictedDeltaRotation = Quaternion.identity;
        //if (_angularVelocity.sqrMagnitude > 0f)
        //{
        //    predictedDeltaRotation = Quaternion.AngleAxis(predictedDeltaAngle, _angularVelocity.normalized);
        //}
        //Quaternion predictedRotation = predictedDeltaRotation * _rotation;

        //// Compute residual between predicted and measured rotations
        //Quaternion residualRotation = measuredRotation * Quaternion.Inverse(predictedRotation);
        //residualRotation.ToAngleAxis(out float residualAngleInDegrees, out Vector3 axis);

        //// Make sure axis is valid
        //if (axis.sqrMagnitude > 0f)
        //{
        //    if (residualAngleInDegrees > 180f) residualAngleInDegrees -= 360f;
        //    Vector3 residualAngularVelocity = axis.normalized * (residualAngleInDegrees * Mathf.Deg2Rad) / Time.fixedDeltaTime;
        //    float residualDeltaAngle = residualAngularVelocity.magnitude * dt * Mathf.Rad2Deg;

        //    // Correct predicted rotation and angular velocity using residual
        //    Quaternion correctionDeltaRotation = Quaternion.AngleAxis(alphaRotation * residualDeltaAngle, residualAngularVelocity.normalized);
        //    _rotation = predictedRotation * correctionDeltaRotation;
        //    _angularVelocity += betaRotation * residualAngularVelocity / dt;
        //}
        //else
        //{
        //    // No residual, just use prediction
        //    _rotation = predictedRotation;
        //}

        // --- ROTATION α-β FILTER ---
        //float dt = Time.fixedDeltaTime;

        //// Predict orientation forward by integrating angular velocity
        //float predictedDeltaAngle = _angularVelocity.magnitude * dt * Mathf.Rad2Deg;
        //Quaternion predictedDeltaRotation = Quaternion.AngleAxis(predictedDeltaAngle, _angularVelocity.normalized);
        //Quaternion predictedRotation = predictedDeltaRotation * _rotation;

        //// Compute residual between predicted and measured rotations
        //Quaternion residualRotation = measuredRotation * Quaternion.Inverse(predictedRotation);
        //residualRotation.ToAngleAxis(out float residualAngleInDegrees, out Vector3 axis);
        //if (residualAngleInDegrees > 180f) residualAngleInDegrees -= 360f;
        //Vector3 residualAngularVelocity = axis * (residualAngleInDegrees * Mathf.Deg2Rad) / Time.fixedDeltaTime;
        //float residualDeltaAngle = residualAngularVelocity.magnitude * dt * Mathf.Rad2Deg;

        //// Correct predicted rotation and angular velocity using residual
        //Quaternion correctionDeltaRotation = Quaternion.AngleAxis(alphaRotation * residualDeltaAngle, residualAngularVelocity.normalized);
        //_rotation = predictedRotation * correctionDeltaRotation;
        //_angularVelocity += betaRotation * residualAngularVelocity / dt;
    }

    private void UpdateFilteredPositionAndLinearVelocity()
    {
        float dt = Time.fixedDeltaTime;

        Vector3 predictedPosition = _filteredPosition + _filteredLinearVelocity * dt;
        Vector3 residualPosition = base.CurrentPosition - predictedPosition;
        _filteredPosition = predictedPosition + alphaPosition * residualPosition;

        Vector3 predictedLinearVelocity = _filteredLinearVelocity;
        _filteredLinearVelocity = predictedLinearVelocity + betaPosition * residualPosition / dt;
    }
}
