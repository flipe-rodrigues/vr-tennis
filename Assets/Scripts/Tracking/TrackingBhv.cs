using UnityEngine;
using System.Collections.Generic;

public class TrackingBhv : CachedTransformBhv
{
    // Public fields
    public Transform trackedTransform;
    [Header("Gizmo Settings:")]
    public Color gizmoColor = Color.red;
    [Min(.001f)]
    public float gizmoRadius = 0.025f;
    [Range(0, 1)]
    public float gizmoSelectedAlpha = 0.75f;
    [Range(0, 1)]
    public float gizmoIdleAlpha = 0.25f;

    // Private fields
    private List<float[]> _states;

    private void OnValidate()
    {
        if (trackedTransform != null)
        {
            this.name = trackedTransform.name + " Tracker";

            this.Position = trackedTransform.position;
        }
    }

    private void Start()
    {
        _states = new List<float[]>();
    }

    private void FixedUpdate()
    {
        if (trackedTransform == null)
        {
            return;
        }

        float[] state = new float[] {
           Time.timeSinceLevelLoad,
           trackedTransform.position.x, trackedTransform.position.y, trackedTransform.position.z,
           trackedTransform.rotation.x, trackedTransform.rotation.y, trackedTransform.rotation.z, trackedTransform.rotation.w
       };

        _states.Add(state);
    }

    private void OnDrawGizmos()
    {
        if (trackedTransform == null)
        {
            return;
        }

        Gizmos.color = gizmoColor.SetAlpha(gizmoIdleAlpha);
        Gizmos.DrawSphere(trackedTransform.position, gizmoRadius);
    }

    private void OnDrawGizmosSelected()
    {
        if (trackedTransform == null)
        {
            return;
        }

        Gizmos.color = gizmoColor.SetAlpha(gizmoSelectedAlpha);
        Gizmos.DrawSphere(trackedTransform.position, gizmoRadius);
    }
}
