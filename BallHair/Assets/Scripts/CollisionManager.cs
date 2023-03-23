using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    private static CollisionManager _instance;
    public static CollisionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CollisionManager>();
                if (_instance == null)
                {
                    GameObject singleton = new GameObject();
                    _instance = singleton.AddComponent<CollisionManager>();
                    singleton.name = typeof(CollisionManager).ToString() + " (Singleton)";
                }
            }
            return _instance;
        }
    }

    private Dictionary<Rigidbody, Vector3> _velocities = new Dictionary<Rigidbody, Vector3>();

    private List<(float, Collision)> collisions = new System.Collections.Generic.List<(float, Collision)>();

    void Update()
    {
        // Update the velocities of all the rigidbodies
        if(collisions.Count > 0)
        {
            foreach((float, Collision) c in collisions)
            {
                if(c.Item1 > 0.2f)
                {
                    print("Its bigger ");
                }
            }
            collisions.Clear();
        }
    }

    public Vector3 GetVelocity(Rigidbody rb)
    {
        if (_velocities.ContainsKey(rb))
        {
            return _velocities[rb];
        }
        else
        {
            _velocities[rb] = rb.velocity;
            return rb.velocity;
        }
    }

    public void AddVelocity(Rigidbody rb, Vector3 velocity)
    {
        if (_velocities.ContainsKey(rb))
        {
            _velocities[rb] += velocity;
        }
        else
        {
            _velocities[rb] = velocity;
        }
    }

    public void RemoveVelocity(Rigidbody rb)
    {
        if (_velocities.ContainsKey(rb))
        {
            _velocities.Remove(rb);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {

        
        Rigidbody rb1 = collision.contacts[0].thisCollider.attachedRigidbody;
        Rigidbody rb2 = collision.contacts[0].otherCollider.attachedRigidbody;

        collisions.Add((rb2.velocity.magnitude, collision));
        print("Hello" + rb1.name + " " + rb1.velocity.magnitude + " " + rb2.name + " " + rb2.velocity.magnitude);



        return;

        if (rb1 == null || rb2 == null)
        {
            return;
        }

        Vector3 v1 = GetVelocity(rb1);
        Vector3 v2 = GetVelocity(rb2);

        float m1 = rb1.mass;
        float m2 = rb2.mass;

        Vector3 normal = collision.contacts[0].normal;

        float v1n = Vector3.Dot(v1, normal);
        float v2n = Vector3.Dot(v2, normal);

        float impulse = (2.0f * (v1n - v2n)) / (m1 + m2);

        Vector3 collisionForce = impulse * normal;

        AddVelocity(rb1, collisionForce / m1);
        AddVelocity(rb2, -collisionForce / m2);
    }
}