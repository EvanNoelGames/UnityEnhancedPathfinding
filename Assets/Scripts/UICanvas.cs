using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UICanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    
    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void SetMoneyText(int newValue)
    {
        moneyText.text = "$" + newValue.ToString();
    }
}
