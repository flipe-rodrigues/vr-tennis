using UnityEngine;

// move refractory to swing/hit script
// replace lin & ang velocities with smooth versions (small tau)

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class RacketRigidbodyBhvOLD : CachedRigidbodyBhv
{
    //// Public fields
    //public RacketTransformBhv connectedTransform;

    //private void OnValidate()
    //{
    //    if (connectedTransform != null && connectedTransform.connectedRigidbody != this)
    //    {
    //        connectedTransform.connectedRigidbody = this;
    //    }
    //}

    //private void Update()
    //{
    //    if (!Application.isPlaying)
    //    {
    //        this.MoveTransform();
    //    }
    //}

    //protected override void FixedUpdate()
    //{
    //    base.FixedUpdate();

    //    this.MoveRigidbody();
    //}

    //public void MoveTransform()
    //{
    //    if (connectedTransform == null)
    //    {
    //        return;
    //    }

    //    this.Position = connectedTransform.Position;
    //    this.Rotation = connectedTransform.Rotation;
    //}

    //private void MoveRigidbody()
    //{
    //    if (connectedTransform == null)
    //    {
    //        return;
    //    }

    //    this.Move(connectedTransform.Position, connectedTransform.Rotation);
    //}
}
