using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoundGameMode : GameMode
{
    private int[] playerScoreRounds;
    private int numberOfRounds = 3;
    public string GetGameMode()
    {
        return "Rounds";
    }

    public void InitializeGameMode(int numberOfConnectedClients)
    {
        playerScoreRounds = new int[numberOfConnectedClients];
        for (int i = 0; i < playerScoreRounds.Length; i++)
        {
            playerScoreRounds[i] = 0;
        }
    }
    public void StartGameMode()
    {

    }

    public bool HandleRoundsDeath(ulong clientID, bool[] playerIsDead)
    {
        int amountOfDeadPlayers = playerIsDead.Count(c => c);

        //No score for the first player death
        if (playerIsDead.Count() - 1 == amountOfDeadPlayers && playerIsDead.Count() > 2) return false;

        //Apply Score
        playerScoreRounds[clientID] += amountOfDeadPlayers - 1;

        //If there is only one player left. give him score and end the round
        if (playerIsDead.Length - amountOfDeadPlayers == 1)
        {
            Debug.Log("one player left ");
            for (int i = 0; i < playerIsDead.Length; i++)
            {
                if (playerIsDead[i] == false)
                {
                    playerScoreRounds[i] += amountOfDeadPlayers + 1;
                    numberOfRounds = numberOfRounds - 1;
                    if (numberOfRounds <= 0)
                        EndGameMode();
                    else
                        RestartRound();
                    DeathManager.Instance.oneManLeftStanding.Invoke((ulong)i);
                }
            }
        }

        Debug.Log("Current Score is " + playerScoreRounds.ToString());

        return false;
    }

    public void EndGameMode()
    {
        GameModeManager.Instance.EndGameMode();
    }

    public void RestartRound()
    {
        GameModeManager.Instance.RestartGameMode();
    }

    public int[] GetPlacings()
    {
        return playerScoreRounds;
    }

    public int GetRoundsLeft()
    {
        return numberOfRounds;
    }


}
