using UnityEngine;

public class MeshOverlapBhv : MonoBehaviour
{
    // Public fields
    public MeshRenderer meshRendererA;
    public MeshRenderer meshRendererB;

    // Readonly fields
    [SerializeField, ReadOnly, Range(0f, 1f)]
    private float _overlapFraction;

    private void Update()
    {
        _overlapFraction = OverlapFraction(meshRendererA, meshRendererB);
    }

    private float GetBoundsOverlapVolume(Bounds a, Bounds b)
    {
        if (!a.Intersects(b)) return 0f;
        float x = Mathf.Max(0, Mathf.Min(a.max.x, b.max.x) - Mathf.Max(a.min.x, b.min.x));
        float y = Mathf.Max(0, Mathf.Min(a.max.y, b.max.y) - Mathf.Max(a.min.y, b.min.y));
        float z = Mathf.Max(0, Mathf.Min(a.max.z, b.max.z) - Mathf.Max(a.min.z, b.min.z));
        return x * y * z;
    }

    private float OverlapFraction(MeshRenderer a, MeshRenderer b)
    {
        Bounds A = a.bounds;
        Bounds B = b.bounds;

        float overlap = GetBoundsOverlapVolume(A, B);
        float volume = A.size.x * A.size.y * A.size.z;

        return overlap / Mathf.Max(volume, 1e-6f);
    }
}
