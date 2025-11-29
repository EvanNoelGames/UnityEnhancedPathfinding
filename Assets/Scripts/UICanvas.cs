using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICanvas : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI agentCostText;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Canvas fadeCanvas;
    [Header("Values")]
    [SerializeField] private string playerLostMessage = "Player Lost!";
    [SerializeField] private string playerWonMessage = "Player Won!";
    
    public event Action RestartGame = delegate { };

    public void RestartButton()
    {
        RestartGame.Invoke();
    }
    
    public void SetMoneyText(int newValue)
    {
        moneyText.text = "$" + newValue.ToString();
    }
    
    public void SetAgentCostText(int newValue)
    {
        agentCostText.text = "$" + newValue.ToString() + " per agent";
    }


    public void GameOver(bool playerWon)
    {
        if (playerWon)
        {
            gameOverText.SetText(playerLostMessage);
        }
        else
        {
            gameOverText.SetText(playerWonMessage);
        }
        
        restartButton.gameObject.SetActive(true);
        fadeCanvas.gameObject.SetActive(true);
    }
}
