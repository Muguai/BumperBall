using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

using Unity.Netcode.Components;


public class BallCollision : NetworkBehaviour
{
    private float forceMultiplier = 2f;
    private float speedThreshold = 0f;
    private ulong thisClientID;
    Rigidbody rb;
    bool startCollision = false;


    float _mag;
    float _mass;
    Vector3 _forceDirection;

    private ulong lastCollidedPlayer = 69;
    private PlayerMovement playerMovement;

    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();

        if (!IsServer)
        {
            
        }

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer)
        {
            return;
        }

        print("This client collided with something " + this.OwnerClientId);

        if(collision.gameObject.tag == "Stage") { playerMovement.parachute = false; }

        if (collision.gameObject.tag != "Player") return;

        thisClientID = this.OwnerClientId;
        Rigidbody otherRigidbody1 = collision.rigidbody;

        if (otherRigidbody1 != null && startCollision == false && collision.gameObject.tag == "Player")
        {
            
            NetworkRigidbody otherRigidbody = collision.gameObject.GetComponent<NetworkRigidbody>();
            lastCollidedPlayer = otherRigidbody.OwnerClientId;
            

            print("This gets hit " +  thisClientID + " This is the hitter " + otherRigidbody.OwnerClientId);

            print("This is the hitters speed " + collision.rigidbody.velocity.magnitude);


            Vector3 _forceDirection = collision.contacts[0].normal;
            _mag = rb.velocity.magnitude;
            _mass = rb.mass;
            float forceMagnitude = (_mag) * _mass * forceMultiplier;

            //startCollision = true;
            CollideClientRpc(forceMagnitude, _forceDirection);

            //StartCoroutine(DelayedForceApplication(forceMagnitude, _forceDirection.normalized));



        }
        else
        {
            print("NullRigidbody " + collision.gameObject.name);
        }

    }

    private void LateUpdate()
    {
        if (!IsServer) return;

        if (startCollision)
        {
            //CollideClientRpc(_mag, _mass, _forceDirection);
            startCollision = false;
        }
    }

    private IEnumerator DelayedForceApplication(float forceMagnitude, Vector3 forceDirection)
    {
        yield return new WaitForSeconds(0.1f); // Wait for 0.1 seconds

        CollideClientRpc(forceMagnitude, _forceDirection);

        //AskToCollideServerRpc(forceMagnitude, forceDirection);
    }

    

    [ClientRpc]
    private void CollideClientRpc(float forceMagnitude, Vector3 forceDirection)
    {
        playerMovement.parachute = false;
        // Calculate the force to apply based on the relative velocity and mass of the colliding objects
        if (forceMagnitude > speedThreshold)
        {

            //StartCoroutine(DelayedForceApplication(forceMagnitude, forceDirection));
            rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
            print("COLLIDE " + this.GetComponent<NetworkTransform>().OwnerClientId + " mag: " + forceMagnitude);
            //AskToCollideServerRpc(forceMagnitude, forceDirection);
        }
        else
        {
            Debug.Log(" OtherRigidBodyVelocity: " + forceMagnitude + " VS SpeedThreshold: " + speedThreshold);
        }
        //rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
    }

    public ulong GetLastCollidedPlayer()
    {
        return lastCollidedPlayer;
    }

    public void SetLastCollidedPlayer()
    {
        lastCollidedPlayer = 69;
    }
}