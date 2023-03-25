using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stocks : MonoBehaviour
{
    private int currentStockCount;
    private int paddingWhenAlone = 45;
    private int paddingIncrease = 6;
    HorizontalLayoutGroup group;

    private void Start()
    {
        group = this.GetComponent<HorizontalLayoutGroup>();
    }


    public void SetStartStock()
    {
        if(DeathManager.Instance.stocks == false)
        {
            currentStockCount = 0;

            int maxNumberOfPossibleStocks = DeathManager.Instance.GetMaxNumberOfStocks();
            for (int i = 0; i < maxNumberOfPossibleStocks; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            int maxNumberOfPossibleStocks = DeathManager.Instance.GetMaxNumberOfStocks();
            currentStockCount = DeathManager.Instance.numberOfStocks;
            for (int i = maxNumberOfPossibleStocks; i > currentStockCount; i--)
            {
                group.padding.left += paddingIncrease;
                transform.GetChild(i - 1).gameObject.SetActive(false);
            }
            if(currentStockCount == 1)
            {
                group.padding.left = paddingWhenAlone;
            }
        }


        
    }

    public void RemoveStock()
    {
        if (currentStockCount <= 0) return;

        group.padding.left += paddingIncrease;
        
        transform.GetChild(currentStockCount - 1).gameObject.SetActive(false);
        currentStockCount--;

        if(currentStockCount == 1)
        {
            group.padding.left = paddingWhenAlone;
        }
    }
}
