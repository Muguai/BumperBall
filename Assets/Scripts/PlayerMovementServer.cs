using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

public class PlayerMovementServer : NetworkBehaviour
{
    private PlayerMovement PlayerMovementInstance;
    private Rigidbody rb;
    private float speed;
    private bool clientPred;

    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 120f;
    private const int BUFFER_SIZE = 1024;

    private StatePayload[] stateBuffer;
    private Queue<InputPayload> inputQueue;

    void Start()
    {
        PlayerMovementInstance = GetComponent<PlayerMovement>();
        rb = PlayerMovementInstance.rb;
        speed = PlayerMovementInstance.speed;
        clientPred = PlayerMovementInstance.clientPred;

        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        stateBuffer = new StatePayload[BUFFER_SIZE];
        inputQueue = new Queue<InputPayload>();
    }

    void Update() 
    {
        if (!IsServer && clientPred) return;

        timer += Time.deltaTime;

        while (timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            HandleTick();
            currentTick++;
        }
    }

    [ServerRpc]
    public void OnClientInputServerRPC(InputPayload inputPayload)
    {
        inputQueue.Enqueue(inputPayload);
    }

    void HandleTick()
    {
        // Process the input queue
        int bufferIndex = -1;
        while (inputQueue.Count > 0)
        {
            InputPayload inputPayload = inputQueue.Dequeue();

            bufferIndex = inputPayload.tick % BUFFER_SIZE;

            StatePayload statePayload = ProcessMovement(inputPayload);
            statePayload.playerId = inputPayload.playerId;
            stateBuffer[bufferIndex] = statePayload;
        }

        if (bufferIndex != -1)
        {
            StatePayload state = stateBuffer[bufferIndex];

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { 0 }
                }
            };

            clientRpcParams.Send.TargetClientIds = new ulong[] { state.playerId };


            PlayerMovementInstance.OnServerMovementStateClientRPC(state, clientRpcParams);
        }
    }

    StatePayload ProcessMovement(InputPayload input)
    {
        // Should always be in sync with same function on Client
       rb.AddForce(input.inputVector * speed * minTimeBetweenTicks, ForceMode.Force);

        //transform.position += input.inputVector * 5f * minTimeBetweenTicks;

        return new StatePayload()
        {
            tick = input.tick,
            position = PlayerMovementInstance.rb.position,
            velocity = PlayerMovementInstance.rb.velocity,
            //acceleration = PlayerMovementInstance.rb.angularVelocity
        };
    }
}
