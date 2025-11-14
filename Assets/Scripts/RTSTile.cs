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
    [SerializeField] private float moneyInterval = 2.0f;

    public int value = 2;
    
    private RTSGrid grid;
    
    private Vector2Int gridPosition;
    private TileType tileType = TileType.None;
    private GameObject owner;

    public event Action<RTSTile> MoneyGet = delegate { };

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("AgentPlane"))
            SetOwner(other.gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == owner)
            ResetOwner();
    }

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
    public Vector2Int GetCurrentTilePosition()
    {
        return gridPosition;
    }
    #endregion
    
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
        MoneyGet.Invoke(this);
        
        if (owner)
            StartCoroutine(CoroutineMoneyTimer(moneyInterval));
    }
    #endregion
    
    #region Owner
    private void SetOwner(GameObject newOwner)
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

    private void ResetOwner()
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
