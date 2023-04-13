using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody rb;
    private float speed = 2.5f;
    private bool isMovementPaused = false;

    public Material[] materials;

    public MeshRenderer playerHead;
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
        this.playerHead.material = materials[this.OwnerClientId];
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (isMovementPaused)
        {
            rb.velocity = Vector3.zero;
            return;
        }
            

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

    [ClientRpc]
    public void PauseMovementClientRpc(bool set, ClientRpcParams clientRpcParams = default)
    {
        rb.velocity = Vector3.zero;
        this.isMovementPaused = set;
    }

    [ClientRpc]
    public void PutPlayersAtSpawnPosClientRPC(Vector3 point, ClientRpcParams clientRpcParams = default)
    {
        rb.velocity = Vector3.zero;
        this.transform.position = point;
    }

}
