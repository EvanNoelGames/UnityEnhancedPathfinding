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
    [SerializeField] private GameObject hiddenPlane;
    [Header("Colors")]
    [SerializeField] private Color colorBlock = Color.gray;
    [SerializeField] private Color colorMoney =  Color.yellow;
    [SerializeField] private Color colorNone =  Color.red;
    [SerializeField] private Color colorHidden =  Color.blue;
    [Header("Values")]
    [SerializeField] private float moneyInterval = 2.0f;

    public int value = 2;
    
    private RTSGrid grid;
    
    private Vector2Int gridPosition;
    private TileType tileType = TileType.None;
    private GameObject owner;
    public GameObject plannedOwner;

    private bool isHidden = true;

    public event Action<RTSTile> MoneyGet = delegate { };
    public event Action<RTSTile, EvanTestAgent> TileEntered = delegate { };

    private Color startingColor;

    RaycastHit2D hit;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        EvanTestAgent agentScript = other.GetComponentInParent<EvanTestAgent>();
        TileEntered.Invoke(this, agentScript);
        if (agentScript)
            SetOwner(agentScript.gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        EvanTestAgent agentScript = other.GetComponentInParent<EvanTestAgent>();
        if (agentScript.gameObject == owner)
            ResetOwner();
    }

    public void SetGrid(RTSGrid grid)
    {
        this.grid = grid;
    }
    
    #region Tile Type
    public bool GetIsHidden()
    {
        return isHidden;
    }
    
    public void DiscoverTile()
    {
        isHidden = false;
        hiddenPlane.SetActive(false);
    }
    
    public void SetTileType(TileType type)
    {
        tileType = type;

        if (isHidden)
        {
            hiddenPlane.GetComponent<MeshRenderer>().material.color = colorHidden;
        }
        else
        {
            DiscoverTile();
        }
        
        switch (type)
        {
            case TileType.Blocked:
                plane.GetComponent<MeshRenderer>().material.color = colorBlock;
                startingColor = colorBlock;
                break;
            case TileType.Money:
                plane.GetComponent<MeshRenderer>().material.color = colorMoney;
                startingColor = colorMoney;
                break;
            case TileType.None:
                plane.GetComponent<MeshRenderer>().material.color = colorNone;
                startingColor = colorNone;
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
        if (owner) return;
        
        owner = newOwner;
        
        if (owner == plannedOwner) plannedOwner = null;
        
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
        if (tileType == TileType.Blocked) return;
        if (plannedOwner) return;

        Color newColor = startingColor;
        newColor.r -= 0.3f;
        newColor.g -= 0.3f;
        newColor.b -= 0.3f;
        plane.GetComponent<MeshRenderer>().material.color = newColor;
        
        grid.SetNewTileHovered(this);
    }
    
    private void OnMouseExit()
    {
        plane.GetComponent<MeshRenderer>().material.color = startingColor;
        
        grid.ClearTile();
    }
    #endregion
}
