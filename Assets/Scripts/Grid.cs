using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    private Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();
    
    AStar astar = new AStar();

    private Tile startTile;
    private Tile exitTile;

    public GameObject tilePrefab;
    
    public float tileDistance = 0.8f;
    public int xSize = 9;
    public int ySize = 9;

    public int nextXSize = 9;
    public int nextYSize = 9;
    
    // Start is called before the first frame update
    void Start()
    {
        SetupBoard();
    }

    void Update()
    {
        CheckForClick();
    }

    void ClickedTile(GameObject clickedTile)
    {
        Tile tile = clickedTile.GetComponent<Tile>();
        
        // Do different things depending on the placement mode
        switch (DebugMenu.instance.selectedPlaceType)
        {
            case DebugMenu.PlaceType.ENTRANCE:
                // Place entrance
                if (startTile != null)
                    startTile.Reset();
                tile.SetStart();
                startTile = tile;
                break;
            case DebugMenu.PlaceType.EXIT:
                // Place exit
                if (exitTile != null)
                    exitTile.Reset();
                tile.SetExit();
                exitTile = tile;
                break;
            case DebugMenu.PlaceType.FILL:
                // Toggle tile fill
                tile.SetFill(!tile.GetFill());
                break;
            case DebugMenu.PlaceType.AGENT:
                // Implement
                break;
        }
    }

    void CheckForClick()
    {
        // Left click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Tile"))
                {
                    ClickedTile(hit.collider.gameObject);
                }
            }
        }
    }

   public  Vector2 GetTilePosition(Tile tile)
    {
        return tile.gridPosition;
    }
    
    public List<Tile> FindNeighbors(Tile tile)
    {
        // get the current tile pos
        Vector2 currentPos = GetTilePosition(tile);
        
        // add all the potential offsets to the vector or just loop through them 
        List<Tile> neighbors = new List<Tile>();
        
        for (int x = -1; x <= 1; ++x)
        {
            for (int y = -1; y <= 1; ++y)
            {
                if (x == 0 && y == 0)
                    continue; 
                
                // make the neighbor point
                Vector2 neighborPos = new Vector2(currentPos.x + x, currentPos.y + y);
               
                // check if its in the grid list which means that its a valid tile
                if (tiles.TryGetValue(neighborPos,  out Tile neighborTile) &&  !neighborTile.GetFill())
                {
                    neighbors.Add(neighborTile);
                }
            }
        }
        return neighbors;
    }

    #region SETUP
    public void SetupBoard()
    {
        ClearBoard();
        PlaceTiles();
    }

    public void RunSimulation()
    {

        if (startTile == null || exitTile == null)
        {
            Debug.LogWarning("Start and Exit Tile arent set!");
            return;
        }
        
        List<Tile> path = astar.FindPath(startTile, exitTile, this);

        if (path.Count == 0)
        {
            Debug.Log("No path found!");
            return;
        }

        foreach (var tile in path)
        {
            tile.SetPath();
        }
    }

    void PlaceTiles()
    {
        xSize = nextXSize;
        ySize = nextYSize;
        
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                // Create new tile
                GameObject newTile = Instantiate(tilePrefab);
                Tile newTileComponent = newTile.GetComponent<Tile>();
                Vector2 newTileGridPosition = new Vector2(x, y);
                
                // Add to dictionary of tiles
                tiles.Add(newTileGridPosition, newTileComponent);
                
                // Setup position
                newTile.transform.parent = transform;
                newTile.transform.localPosition = new Vector3(x * tileDistance, y * tileDistance, 0);
                float xMod = 0;
                if (xSize % 2 == 0) xMod = -0.5f;
                float yMod = 0;
                if (ySize % 2 == 0) yMod = -0.5f;
                newTile.transform.localPosition -= new Vector3(((xSize / 2) + xMod) * tileDistance, ((ySize / 2) + yMod) * tileDistance, 0);
                
                // Give the tile its grid position
                newTileComponent.gridPosition = newTileGridPosition;
                
                // Give each tile a name
                newTile.name = "Tile " + x + ", " + y;
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
    #endregion
}
