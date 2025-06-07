using Oculus.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BallBounceBhv : CachedRigidbodyBhvOLD
{
    public LayerMask courtLayerMask;
    public LayerMask racketLayerMask;
    public float radius = 0.033f;
    public float normalRestitution = 0.75f;
    public float tangentialRestitution = 0.6f;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        //Vector3 movement = this.Position - _positions[0];
        //float distance = movement.magnitude;

        //Ray ray = new Ray(_positions[0], movement.normalized);

        //if (Physics.Raycast(ray, out RaycastHit hit, distance, racketLayerMask))
        //{
        //    Collider other = hit.collider;

        //    RacketBhv racket = other.GetComponentInParent<RacketBhv>();

        //    if (racket.CanHit())
        //    {
        //        racket.Hit();

        //        this.Hit2(other, racket);

        //        Debug.Log("Racket hit detected via raycast!");
        //    }
        //}
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.darkCyan;

    //    Gizmos.DrawLine(_positions[0], this.Position);
    //}

    private void OnCollisionEnter(Collision collision)
    {
        if (!this.isActiveAndEnabled)
        {
            return;
        }

        if (((1 << collision.gameObject.layer) & courtLayerMask) != 0)
        {
            Bounce(collision);
        }

        //if (((1 << collision.gameObject.layer) & racketLayerMask) != 0)
        //{
        //    RacketBhv racket = collision.gameObject.GetComponent<RacketBhv>();

        //    Vector3 relativePosition = this.Position - racket.Position;

        //    Debug.Log(Vector3.Dot(relativePosition, racket.LastLinearVelocity));

        //    if (Vector3.Dot(relativePosition, racket.LastLinearVelocity) >= 0)
        //    {
        //        Hit(collision, racket);
        //    }
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (((1 << other.gameObject.layer) & racketLayerMask) != 0)
        //{
        //    RacketBhv racket = other.GetComponentInParent<RacketBhv>();

        //    Vector3 relativePosition = this.Position - racket.Position;

        //    if (Vector3.Dot(relativePosition, racket.LastLinearVelocity) >= 0)
        //    {
        //        if (racket.CanHit())
        //        {
        //            racket.Hit();

        //            Hit2(other, racket);
        //        }
        //    }
        //}
    }

    private void Bounce(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;

        Vector3 w_i = this.Rigidbody.angularVelocity;

        Vector3 v_i = this.LastLinearVelocity;
        Vector3 v_normal_i = Vector3.Project(v_i, normal);
        Vector3 v_tangential_i = v_i - v_normal_i;

        Vector3 w_f = 0.4f * w_i + 0.58f * Vector3.Cross(normal, v_tangential_i) / radius;

        Vector3 v_normal_f = -normalRestitution * v_normal_i;
        Vector3 v_tangential_f = 0.65f * v_tangential_i + 0.3f * radius * Vector3.Cross(w_i, normal);

        Vector3 v_other_f = v_normal_f + v_tangential_f;
        Vector3 v_vertical_f = Vector3.Project(v_other_f, Vector3.up);
        Vector3 v_horiontal_f = v_other_f - v_vertical_f;
        Vector3 v_f = v_vertical_f + v_horiontal_f;

        this.Rigidbody.angularVelocity = w_f;
        this.Rigidbody.linearVelocity = v_f;
    }

    private void Bounce2(Collision collision, CachedRigidbodyBhv other2)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;

        Vector3 w_i = this.Rigidbody.angularVelocity;

        Vector3 v_i = this.LastLinearVelocity;
        Vector3 v_normal_i = Vector3.Project(v_i, normal);
        Vector3 v_tangential_i = v_i - v_normal_i;

        Vector3 w_f = 0.4f * w_i + 0.58f * v_tangential_i / radius;

        Vector3 v_normal_f = -normalRestitution * v_normal_i;
        Vector3 v_tangential_f = 0.65f * v_tangential_i + 0.3f * radius * w_i;

        Vector3 v_other_f = v_normal_f + v_tangential_f;
        Vector3 v_vertical_f = Vector3.Project(v_other_f, Vector3.up);
        Vector3 v_horiontal_f = v_other_f - v_vertical_f;
        Vector3 v_f = v_vertical_f + v_horiontal_f;

        this.Rigidbody.angularVelocity = w_f;
        this.Rigidbody.linearVelocity = v_f;
    }


    private void Hit(Collision collision, CachedRigidbodyBhvOLD other)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;

        Vector3 V_ball_i = this.LastLinearVelocity;
        Vector3 V_other_i = other.LastLinearVelocity;
        Vector3 V_other_f = other.Rigidbody.linearVelocity;

        // Separate velocity into normal and tangential components
        Vector3 V_ball_yi = Vector3.Project(V_ball_i, normal);
        Vector3 V_ball_xi = V_ball_i - V_ball_yi;

        Vector3 V_other_yi = Vector3.Project(V_other_i, normal);
        Vector3 V_other_yf = Vector3.Project(V_other_f, normal);
        Vector3 V_other_xi = V_other_i - V_other_yi;
        Vector3 V_other_xf = V_other_f - V_other_yf;

        // Apply restitution to normal component
        Vector3 V_ball_yf = -normalRestitution * (V_ball_yi - V_other_yi) + V_other_yf;

        // Apply friction and spin effects to tangential component
        Vector3 V_ball_xf = tangentialRestitution * (V_ball_xi - V_other_xi) + V_other_xf;

        Vector3 V_ball_f = V_ball_yf + V_ball_xf;

        this.Rigidbody.linearVelocity = V_ball_f;
    }


    private void Hit2(Collider collider, RacketBhv other)
    {
        Vector3 normal = collider.transform.up;

        Vector3 V_ball_i = this.LastLinearVelocity;
        Vector3 V_other_i = other.LastLinearVelocity;
        Vector3 V_other_f = other.Rigidbody.linearVelocity;

        // Separate velocity into normal and tangential components
        Vector3 V_ball_yi = Vector3.Project(V_ball_i, normal);
        Vector3 V_ball_xi = V_ball_i - V_ball_yi;

        Vector3 V_other_yi = Vector3.Project(V_other_i, normal);
        Vector3 V_other_yf = Vector3.Project(V_other_f, normal);
        Vector3 V_other_xi = V_other_i - V_other_yi;
        Vector3 V_other_xf = V_other_f - V_other_yf;

        // Apply restitution to normal component
        Vector3 V_ball_yf = -normalRestitution * (V_ball_yi - V_other_yi) + V_other_yf;

        // Apply friction and spin effects to tangential component
        Vector3 V_ball_xf = tangentialRestitution * (V_ball_xi - V_other_xi) + V_other_xf;

        Vector3 V_ball_f = V_ball_yf + V_ball_xf;

        this.Rigidbody.linearVelocity = V_ball_f;
    }

    Vector3 CalculateBounceWithSpin(Vector3 incomingVelocity, Vector3 normal)
    {
        // Separate velocity into normal and tangential components
        Vector3 normalVelocity = Vector3.Project(incomingVelocity, normal);
        Vector3 tangentialVelocity = incomingVelocity - normalVelocity;

        // Apply restitution to normal component
        Vector3 bounceNormal = -normalVelocity * normalRestitution;

        // Apply friction and spin effects to tangential component
        Vector3 bounceTangential = tangentialVelocity * tangentialRestitution;

        return bounceNormal + bounceTangential;
    }
}
