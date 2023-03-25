using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody rb;
    private float speed = 1f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(this.OwnerClientId == 0)
        {
            GetComponent<Rigidbody>().mass = 3f;

        }
        else
        {

            GetComponent<Rigidbody>().mass = 3f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        float directionHorzontal = Input.GetAxis("Horizontal");
        float directionVertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(directionHorzontal, 0, directionVertical);

        AskToMoveServerRpc(direction.normalized);
    }

    [ServerRpc]
    private void AskToMoveServerRpc(Vector3 forceDirection)
    {
        MovePlayerClientRpc(forceDirection);
    }

    [ClientRpc]
    private void MovePlayerClientRpc(Vector3 forceDirection)
    {
        if(rb != null)
        {
            rb.AddForce(forceDirection * speed, ForceMode.Force);
        }
    }
    
}