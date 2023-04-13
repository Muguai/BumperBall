using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;


public class PlayerUiManager : NetworkBehaviour
{

    private GameObject[] playerUis;

    // Start is called before the first frame update
    void Start()
    {
        DeathManager.Instance.onPlayerDeath.AddListener(OnPlayerDeath);
        GameModeManager.Instance.onGameStart.AddListener(SetPlayerUiActive);
    }

    void OnPlayerDeath(ulong playerIndex, bool shouldRespawn)
    {
        if (GameModeManager.Instance.GM.GetGameMode() == "Stocks")
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
        child.GetComponentInChildren<RespawnTimer>().StartRespawnUiTimer();
    }

    void SetPlayerUiActive(ulong numberOfPlayers)
    {
        playerUis = new GameObject[numberOfPlayers];
        for (int i = 0; i < (int)numberOfPlayers; i++)
        {
            GameObject playerUI = this.transform.GetChild(i).gameObject;
            playerUis[i] = playerUI;
            playerUI.SetActive(true);
        }

        foreach (GameObject p in playerUis)
        {
            p.transform.GetChild(2).gameObject.GetComponent<Stocks>().SetStartStock();
        }

    }
}
