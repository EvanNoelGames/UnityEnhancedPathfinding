using TMPro;
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
    [SerializeField] private Color colorMoney =  Color.green;
    [SerializeField] private Color colorNone =  Color.red;

    public int value = 3;
    
    private Vector2Int gridPosition;
    private Material planeMaterial;
    private TileType tileType = TileType.None;
    private GameObject owner;
    
    void Start()
    {
        planeMaterial = plane.GetComponent<MeshRenderer>().material;
    }
    
    #region Tile Type
    public void SetTileType(TileType type)
    {
        tileType = type;

        switch (type)
        {
            case TileType.Blocked:
                planeMaterial.color = colorBlock;
                break;
            case TileType.Money:
                planeMaterial.color = colorMoney;
                break;
            case TileType.None:
                planeMaterial.color = colorNone;
                break;
        }
    }
    
    public TileType GetTileType()
    {
        return tileType;
    }
    
    public void ResetTileType()
    {
        planeMaterial.color = colorNone;
        tileType = TileType.None;
    }
    #endregion
    
    #region Owner
    public void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
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
}
