using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Grid : MonoBehaviour
{
    private Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();
    
    AStar astar = new AStar();

    // Used for when the user is placing tile
    private GameObject lastClickedTile;
    private enum FillingType
    {
        NONE,
        FILLING,
        CLEARING
    }
    private FillingType fillingTileType = FillingType.NONE;
    
    private List<Tile> visiblePath = new List<Tile>();
    
    private Tile startTile;
    private Tile exitTile;
    private GameObject agent;
    private Agent agentComponent; // Spawns where user selects
    private GameObject pathAgent;
    private Agent pathAgentComponent; // Spawns at starting tile
    private Tile agentTile;
    
    public GameObject tilePrefab;
    public GameObject agentPrefab;
    
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

        // Ensure user doesn't place tile on top of agents
        if (agent != null)
        {
            if (agentComponent.targetTile == tile || agentComponent.currentTile == tile)
            {
                print("Cannot place tile on agent!");
                return;
            }
        }
        if (pathAgent != null)
        {
            if (pathAgentComponent.targetTile == tile || pathAgentComponent.currentTile == tile)
            {
                print("Cannot place tile on agent!");
                return;
            }
        }
        
        // Do different things depending on the placement mode
        switch (DebugMenu.instance.selectedPlaceType)
        {
            case DebugMenu.PlaceType.ENTRANCE:
                // Place entrance
                // Destory previous entrance
                if (startTile != null)
                    startTile.Reset();
                // Destroy agent if on tile
                if (tile == agentTile)
                    Destroy(agent);
                // Reset exit if its the exit tile
                if (tile == exitTile)
                {
                    exitTile.Reset();
                    exitTile = null;
                }
                tile.SetStart();
                startTile = tile;
                break;
            case DebugMenu.PlaceType.EXIT:
                // Place exit
                // Destroy previous exit
                if (exitTile != null)
                    exitTile.Reset();
                // Destroy agent if on tile
                if (tile == agentTile)
                    Destroy(agent);
                // Reset start if its the start tile
                if (tile == startTile)
                {
                    startTile.Reset();
                    startTile = null;
                }
                tile.SetExit();
                exitTile = tile;
                break;
            case DebugMenu.PlaceType.FILL:
                // Destroy agent if on tile
                if (tile == agentTile)
                    Destroy(agent);
                // Reset exit if its the exit tile
                if (tile == exitTile)
                {
                    exitTile.Reset();
                    exitTile = null;
                }
                // Reset start if its the start tile
                if (tile == startTile)
                {
                    startTile.Reset();
                    startTile = null;
                }
                // Toggle tile fill
                tile.SetFill(!tile.GetFill());
                break;
            case DebugMenu.PlaceType.AGENT:
                if (agent != null)
                    Destroy(agent);
                // Reset exit if its the exit tile
                if (tile == exitTile)
                {
                    exitTile.Reset();
                    exitTile = null;
                }
                // Reset start if its the start tile
                if (tile == startTile)
                {
                    startTile.Reset();
                    startTile = null;
                }
                // Clear if filled
                if (tile.GetFill())
                    tile.Reset();
                CreateAgent(clickedTile);
                break;
        }
    }

    void CreateAgent(GameObject spawnTile)
    {
        agent = Instantiate(agentPrefab);
        agent.transform.position = new Vector3(spawnTile.transform.position.x, spawnTile.transform.position.y, -3);
        agentComponent = agent.GetComponent<Agent>();
        agentComponent.isPathFinding = false;
        agentComponent.grid = this;
        agentTile = spawnTile.GetComponent<Tile>();
    }

    void CheckForClick()
    {
        // Left click pressed not in fill mode
        if (DebugMenu.instance.selectedPlaceType != DebugMenu.PlaceType.FILL && Input.GetMouseButtonDown(0))
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
        // Left click held in fill mode
        else if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Tile") && lastClickedTile != hit.collider.gameObject)
                {
                    Tile clickedTileComponent = hit.collider.gameObject.GetComponent<Tile>();

                    // Set new fill type
                    if (fillingTileType == FillingType.NONE)
                    {
                        if (clickedTileComponent.GetFill())
                            fillingTileType = FillingType.CLEARING;
                        else
                            fillingTileType = FillingType.FILLING;
                    }
                    
                    // Tile type is the same
                    if (fillingTileType == FillingType.CLEARING && clickedTileComponent.GetFill() ||
                        fillingTileType == FillingType.FILLING && !clickedTileComponent.GetFill())
                    {
                        lastClickedTile = hit.collider.gameObject;
                        ClickedTile(hit.collider.gameObject);
                    }
                }
            }
        }
        else
        {
            fillingTileType = FillingType.NONE;
            lastClickedTile = null;
        }
    }

    public Tile GetTileAtPosition(Vector2 position)
    {
        return tiles.GetValueOrDefault(position);
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
        if (agent != null)
            Destroy(agent);
        if (pathAgent != null)
            Destroy(pathAgent);
        
        ClearBoard();
        PlaceTiles();
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
                newTile.transform.localPosition = new Vector3(x * tileDistance, y * tileDistance, 1);
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
    
    public bool RunSimulation()
    {
        if (startTile == null || exitTile == null)
        {
            Debug.LogWarning("Start and Exit Tile arent set!");
            return false;
        }
        
        List<Tile> path = astar.FindPath(startTile, exitTile, this);

        // Agent's path
        if (agent != null)
        {
            List<Tile> agentPath = astar.FindPath(agentTile, startTile, this);
            
            if (agentPath.Count == 0)
            {
                Debug.Log("No path found for agent!");
                return false;
            }
            
            agentComponent.BeginWalk(agentPath);
        }
        
        if (path.Count == 0)
        {
            Debug.Log("No path found!");
            return false;
        }
        
        // Spawn the pathing agent
        pathAgent = Instantiate(agentPrefab);
        pathAgent.transform.position = new Vector3(startTile.transform.position.x, startTile.transform.position.y, -3);
        pathAgentComponent = pathAgent.GetComponent<Agent>();
        pathAgentComponent.BeginWalk(path);
        pathAgentComponent.grid = this;
        startTile.Reset();

        SetVisiblePath(path);
        
        return true;
    }

    public void SetVisiblePath(List<Tile> path)
    {
        ClearVisiblePath();
        visiblePath = new List<Tile>(path);
        
        foreach (var tile in visiblePath)
        {
            if (!tile.GetExit())
                tile.SetPath();
        }
    }
    
    void ClearVisiblePath()
    {
        foreach (var tile in visiblePath)
        {
            if (!tile.GetFill() && !tile.GetExit())
                tile.Reset();
        }
        
        visiblePath.Clear();
    }

    public void PathfindingAgentUpdatedPosition()
    {
        if (agent != null)
        {
            Tile targetTile = pathAgentComponent.currentTile;
            // Shoot ahead if we can
            if (pathAgentComponent.targetTile != null)
                targetTile  = pathAgentComponent.targetTile;
            List<Tile> path = astar.FindPath(agentComponent.targetTile, targetTile, this);
            agentComponent.UpdatePath(path);
        }
    }

    public void AgentUpdatedPosition()
    {
        // Agents are touching
        if (pathAgent != null && agent != null && agentComponent.currentTile == pathAgentComponent.currentTile)
        {
            Destroy(pathAgent);
            List<Tile> path = astar.FindPath(agentComponent.targetTile, exitTile, this);
            agentComponent.UpdatePath(path);
        }
    }

    public bool PathFinderAlive()
    {
        return pathAgent != null;
    }
}
