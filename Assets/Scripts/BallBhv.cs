using UnityEngine;

public class BallBhv : CachedRigidbodyBhvOLD
{
    public float verticalCoefficientOfRestitution;

    private float _verticalVelocityBefore;
    private float _verticalVelocityAfter;

    private void OnCollisionEnter(Collision collision)
    {
        _verticalVelocityBefore = LinearVelocity.y;
    }

    private void OnTriggerEnter(Collider other)
    {
        _verticalVelocityBefore = LinearVelocity.y;
    }

    private void OnCollisionExit(Collision collision)
    {
        this.UpdateRestitution();
    }

    private void OnTriggerExit(Collider other)
    {
        this.UpdateRestitution();
    }

    private void UpdateRestitution()
    {
        _verticalVelocityAfter = LinearVelocity.y;

        verticalCoefficientOfRestitution = Mathf.Abs(_verticalVelocityAfter / _verticalVelocityBefore);

        Debug.Log($"e_y: {verticalCoefficientOfRestitution}, time: {Time.time}, timescale {Time.timeScale}");
    }
}
