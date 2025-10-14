using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    private Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();

    private Tile startTile;
    private Tile exitTile;

    public GameObject tilePrefab;
    
    public float tileDistance = 0.8f;
    public int xSize = 9;
    public int ySize = 9;
    
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

    #region SETUP
    public void SetupBoard()
    {
        ClearBoard();
        PlaceTiles();
    }

    public void RunSimulation()
    {
        
    }

    void PlaceTiles()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                // Create new tile
                GameObject newTile = Instantiate(tilePrefab);
                tiles.Add(new Vector2(x, y), newTile.GetComponent<Tile>());
                
                // Setup position
                newTile.transform.parent = transform;
                newTile.transform.localPosition = new Vector3(x * tileDistance, y * tileDistance, 0);
                newTile.transform.localPosition -= new Vector3((xSize / 2) * tileDistance, (ySize / 2) * tileDistance, 0);
                
                
                
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
