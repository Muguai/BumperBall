using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeGameMode : GameMode
{
    private int[] playerScoreTime;
    private float gameTime = 30f;
    public string GetGameMode()
    {
        return "Time";
    }

    public void InitializeGameMode(int numberOfConnectedClients)
    {
        //Set gameTime from settings later
        gameTime = 30f;

        playerScoreTime = new int[numberOfConnectedClients];
        for (int i = 0; i < playerScoreTime.Length; i++)
        {
            playerScoreTime[i] = 0;
        }
    }

    public void StartGameMode()
    {
        //StartCoroutine(TimerCoroutine());
    }

    private IEnumerator TimerCoroutine()
    {

        while (gameTime > 0f)
        {

            yield return new WaitForSeconds(0.1f);
            gameTime -= 0.1f;
        }

        EndGameMode();
        //End Game Mode
    }

    public bool HandleTimeDeath(GameObject deadPlayer, ulong clientID)
    {
        ulong killer = deadPlayer.GetComponent<BallCollision>().GetLastCollidedPlayer();
        if(killer <= 10)
        {
            playerScoreTime[killer] += 1;
            deadPlayer.GetComponent<BallCollision>().SetLastCollidedPlayer();
        }
        playerScoreTime[clientID] -= 1;

        Debug.Log("Current Score is " + playerScoreTime.ToString());
        return true;
    }

    public int[] GetPlacings()
    {
        return playerScoreTime;
    }

    public void EndGameMode()
    {
        GameModeManager.Instance.EndGameMode();
    }
}
