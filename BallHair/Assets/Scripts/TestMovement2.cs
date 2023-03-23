using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement2 : MonoBehaviour
{
    Rigidbody rb;
    float speed = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float directionHorzontal = Input.GetAxis("Horizontal");
        float directionVertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(-directionHorzontal, 0, -directionVertical);
        
        //rb.AddForce(direction * speed, ForceMode.Force);

        //print("X: " + directionHorzontal + " Y: " + directionVertical);
    }

    private void LateUpdate()
    {
        
    }
}
