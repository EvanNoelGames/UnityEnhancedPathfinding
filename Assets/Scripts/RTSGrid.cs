using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSGrid : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int xSize = 9;
    [SerializeField] private int ySize = 9;
    [SerializeField] private float tileDistance = 0.8f;
    [Header("Prefabs")]
    [SerializeField] private GameObject tilePrefab;
    [Header("Tiles")]
    [SerializeField] private List<Vector2Int> moneyTiles =  new List<Vector2Int>();
    
    private Dictionary<Vector2Int, RTSTile> tiles = new Dictionary<Vector2Int, RTSTile>();

    public event Action<RTSTile> NewTileHovered = delegate { }; 
    public event Action TileExited = delegate { }; 
    public event Action MoneyTileEarned = delegate { }; 
    
    void Start()
    {
        SetupBoard();
    }

    public void SetupBoard()
    {
        ClearBoard();
        PlaceTiles();
    }

    void PlaceTiles()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                // Create new tile
                GameObject newTile = Instantiate(tilePrefab);
                RTSTile newTileComponent = newTile.GetComponent<RTSTile>();
                Vector2Int newTileGridPosition = new Vector2Int(x, y);
                
                // Place money tiles
                if (moneyTiles.Contains(new Vector2Int(x, y)))
                {
                    newTileComponent.SetTileType(RTSTile.TileType.Money);
                }

                // Add to dictionary of tiles
                tiles.Add(newTileGridPosition, newTileComponent);

                // Setup position
                newTile.transform.parent = transform;
                newTile.transform.localPosition = new Vector3(x * tileDistance, y * tileDistance, 1);
                float xMod = 0;
                if (xSize % 2 == 0) xMod = -0.5f;
                float yMod = 0;
                if (ySize % 2 == 0) yMod = -0.5f;
                newTile.transform.localPosition -= new Vector3(((xSize / 2) + xMod) * tileDistance,
                    ((ySize / 2) + yMod) * tileDistance, 0);

                // Give the tile its grid position
                newTileComponent.SetGridPosition(newTileGridPosition);
                
                // Set its grid reference
                newTileComponent.SetGrid(this);

                // Give each tile a name
                newTile.name = "Tile " + x + ", " + y;

                newTileComponent.MoneyGet += MoneyTileMade;
            }
        }
    }

    void ClearBoard()
    {
        if (tiles.Count == 0) return;
        
        foreach (var items in tiles)
        {
            Destroy(items.Value.gameObject);
        }
        
        tiles.Clear();
    }

    public void SetNewTileHovered(RTSTile tile)
    {
        NewTileHovered.Invoke(tile);
    }
    
    public void ClearTile()
    {
        TileExited.Invoke();
    }

    private void MoneyTileMade()
    {
        MoneyTileEarned.Invoke();
    }
}
