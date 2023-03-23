using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    Rigidbody rb;
    float speed = 1f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!IsOwner) return;

        float directionHorzontal = Input.GetAxis("Horizontal");
        float directionVertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(directionHorzontal, 0, directionVertical);

        direction.Normalize();

        rb.AddForce(direction * speed, ForceMode.Force);

        //print("X: " + directionHorzontal + " Y: " + directionVertical);
    }
}
