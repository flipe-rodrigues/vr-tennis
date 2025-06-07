using UnityEngine;

public class ColorByVelocityBhv : MonoBehaviour
{
    public VelocityType velocityType = VelocityType.Linear;

    public Gradient linearColorGradient;
    public Gradient angularColorGradient;
    public float minVelocity = -1f;
    public float maxVelocity = 1f;

    private CachedRigidbodyBhv _rigidbody;
    private Renderer _renderer;

    private void Awake()
    {
        _rigidbody = GetComponentInParent<KinematicRigidbodyBhv>();
        _renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (_rigidbody != null)
        {
            if (velocityType == VelocityType.Linear)
            {
                float linearVelocity = Vector3.Dot(_rigidbody.LinearVelocity, _rigidbody.Forward);
                float linearT = Mathf.InverseLerp(minVelocity, maxVelocity, linearVelocity);
                Color linearColor = linearColorGradient.Evaluate(linearT);
                _renderer.material.color = linearColor;
            }
            else if (velocityType == VelocityType.Angular)
            {
                float angularVelocity = Vector3.Dot(_rigidbody.AngularVelocity, Vector3.up);
                float angularT = Mathf.InverseLerp(minVelocity, maxVelocity, angularVelocity);
                Color angularColor = angularColorGradient.Evaluate(angularT);
                _renderer.material.color = angularColor;
            }
        }
    }
}

public enum VelocityType
{
    Linear,
    Angular
}
