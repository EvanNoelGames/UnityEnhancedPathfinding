using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{
    static public DebugMenu instance;
    
    public enum PlaceType
    {
        FILL,
        EXIT,
        ENTRANCE,
        AGENT
    }
    
    public Grid grid;
    public TMP_Dropdown placeDropdown;

    public PlaceType selectedPlaceType;

    private void Awake()
    {
        instance = this;
    }

    public void ResetButton()
    {
        grid.SetupBoard();
    }

    public void RunButton()
    {
        grid.RunSimulation();
    }

    public void PlaceTypeChanged()
    {
        selectedPlaceType = (PlaceType)placeDropdown.value;
    }
}
