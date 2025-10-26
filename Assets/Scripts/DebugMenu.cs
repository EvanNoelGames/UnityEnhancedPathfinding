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
    public Slider xSlider;
    public Slider ySlider;
    public Button runButton;
    public Toggle useTheta;

    public PlaceType selectedPlaceType;

    private void Awake()
    {
        instance = this;

        xSlider.onValueChanged.AddListener(XSizeChanged);
        ySlider.onValueChanged.AddListener(YSizeChanged);
    }

    public void ResetButton()
    {
        grid.SetupBoard();
        
        // User can change values again
        placeDropdown.interactable = true;
        runButton.interactable = true;
        useTheta.interactable = true;
        grid.lineRenderer.positionCount = 0;
    }

    public void RunButton()
    {
        // Exit if simulation cannot run
        if (!grid.RunSimulation()) return;

        // Prevent user from changing values while simulation running
        placeDropdown.value = 0;
        placeDropdown.interactable = false;
        runButton.interactable = false;
        useTheta.interactable = false;
    }

    public void PlaceTypeChanged()
    {
        selectedPlaceType = (PlaceType)placeDropdown.value;
    }

    public void XSizeChanged(Single value)
    {
        grid.nextXSize = (int)value;
    }
    
    public void YSizeChanged(Single value)
    {
        grid.nextYSize = (int)value;
    }

    public void UseThetaChanged()
    {
        grid.useTheta = useTheta.isOn;
    }
}
