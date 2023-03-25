using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RespawnTimer : MonoBehaviour
{
    float respawnDelay;
    TextMeshProUGUI countdownText;
    TextMeshProUGUI nameText;
    private void Start()
    {
        respawnDelay = DeathManager.Instance.respawnDelay;
        countdownText = this.GetComponent<TextMeshProUGUI>();
        nameText = this.transform.parent.GetComponent<TextMeshProUGUI>();
        print(nameText.text);

        countdownText.color = new Vector4(countdownText.color.r, countdownText.color.g, countdownText.color.b, 0f);

    }
    public void StartTimer()
    {
        print("HelloClientUITIMER");

        StartCoroutine(TimerCoroutine());
    }

    private IEnumerator TimerCoroutine()
    {
        // Show the countdown text
        nameText.color = new Vector4(nameText.color.r, nameText.color.g, nameText.color.b, 0.2f);
        countdownText.color = new Vector4(countdownText.color.r, countdownText.color.g, countdownText.color.b, 255f);


        // Wait for the respawn delay
        float remainingTime = respawnDelay;
        while (remainingTime > -1f)
        {
            if(remainingTime <= 0f)
            {
                countdownText.text = "GO";
            }
            else
            {
                countdownText.text = $"{remainingTime:0}";
            }
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }


        nameText.color = new Vector4(nameText.color.r, nameText.color.g, nameText.color.b, 255f);
        countdownText.color = new Vector4(countdownText.color.r, countdownText.color.g, countdownText.color.b, 0f);
        // Hide the countdown text and respawn the player
    }
}
