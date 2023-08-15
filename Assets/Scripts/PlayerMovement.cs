using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public struct InputPayload : INetworkSerializable
{
    public int tick;
    public ulong playerId;
    public Vector3 inputVector;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref inputVector);
        serializer.SerializeValue(ref playerId);
    }
}

public struct StatePayload : INetworkSerializable
{
    public int tick;
    public Vector3 position;
    public Vector3 velocity;

    public ulong playerId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref position);

        serializer.SerializeValue(ref velocity);

        serializer.SerializeValue(ref playerId);
    }
}

public class PlayerMovement : NetworkBehaviour
{
    public Rigidbody rb { get; set; }
    public float speed { get; } = 500f;
    private bool isMovementPaused = false;

    public Material[] materials;

    public MeshRenderer playerHead;

    // Shared
    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 120f;
    private const int BUFFER_SIZE = 1024;

    // Client specific
    private StatePayload[] stateBuffer;
    private InputPayload[] inputBuffer;
    private StatePayload latestServerState;
    private StatePayload lastProcessedState;
    private float horizontalInput;
    private float verticalInput;

    private PlayerMovementServer serverInstance;

    public bool clientPred { get; } = false;
    public bool parachute { get; set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        serverInstance = GetComponent<PlayerMovementServer>();

        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        stateBuffer = new StatePayload[BUFFER_SIZE];
        inputBuffer = new InputPayload[BUFFER_SIZE];

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
        if (!IsOwner && !IsLocalPlayer) return;

        if (isMovementPaused)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput);

        


        if (clientPred == false)
        {
            if (parachute)
            {
                rb.useGravity = false;
                AskToParachuteServerRpc(direction.normalized);
                return;
            }
            else
            {
                rb.useGravity = true;
                AskToMoveServerRpc(direction.normalized);
                return;
            }
        }


        timer += Time.deltaTime;

        while (timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            HandleTick();
            currentTick++;
        }
    }
    

    [ClientRpc]
    public void OnServerMovementStateClientRPC(StatePayload serverState, ClientRpcParams clientRpcParams = default)
    {
        latestServerState = serverState;
    }

    void HandleTick()
    {
        if (!latestServerState.Equals(default(StatePayload)) &&
            (lastProcessedState.Equals(default(StatePayload)) ||
            !latestServerState.Equals(lastProcessedState)))
        {
            HandleServerReconciliation();
        }

        int bufferIndex = currentTick % BUFFER_SIZE;

        // Add payload to inputBuffer
        InputPayload inputPayload = new InputPayload();
        inputPayload.tick = currentTick;
        inputPayload.playerId = OwnerClientId;
        inputPayload.inputVector = new Vector3(horizontalInput, 0, verticalInput);
        inputBuffer[bufferIndex] = inputPayload;

        // Add payload to stateBuffer
        stateBuffer[bufferIndex] = ProcessMovement(inputPayload);

        // Send input to server
        serverInstance.OnClientInputServerRPC(inputPayload);
    }

    StatePayload ProcessMovement(InputPayload input)
    {
        // Should always be in sync with same function on Server
        rb.AddForce(input.inputVector * speed * minTimeBetweenTicks, ForceMode.Force);

        return new StatePayload()
        {
            tick = input.tick,
            position = rb.position,
            velocity = rb.velocity
        };
    }

    void HandleServerReconciliation()
    {

        //if (IsServer) return;

        lastProcessedState = latestServerState;

        int serverStateBufferIndex = latestServerState.tick % BUFFER_SIZE;
        float positionError = Vector3.Distance(latestServerState.position, stateBuffer[serverStateBufferIndex].position);
        float velocityError = Vector3.Distance(latestServerState.velocity, stateBuffer[serverStateBufferIndex].velocity);
        //float accelerationError = Vector3.Distance(latestServerState.acceleration, stateBuffer[serverStateBufferIndex].acceleration);

        if (positionError > 0.1f || velocityError > 0.1f) // || velocityError  > 0.1f|| accelerationError > 0.1f
        {
            Debug.Log("We have to reconcile");
            // Rewind & Replay
            rb.position = latestServerState.position;
            rb.velocity = latestServerState.velocity;
           // rb.angularVelocity = latestServerState.acceleration;

            // Update buffer at index of latest server state
            stateBuffer[serverStateBufferIndex] = latestServerState;

            // Now re-simulate the rest of the ticks up to the current tick on the client
            int tickToProcess = latestServerState.tick + 1;

            while (tickToProcess < currentTick)
            {
                int bufferIndex = tickToProcess % BUFFER_SIZE;

                // Process new movement with reconciled state
                StatePayload statePayload = ProcessMovement(inputBuffer[bufferIndex]);

                // Update buffer with recalculated state
                stateBuffer[bufferIndex] = statePayload;

                tickToProcess++;
            }
        }
    }

    [ServerRpc]
    private void AskToParachuteServerRpc(Vector3 forceDirection)
    {
        if (rb != null)
        {
            rb.AddForce(forceDirection * speed, ForceMode.Force);
            rb.AddForce(Physics.gravity * 0.1f);
        }

    }

    [ServerRpc]
    private void AskToMoveServerRpc(Vector3 forceDirection)
    {
        if (rb != null)
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
