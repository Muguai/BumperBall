using System.Collections;
using System.Collections.Generic;

public interface GameMode
{

    public string GetGameMode();

    public void EndGameMode();

    public void StartGameMode();

    public void InitializeGameMode(int numberOfConnectedClients);

    public int[] GetPlacings();

}
