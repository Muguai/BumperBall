using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class EndGameUiManager : MonoBehaviour
{
    [SerializeField]
    private Transform placingsGroup;
    [SerializeField]
    private TextMeshProUGUI winnerText;
    // Start is called before the first frame update
    void Start()
    {

        GameModeManager.Instance.onGameEnd.AddListener(EndGameUi);

    }

    public void EndGameUi()
    {
        print("EndgameUI");
        this.transform.GetChild(0).gameObject.SetActive(true);

        for (int i = 4; i > (int)GameModeManager.Instance.amountOfPlayers; i--)
        {
            placingsGroup.GetChild(i - 1).gameObject.SetActive(false);
        }
        
        int[] placings = GameModeManager.Instance.placings;
        string[] names = new string[] { "Player 1", "Player 2", "Player 3", "Player 4" };
        //Sorting the array
        for (int j = 0; j <= placings.Length - 2; j++)
        {
            //intArray.Length - 2
            for (int i = 0; i <= placings.Length - 2; i++)
            {
                //count = count + 1;
                if (placings[i] < placings[i + 1])
                {
                    int temp = placings[i + 1];
                    placings[i + 1] = placings[i];
                    placings[i] = temp;
                    string temp2 = names[i + 1];
                    names[i + 1] = names[i];
                    names[i] = temp2;
                }
            }
        }


        winnerText.text = names[0];


        for (int i = 0; i < placings.Length; i++)
        {
            placingsGroup.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
            placingsGroup.transform.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().text = names[i];
            placingsGroup.transform.GetChild(i).GetChild(3).GetComponent<TextMeshProUGUI>().text = placings[i].ToString();
        }
    }


}
