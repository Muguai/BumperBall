using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    private bool choosingSpawn = false;
    private bool lockedIn = false;
    private float respawnDelay;
    private Vector3 highlightStartPoint;
    private ulong activePlayer;
    private GameObject chooseSpawnHighlight;
    private Vector3 spawnPos;
    [SerializeField]
    private GameObject parachute;

    void Start()
    {
        if(parachute != null)
            parachute.GetComponent<DelayParachuteMovement>().player = this.transform;
        if (DeathManager.Instance != null)
        {

            chooseSpawnHighlight = DeathManager.Instance.chooseSpawnHighlight;

            highlightStartPoint = chooseSpawnHighlight.transform.position;
        }
    }

    void Update()
    {
        if (choosingSpawn)
        {
            respawnDelay -= Time.deltaTime;

            if (Input.GetMouseButtonDown(0))
            {
                lockedIn = !lockedIn;
            }

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool isHit = Physics.Raycast(ray, out hit, 500.0f);
            if (isHit && hit.transform.tag == "Stage" && !lockedIn)
            {

                chooseSpawnHighlight.transform.position = hit.point;

                Debug.Log("HIT " + hit.transform.name);
            }

            if (respawnDelay < 0f)
            {
                chooseSpawnHighlight.SetActive(false);

                spawnPos = chooseSpawnHighlight.transform.position;
                choosingSpawn = false;
                ChooseSpawnServerRPC(activePlayer, spawnPos);
            }
        }
    }
    //Player spawns in the middle of the arena
    public void PreStandardSpawn()
    {
        
    }
    public void StandardSpawn(Vector3 _spawnPos, GameObject player)
    {
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;

        player.transform.position = _spawnPos;
    }

    //Players chooses where to spawn by mouse cursor
    public void PreChooseSpawn(ulong player, float _respawnDelay)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { 0 }
            }
        };

        clientRpcParams.Send.TargetClientIds = new ulong[] { player };
        
        ChooseSpawnPointClientRPC(_respawnDelay, player, clientRpcParams);
    }

    [ClientRpc]
    private void ChooseSpawnPointClientRPC(float _respawnDelay, ulong player,  ClientRpcParams clientRpcParams = default)
    {
        respawnDelay = _respawnDelay;
        choosingSpawn = true;
        activePlayer = player;

        chooseSpawnHighlight.SetActive(true);
        chooseSpawnHighlight.transform.position = highlightStartPoint;
    }

    public void ChooseSpawn()
    {

    }

    [ServerRpc]
    public void ChooseSpawnServerRPC(ulong player, Vector3 _spawnPoint)
    {
        NetworkObject spawningPlayer = NetworkManager.Singleton.ConnectedClients[player].PlayerObject;
        spawningPlayer.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Vector3 finalSpawnPos = new Vector3(_spawnPoint.x, _spawnPoint.y + 15f, _spawnPoint.z);
        spawningPlayer.transform.position = finalSpawnPos;
    }

    //Player gets a little parachute which they slowly fall down on the arena with
    //The parachute can be cut at anytime

    public void PreParachuteSpawn()
    {

    }
    public void ParachuteSpawn()
    {

    }
}
