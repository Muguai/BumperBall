using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StocksGameMode : GameMode
{
    private int numberOfStocks = 3;

    private const int MaxNumberOfStock = 6;


    private int[] playerStocks;
    public string GetGameMode()
    {
        return "Stocks";
    }

    public void InitializeGameMode(int numberOfConnectedClients)
    {

        playerStocks = new int[numberOfConnectedClients];

        for (int i = 0; i < playerStocks.Length; i++)
        {
            playerStocks[i] = numberOfStocks;
        }

        if (numberOfStocks > MaxNumberOfStock)
        {
            numberOfStocks = MaxNumberOfStock;
        }
    }

    public void EndGameMode()
    {
        GameModeManager.Instance.EndGameMode();
    }

    public void StartGameMode()
    {
        
    }


    public bool HandleStocksDeath(ulong clientID)
    {
        if (GameModeManager.Instance.HasGameStarted() == false) return false;

       

        if (playerStocks[clientID] > 0)
        {
            playerStocks[clientID] -= 1;

            int amountOfDeadPlayers = 0;

            foreach (int i in playerStocks)
            {
                if (i <= 0) amountOfDeadPlayers++;
            }


            if (amountOfDeadPlayers >= playerStocks.Length - 1)
            {
                playerStocks[clientID] = amountOfDeadPlayers - playerStocks.Length;
                EndGameMode();
                return false;
            }

            if (playerStocks[clientID] <= 0)
            {
                playerStocks[clientID] = amountOfDeadPlayers - playerStocks.Length;
                return false;
            }
                

            return true;
        }

        return false;
    }

    public int[] GetPlacings()
    {
        return playerStocks;
    }

    public int GetNumberOfStocks()
    {
        return numberOfStocks;
    }

    public int GetNumberOfMaxStocks()
    {
        return MaxNumberOfStock;
    }
}
