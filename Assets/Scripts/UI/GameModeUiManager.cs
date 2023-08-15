using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameModeUiManager : MonoBehaviour
{
    string currentGameMode;
    [SerializeField]
    private TextMeshProUGUI timerText;

    [SerializeField]
    private TextMeshProUGUI roundsText;
    private float gameTime = 30f;
    void Start()
    {
        GameModeManager.Instance.onGameStartCountdownDone.AddListener(IniatlizeGameModeUi);
    }
    public void IniatlizeGameModeUi(string gameMode)
    {


        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        currentGameMode = gameMode;

        switch (currentGameMode)
        {
            case "Stocks":
                StartStocksUi();
                break;
            case "Rounds":
                StartRoundUi();
                break;
            case "Time":
                StartTimeUi();
                break;
        }

    }

    private void StartTimeUi()
    {
        transform.GetChild(2).gameObject.SetActive(true);
        StartCoroutine(TimerCoroutine());
    }


    private IEnumerator TimerCoroutine()
    {

        while (gameTime > 0f)
        {
            timerText.text = gameTime.ToString("0.00");
            yield return new WaitForSeconds(0.1f);
            gameTime -= 0.1f;
        }
        timerText.text = gameTime.ToString("0,00");
        //End Game
        GameModeManager.Instance.EndGameMode();
    }

    private void StartRoundUi()
    {
        transform.GetChild(1).gameObject.SetActive(true);
    }

    private void StartStocksUi()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }
}
