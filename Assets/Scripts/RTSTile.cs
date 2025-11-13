using System;
using System.Collections;
using System.Timers;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RTSTile : MonoBehaviour
{
    public enum TileType
    {
        Blocked,
        Money,
        None
    }
    
    [Header("References")]
    [SerializeField] private GameObject plane;
    [Header("Colors")]
    [SerializeField] private Color colorBlock = Color.gray;
    [SerializeField] private Color colorMoney =  Color.yellow;
    [SerializeField] private Color colorNone =  Color.red;
    [Header("Values")]
    [SerializeField] private float moneyInterval = 1.0f;

    public int value = 3;
    
    private RTSGrid grid;
    
    private Vector2Int gridPosition;
    private TileType tileType = TileType.None;
    private GameObject owner;

    public event Action MoneyGet = delegate { };

    public void SetGrid(RTSGrid grid)
    {
        this.grid = grid;
    }
    
    #region Tile Type
    public void SetTileType(TileType type)
    {
        tileType = type;

        switch (type)
        {
            case TileType.Blocked:
                plane.GetComponent<MeshRenderer>().material.color = colorBlock;
                break;
            case TileType.Money:
                plane.GetComponent<MeshRenderer>().material.color = colorMoney;
                break;
            case TileType.None:
                plane.GetComponent<MeshRenderer>().material.color = colorNone;
                break;
        }
    }
    
    public TileType GetTileType()
    {
        return tileType;
    }
    
    public void ResetTileType()
    {
        plane.GetComponent<MeshRenderer>().material.color = colorNone;
        tileType = TileType.None;
    }
    #endregion
    
    //TODO
    #region Money

    private IEnumerator CoroutineMoneyTimer(float countdownTime)
    {
        // Ensure owner is the same once timer ends
        GameObject startingOwner = owner;
        
        yield return new WaitForSeconds(countdownTime);

        if (startingOwner == owner)
            TimerFinished();
    }
    
    private void TimerFinished()
    {
        MoneyGet.Invoke();
        
        if (owner)
            StartCoroutine(CoroutineMoneyTimer(moneyInterval));
    }
    #endregion
    
    #region Owner
    public void SetOwner(GameObject newOwner)
    {
        owner = newOwner;

        if (tileType == TileType.Money)
        {
            StartCoroutine(CoroutineMoneyTimer(moneyInterval));
        }
    }

    public GameObject GetOwner()
    {
        return owner;
    }

    public void ResetOwner()
    {
        owner = null;
    }

    public bool HasOwner()
    {
        return owner != null;
    }
    #endregion
    
    #region Grid Position
    public void SetGridPosition(Vector2Int newPosition)
    {
        gridPosition = newPosition;
    }

    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }
    #endregion
    
    #region Mouse
    private void OnMouseEnter()
    {
        grid.SetNewTileHovered(this);
    }
    
    private void OnMouseExit()
    {
        grid.ClearTile();
    }
    #endregion
}
