using UnityEngine;

public class MeshOverlapMontecarloBhv : MonoBehaviour
{
    // Public fields
    public Collider meshA;
    public Collider meshB;
    public int sampleCount = 10000;

    // Readonly fields
    [SerializeField, ReadOnly, Range(0, 1)]
    private float _overlapFraction;

    private void Update()
    {
        _overlapFraction = EstimateOverlapFraction(meshA, meshB, sampleCount);
    }

    private float EstimateOverlapFraction(Collider a, Collider b, int samples)
    {
        Bounds bounds = a.bounds;
        float insideBoth = 0f;
        float insideEither = 0f;

        for (int i = 0; i < samples; i++)
        {
            // Random point in local bounds of A
            Vector3 localPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );

            // Transform into world space of A
            Vector3 worldPoint = a.transform.TransformPoint(localPoint);

            bool inA = IsInsideCollider(a, worldPoint);
            bool inB = IsInsideCollider(b, worldPoint);

            if (inA || inB) insideEither++;
            if (inA && inB) insideBoth++;
        }

        return insideBoth / Mathf.Max(1, insideEither);
    }

    private bool IsInsideCollider(Collider col, Vector3 point)
    {
        Vector3 closest = col.ClosestPoint(point);
        return Vector3.Distance(closest, point) < 1e-6f; // inside if no offset
    }
}
