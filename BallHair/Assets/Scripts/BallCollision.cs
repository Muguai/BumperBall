using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

public class BallCollision : NetworkBehaviour
{
    private float forceMultiplier = 3f;
    private float speedThreshold = 0f;

    private float speedThreshold2 = 3f;
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        

    }
    private void OnCollisionEnter(Collision collision)
    {
        
        Rigidbody otherRigidbody = collision.rigidbody;
        if (otherRigidbody != null)
        {
            // Calculate the force to apply based on the relative velocity and mass of the colliding objects
            if (collision.relativeVelocity.magnitude > speedThreshold2 && otherRigidbody.velocity.magnitude > speedThreshold)
            {
                float forceMagnitude = (otherRigidbody.velocity.magnitude) * otherRigidbody.mass * forceMultiplier;
                Vector3 forceDirection = collision.contacts[0].normal;

                if (forceMagnitude <= 0f)
                {
                    Debug.Log("Not enough force");
                    return;
                }

                AskToCollideServerRpc(forceMagnitude, forceDirection);
            }
            else
            {
                Debug.Log("Not fast enough for collision force. Relative Veloctiy: " + collision.relativeVelocity.magnitude + " VS SpeedThreshold: " + speedThreshold2 + " OtherRigidBodyVelocity: " + otherRigidbody.velocity.magnitude + " VS SpeedThreshold: " + speedThreshold);
            }


        }

    }

    [ServerRpc]
    private void AskToCollideServerRpc(float forceMagnitude, Vector3 forceDirection)
    {
        CollideClientRpc(forceMagnitude, forceDirection);
    }

    [ClientRpc]
    private void CollideClientRpc(float forceMagnitude, Vector3 forceDirection)
    {
        rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
    }
}