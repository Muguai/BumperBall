using UnityEngine;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;

using UnityEngine.Events;

public class DeathManager : NetworkBehaviour
{

    [SerializeField]
    public float respawnDelay { get; private set; } = 5f;

    private bool[] playerIsDead;
    private Vector3[] playerRespawnPoints;

    private int[] playerStocks;

    [SerializeField]
    public bool stocks { get; set; } = true;
    public int numberOfStocks { get; private set; } = 4;

    public const int MaxNumberOfStock = 6;


    [System.Serializable]
    public class PlayerDeathEvent : UnityEvent<ulong, bool> { }
    public PlayerDeathEvent onPlayerDeath;

    private static DeathManager _instance;

    public static DeathManager Instance { get { return _instance; } }

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

        if(numberOfStocks > MaxNumberOfStock)
        {
            numberOfStocks = MaxNumberOfStock;
        }
    }


    void Start()
    {
        //Change these from 4 to amount of connected players later;
        playerIsDead = new bool[4];
        playerStocks = new int[4];

        for (int i = 0; i < 4; i++)
        {
            playerStocks[i] = numberOfStocks;
        }

        int numChildren = this.transform.childCount;
        playerRespawnPoints = new Vector3[numChildren];
        for (int i = 0; i < numChildren; i++)
        {
            Transform childTransform = this.transform.GetChild(i);
            playerRespawnPoints[i] = childTransform.position;
        }

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

            if (stocks)
            {
                shouldRespawn = HandleStocks(clientID);
            }


            onPlayerDeath.Invoke(clientID, shouldRespawn);
            playerIsDead[clientID] = true;
            Debug.Log("Player " + nt.gameObject.name + " has died.");

            if (!shouldRespawn) return;

            nt.Interpolate = false;
            StartCoroutine(RespawnPlayer(other.gameObject, clientID));
        }
    }

    IEnumerator RespawnPlayer(GameObject player, ulong playerIndex)
    {

        yield return new WaitForSeconds(respawnDelay);
        print("Respawn");
        playerIsDead[playerIndex] = false;
        player.transform.position = playerRespawnPoints[playerIndex];
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;

        player.gameObject.GetComponent<NetworkTransform>().Interpolate = true;
    }

    private bool HandleStocks(ulong clientID)
    {

        if (playerStocks[clientID] > 0)
        {
            playerStocks[clientID] -= 1;
            if (playerStocks[clientID] <= 0) return false;

            return true;
        }

        return false;
    }

    public int GetMaxNumberOfStocks()
    {
        return MaxNumberOfStock;
    }
}