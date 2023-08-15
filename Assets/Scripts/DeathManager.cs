using UnityEngine;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Linq;
using UnityEngine.Events;
using System;

public class DeathManager : NetworkBehaviour
{

    [SerializeField]
    public float respawnDelay { get; private set; } = 5f;

    public GameObject chooseSpawnHighlight;

    private bool[] playerIsDead;
    private Vector3[] playerRespawnPoints;
    public Transform TestRespawnPos;

    [System.Serializable]
    public class PlayerDeathEvent : UnityEvent<ulong, bool> { }
    public PlayerDeathEvent onPlayerDeath;


    [System.Serializable]
    public class OneManLeftStanding : UnityEvent<ulong> { }
    public OneManLeftStanding oneManLeftStanding;

    private static DeathManager _instance;

    public static DeathManager Instance { get { return _instance; } }

    private string spawnMethod;
    private SpawnManager spawnManager;
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


    void Start()
    {
        spawnManager = GetComponent<SpawnManager>();
        spawnMethod = "Choose";
        GameModeManager.Instance.onGameStart.AddListener(InitializeDeathVariables);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        print("Connected");
        if (other.gameObject.tag == "Player")
        {
            NetworkTransform nt = other.gameObject.GetComponent<NetworkTransform>();
            ulong clientID = nt.OwnerClientId;
            bool shouldRespawn = true;

            if (!GameModeManager.Instance.HasGameStarted())
            {
                nt.Interpolate = false;
                StartCoroutine(PreGameRespawnPlayer(other.gameObject, clientID));
                return;
            }


            onPlayerDeath.Invoke(clientID, shouldRespawn);
            playerIsDead[clientID] = true;
            Debug.Log("Player " + nt.gameObject.name + " has died.");

            switch (GameModeManager.Instance.GM.GetGameMode())
            {
                case "Stocks":
                    StocksGameMode SGM = (StocksGameMode)GameModeManager.Instance.GM;
                    shouldRespawn = SGM.HandleStocksDeath(clientID);
                    break;
                case "Rounds":
                    RoundGameMode RGM = (RoundGameMode)GameModeManager.Instance.GM;
                    shouldRespawn = RGM.HandleRoundsDeath(clientID, playerIsDead);
                    break;
                case "Time":
                    TimeGameMode TGM = (TimeGameMode)GameModeManager.Instance.GM;
                    shouldRespawn = TGM.HandleTimeDeath(other.gameObject, clientID);
                    break;
            }

            if (!shouldRespawn) return;

            nt.Interpolate = false;
            StartCoroutine(RespawnPlayer(other.gameObject, clientID));
        }
    }

    IEnumerator PreGameRespawnPlayer(GameObject player, ulong playerIndex)
    {

        yield return new WaitForSeconds(respawnDelay);
        print("Respawn");
        player.transform.position = TestRespawnPos.position;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;

        player.gameObject.GetComponent<NetworkTransform>().Interpolate = true;
    }

    IEnumerator RespawnPlayer(GameObject player, ulong playerIndex)
    {
        SpawnManager playerSpawnManager = NetworkManager.Singleton.ConnectedClients[playerIndex].PlayerObject.GetComponent<SpawnManager>();
     
        switch (spawnMethod)
        {
            case "Standard":
                playerSpawnManager.PreStandardSpawn();
                break;
            case "Choose":
                playerSpawnManager.PreChooseSpawn(playerIndex, respawnDelay);
                break;
            case "Parachute":
                playerSpawnManager.PreParachuteSpawn();
                break;
        }
        yield return new WaitForSeconds(respawnDelay);
        print("Respawn");

        playerIsDead[playerIndex] = false;

        switch (spawnMethod)
        {
            case "Standard":
                playerSpawnManager.StandardSpawn(playerRespawnPoints[playerIndex], player);
                break;
            case "Choose":
                playerSpawnManager.ChooseSpawn();
                break;
            case "Parachute":
                playerSpawnManager.ParachuteSpawn();
                break;
        }

        player.gameObject.GetComponent<NetworkTransform>().Interpolate = true;
    }

    public void InitializeDeathVariables(ulong NumberOfConnectedPlayers)
    {
        if (!IsServer) return;

     

        playerIsDead = new bool[NumberOfConnectedPlayers];

        int numChildren = this.transform.childCount;
        playerRespawnPoints = new Vector3[numChildren];
        for (int i = 0; i < numChildren; i++)
        {
            Transform childTransform = this.transform.GetChild(i);
            playerRespawnPoints[i] = childTransform.position;
        }
    }

}