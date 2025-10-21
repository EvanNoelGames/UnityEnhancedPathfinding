using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameObject plane;
    public TextMeshPro textMesh;

    public Vector2 gridPosition;
    
    private float weight = 1.0f;
    private Material planeMaterial;
    
    void Start()
    {
        planeMaterial = plane.GetComponent<MeshRenderer>().material;
    }
    public void SetWeight(float newWeight)
    {
        weight = newWeight;
    }

    public float GetWeight()
    {
        return weight;
    }

    public void Reset()
    {
        planeMaterial.color = Color.white;
    }

    public void SetStart()
    {
        planeMaterial.color = Color.green;
    }

    public void SetExit()
    {
        planeMaterial.color = Color.red;
    }

    public bool GetExit()
    {
        return planeMaterial.color == Color.red;
    }
    
    public void SetFill(bool fill)
    {
        if (fill)
        {
            planeMaterial.color = Color.gray;
        }
        else
        {
            Reset();
        }
    }

    public void SetPath()
    {
        planeMaterial.color = Color.blue;
    }

    public bool GetFill()
    {
        return planeMaterial.color == Color.gray;
    }
}
