using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private String gameScene;

    public void StartButtonPressed()
    {
        SceneManager.LoadScene(gameScene);
    }
}
