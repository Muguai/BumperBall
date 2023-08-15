using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.Events;


public class GameModeManager : NetworkBehaviour
{

    [SerializeField]
    private GameObject spawnPoints;
    public int startGameDelay { get; private set; } = 5;
    public ulong amountOfPlayers { get; private set; }

    public int[] placings { get; private set; }

    [System.Serializable]
    public class GameStartEvent : UnityEvent<ulong> { }
    public GameStartEvent onGameStart;

    [System.Serializable]
    public class GameStartCountdownEvent : UnityEvent<string> { }
    public GameStartCountdownEvent onGameStartCountdownDone;

    [System.Serializable]
    public class GameEndEvent : UnityEvent { }
    public GameEndEvent onGameEnd;




    private bool isGameStarted = false;

    public GameMode GM { get; private set; }

    //Rounds Game Mode Logic

    //Time Game Mode Logic

    //Stocks Game Mode Logic

    private static GameModeManager _instance;

    public static GameModeManager Instance { get { return _instance; } }

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
        GM = new StocksGameMode();
        print("This is the current GameMode " + GM.GetGameMode());
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (NetworkManager.Singleton.ConnectedClients.Count < 2) return;
            InitializeGame();
        }
    }

    private void InitializeGame()
    {
        if (!IsServer) return;

        isGameStarted = true;

        int playercount = NetworkManager.Singleton.ConnectedClients.Count;

        GM.InitializeGameMode(playercount);

        print("Start a game of " + playercount + " players with gamemode " + GM.GetGameMode());

        PutPlayersAtSpawnPos(playercount);

        StartGameCountdownClientRPC((ulong)NetworkManager.Singleton.ConnectedClientsIds.Count);
    }

    public void RestartGameMode()
    {
        if (!IsServer) return;

        int playercount = NetworkManager.Singleton.ConnectedClients.Count;

        print("Restart a game of " + playercount + " players with gamemode " + GM.GetGameMode());

        PutPlayersAtSpawnPos(playercount);

        StartGameCountdownClientRPC((ulong)NetworkManager.Singleton.ConnectedClientsIds.Count);
    }

    private void PutPlayersAtSpawnPos(int playercount)
    {

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
    }

    [ClientRpc]
    public void StartGameCountdownClientRPC(ulong numberOfPlayers)
    {
        amountOfPlayers = numberOfPlayers;
        onGameStart.Invoke(numberOfPlayers);
    }

    public void EndGameMode()
    {
        if (!IsServer) return;


        isGameStarted = false;

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

        }
        print("EndgameUIClientRPC");
        EndGameEventClientRPC(GM.GetPlacings());
    }

    [ClientRpc]
    public void EndGameEventClientRPC(int[] _placings)
    {
        placings = _placings;
        Instance.onGameEnd.Invoke();
    }

    public void StartGameAfterCountdown()
    {
        if (!IsServer) return;

        GM.StartGameMode();

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

        GameStartCountdownDoneClientRPC(GM.GetGameMode());
    }

    [ClientRpc]
    public void GameStartCountdownDoneClientRPC(string gameMode)
    {
        
        Instance.onGameStartCountdownDone.Invoke(gameMode);
    }

    public bool HasGameStarted()
    {
        return isGameStarted;
    }

}
