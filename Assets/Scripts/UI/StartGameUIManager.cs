using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using Unity.Netcode;

public class StartGameUIManager : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI startText;
    [SerializeField]
    private GameObject startGame;

    [System.Serializable]
    public class GameStartCountdownEvent : UnityEvent { }
    public GameStartCountdownEvent onGameStartCountdownDone;

    private int roundsLeft = 3;
    // Start is called before the first frame update
    void Start()
    {
        startGame.SetActive(false);
        GameModeManager.Instance.onGameStart.AddListener(StartGameUI);
    }


    public void StartGameUI(ulong numberOfPlayers)
    {
        StartCoroutine(TimerCoroutine());

    }

    private IEnumerator TimerCoroutine()
    {
        startGame.gameObject.SetActive(true);
        // Wait for the respawn delay
        float orgStartTime = GameModeManager.Instance.startGameDelay;
        float remainingTime = orgStartTime;

        while (remainingTime > -1f)
        {
            if (remainingTime <= 0f)
            {
                startText.text = "START";
                
               
            }
            else
            {

                startText.text = $"{remainingTime:0}";

                if (remainingTime > orgStartTime - 1)
                {
                    if (GameModeManager.Instance.GM.GetGameMode() == "Rounds")
                    {
                        startText.text = "Round: " + roundsLeft;
                        roundsLeft = roundsLeft - 1;

                    }
                }
            }

            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }
        startGame.gameObject.SetActive(false);

        onGameStartCountdownDone.Invoke();
        


    }
}
