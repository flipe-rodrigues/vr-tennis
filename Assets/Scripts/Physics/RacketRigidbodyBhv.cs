using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class RacketRigidbodyBhv : CachedRigidbodyBhv
{
    // Public fields
    public RacketAnchorBhv anchorTransform;

    private void OnValidate()
    {
        if (anchorTransform != null && anchorTransform.anchoredRigidbody != this)
        {
            anchorTransform.anchoredRigidbody = this;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            this.MoveTransform();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        this.MoveRigidbody();
    }

    public void MoveTransform()
    {
        if (anchorTransform == null)
        {
            return;
        }

        this.Position = anchorTransform.Position;
        this.Rotation = anchorTransform.Rotation;
    }

    private void MoveRigidbody()
    {
        if (anchorTransform == null)
        {
            return;
        }

        this.Move(anchorTransform.Position, anchorTransform.Rotation);
    }
}
