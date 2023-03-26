using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.Events;


public class RoundManager : NetworkBehaviour
{

    [SerializeField]
    private GameObject spawnPoints;
    public int startGameDelay { get; private set; } = 5;

    [System.Serializable]
    public class GameStartEvent : UnityEvent { }
    public GameStartEvent onGameStart;


    private bool isGameStarted = false;

    private static RoundManager _instance;

    public static RoundManager Instance { get { return _instance; } }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (NetworkManager.Singleton.ConnectedClients.Count < 2) return;
            isGameStarted = true;
            StartGame();
        }
    }

    private void StartGame()
    {
        if (!IsServer) return;

        print("HelloStartGame");

        

        int playercount = NetworkManager.Singleton.ConnectedClients.Count;

        Transform correctSpawnPoints = spawnPoints.transform.GetChild(playercount - 2);

        int numChildren = correctSpawnPoints.childCount;
        Vector3[] playerRespawnPoints = new Vector3[numChildren];
        for (int i = 0; i < numChildren; i++)
        {
            Transform childTransform = correctSpawnPoints.GetChild(i);
            playerRespawnPoints[i] = childTransform.position;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { 0 }
            }
        };

        foreach (ulong u in NetworkManager.Singleton.ConnectedClientsIds)
        {
            clientRpcParams.Send.TargetClientIds = new ulong[] { u };
            NetworkObject o = NetworkManager.Singleton.ConnectedClients[u].PlayerObject;
            PlayerMovement Pm = o.GetComponent<PlayerMovement>();
            Pm.PauseMovementClientRpc(true, clientRpcParams);
            Pm.PutPlayersAtSpawnPosClientRPC(playerRespawnPoints[u]);

        }


        StartGameCountdownClientRPC();
    }

    [ClientRpc]
    public void StartGameCountdownClientRPC()
    {
        onGameStart.Invoke();
    }

    public void StartGameAfterCountdown()
    {
        if (!IsServer) return;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { 0 }
            }
        };

        foreach (ulong u in NetworkManager.Singleton.ConnectedClientsIds)
        {
            clientRpcParams.Send.TargetClientIds = new ulong[] { u };
            NetworkObject o = NetworkManager.Singleton.ConnectedClients[u].PlayerObject;
            o.GetComponent<PlayerMovement>().PauseMovementClientRpc(false, clientRpcParams);
        }
    }

    public bool HasGameStarted()
    {
        return isGameStarted;
    }
}
