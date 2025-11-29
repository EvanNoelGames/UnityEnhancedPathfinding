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
    [SerializeField] private List<Vector2Int> blockedTiles =  new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> discoveredTiles =  new List<Vector2Int>();
    
    private Dictionary<Vector2Int, RTSTile> tiles = new Dictionary<Vector2Int, RTSTile>();

    public event Action<RTSTile> NewTileHovered = delegate { }; 
    public event Action TileExited = delegate { }; 
    public event Action<RTSTile> MoneyTileEarned = delegate { }; 
    
    void Start()
    {
        SetupBoard();
    }

    #region SETUP
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
                
                newTileComponent.SetTileType(RTSTile.TileType.None);
                
                // Set discovered tiles
                if (discoveredTiles.Contains(new Vector2Int(x, y)))
                {
                    newTileComponent.DiscoverTile();
                }
                
                // Place money tiles
                if (moneyTiles.Contains(new Vector2Int(x, y)))
                {
                    newTileComponent.SetTileType(RTSTile.TileType.Money);
                }
                
                // Place blocked tiles
                if (blockedTiles.Contains(new Vector2Int(x, y)))
                {
                    newTileComponent.SetTileType(RTSTile.TileType.Blocked);
                }

                // Add to dictionary of tiles
                tiles.Add(newTileGridPosition, newTileComponent);
                
                // Link to function
                newTileComponent.TileEntered += TileEntered;

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
    #endregion

    #region GETTERS
    public List<Vector2Int> GetMoneyTiles(bool isFriendly)
    {
        List<Vector2Int> emptyMoneyTiles = new List<Vector2Int>();

        foreach (var tile in moneyTiles)
        {
            if (!GetTileAtPosition(tile).HasOwner() || GetTileAtPosition(tile).GetOwner().GetComponent<EvanTestAgent>().GetIsFriendly() == isFriendly)
            {
                emptyMoneyTiles.Add(tile);
            }
        }
        
        return emptyMoneyTiles;
    }
    
    public List<RTSTile> GetTiles()
    {
        List<RTSTile> allTiles = new List<RTSTile>();
        
        foreach (var tile in tiles)
        {
            allTiles.Add(tile.Value);
        }

        return allTiles;
    }
    
    public List<RTSTile> GetEmptyTiles()
    {
        List<RTSTile> allTiles = new List<RTSTile>();
        
        foreach (var tile in tiles)
        {
            RTSTile currentTile = tile.Value;

            if (!currentTile.HasOwner() && currentTile.GetTileType() != RTSTile.TileType.Blocked)
            {
                allTiles.Add(currentTile);
            }
        }

        return allTiles;
    }
    
    public RTSTile GetTileAtPosition(Vector2Int position)
    {
        return tiles.GetValueOrDefault(position);
    }
    #endregion
    
    void ClearBoard()
    {
        if (tiles.Count == 0) return;
        
        foreach (var items in tiles)
        {
            Destroy(items.Value.gameObject);
        }
        
        tiles.Clear();
    }
    
    public List<RTSTile> FindNeighbors(RTSTile tile, bool includeBlocked = true)
    {
        // get the current tile pos
        Vector2Int currentPos = tile.GetGridPosition();
        
        // add all the potential offsets to the vector or just loop through them 
        List<RTSTile> neighbors = new List<RTSTile>();
        
        for (int x = -1; x <= 1; ++x)
        {
            for (int y = -1; y <= 1; ++y)
            {
                if (x == 0 && y == 0)
                    continue; 
                
                // make the neighbor point
                Vector2Int neighborPos = new Vector2Int(currentPos.x + x, currentPos.y + y);
               
                // check if its in the grid list which means that its a valid tile
                if (!includeBlocked)
                {
                    if (tiles.TryGetValue(neighborPos,  out RTSTile neighborTile) &&  (neighborTile.GetTileType() != RTSTile.TileType.Blocked))
                    {
                        neighbors.Add(neighborTile);
                    }
                }
                else
                {
                    if (tiles.TryGetValue(neighborPos,  out RTSTile neighborTile))
                    {
                        neighbors.Add(neighborTile);
                    }
                }
            }
        }
        return neighbors;
    }

    public void SetNewTileHovered(RTSTile tile)
    {
        NewTileHovered.Invoke(tile);
    }
    
    public void ClearTile()
    {
        TileExited.Invoke();
    }

    private void TileEntered(RTSTile tile, EvanTestAgent agent)
    {
        if (!agent.GetIsFriendly()) return;

        foreach (var tiles in FindNeighbors(tile, true))
        {
            tiles.DiscoverTile();
        }
    }

    private void MoneyTileMade(RTSTile tile)
    {
        MoneyTileEarned.Invoke(tile);
    }
}
