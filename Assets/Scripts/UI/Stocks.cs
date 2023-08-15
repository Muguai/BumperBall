using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stocks : MonoBehaviour
{
    private int currentStockCount;


    public void SetStartStock()
    {

        if (GameModeManager.Instance.GM.GetGameMode() != "Stocks")
        {
            currentStockCount = 0;
;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            StocksGameMode SGM = (StocksGameMode)GameModeManager.Instance.GM;
            int maxNumberOfPossibleStocks = SGM.GetNumberOfMaxStocks();
            currentStockCount = SGM.GetNumberOfStocks();
            for (int i = maxNumberOfPossibleStocks; i > currentStockCount; i--)
            {
                transform.GetChild(i - 1).gameObject.SetActive(false);
            }

        }



    }

    public void RemoveStock()
    {
        if (currentStockCount <= 0) return;
        
        transform.GetChild(currentStockCount - 1).gameObject.SetActive(false);
        currentStockCount--;
    }
}
