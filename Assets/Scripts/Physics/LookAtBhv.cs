using UnityEngine;

[ExecuteInEditMode]
public class LookAtBhv : MonoBehaviour
{
    private Transform Transform => _transform == null ? this.GetComponent<Transform>() : _transform;

    public Transform target;

    private Transform _transform;

    private void Awake()
    {
        _transform = this.GetComponent<Transform>();
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        this.Transform.LookAt(target.position, Vector3.up);
    }
}
