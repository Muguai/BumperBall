using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;


public class PlayerUiManager : NetworkBehaviour
{

    GameObject[] playerUis;

    // Start is called before the first frame update
    void Start()
    {
        int numChildren = this.transform.childCount;
        playerUis = new GameObject[numChildren];
        for (int i = 0; i < numChildren; i++)
        {
            GameObject playerUI = this.transform.GetChild(i).gameObject;
            playerUis[i] = playerUI;
        }

        foreach(GameObject p in playerUis)
        {
            p.transform.GetChild(2).gameObject.GetComponent<Stocks>().SetStartStock();
        }

        

        DeathManager.Instance.onPlayerDeath.AddListener(OnPlayerDeath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void OnPlayerDeath(ulong playerIndex, bool shouldRespawn)
    {
        if (DeathManager.Instance.stocks == true)
        {
            UpdateStocksClientRPC(playerIndex);
        }

        if (shouldRespawn)
        {
            RespawnPlayerUIClientRPC(playerIndex);
        }
    }

    [ClientRpc]
    void UpdateStocksClientRPC(ulong playerIndex)
    {
        playerUis[playerIndex].transform.GetChild(2).gameObject.GetComponent<Stocks>().RemoveStock();
    }

    [ClientRpc]
    void RespawnPlayerUIClientRPC(ulong playerIndex)
    {
        print("HelloClientUI");
        GameObject child = playerUis[playerIndex].transform.GetChild(1).GetChild(0).gameObject;
        child.GetComponentInChildren<RespawnTimer>().StartTimer();
    }
}
