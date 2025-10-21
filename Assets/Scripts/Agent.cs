using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private GameObject plane;
    
    public Grid grid;
    
    public Material pathMaterial;
    public Material baseAgentMaterial;
    public bool isPathFinding = true;
    
    public List<Vector3> path =  new List<Vector3>();
    public List<Tile> tilePath = new List<Tile>();
    public Tile targetTile = null;
    public Tile currentTile = null;
    private Vector3 targetPosition;
    private bool walking = false;

    public void UpdatePath(List<Tile> newPath)
    {
        path.Clear();
        tilePath.Clear();
        foreach (Tile tile in newPath)
        {
            path.Add(new Vector3(tile.gameObject.transform.position.x, tile.gameObject.transform.position.y, -3));
        }
        
        tilePath = new List<Tile>(newPath);
        currentTile = tilePath.First();
        targetPosition = path.First();
        walking = true;
    }
    
    public void BeginWalk(List<Tile> newPath)
    {
        // Set material
        if (isPathFinding)
        {
            plane.GetComponent<MeshRenderer>().material = pathMaterial;
        }
        else
        {
            plane.GetComponent<MeshRenderer>().material = baseAgentMaterial;
        }
        
        path.Clear();
        tilePath.Clear();
        foreach (Tile tile in newPath)
        {
            path.Add(new Vector3(tile.gameObject.transform.position.x, tile.gameObject.transform.position.y, -3));
        }
        
        tilePath = new List<Tile>(newPath);
        currentTile = tilePath.First();
        targetPosition = path.First();
        walking = true;
    }

    void FixedUpdate()
    {
        if (walking)
            Walk();
    }

    void Walk()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime);

        if ((transform.position - targetPosition).magnitude < 0.005)
        {
            path.RemoveAt(0);
            currentTile = tilePath.First();
            tilePath.RemoveAt(0);
            
            if (path.Count == 0)
            {
                walking = false;
                transform.position = targetPosition;
                return;
            }

            targetPosition = path.First();
            targetTile = tilePath.First();
            
            // Let the grid know we changed positions
            if (isPathFinding)
            {
                grid.PathfindingAgentUpdatedPosition();
                grid.SetVisiblePath(tilePath);
            }
            else
            {
                if (!grid.PathFinderAlive())
                    grid.SetVisiblePath(tilePath);
            }
            
            grid.AgentUpdatedPosition();
        }
    }
}
