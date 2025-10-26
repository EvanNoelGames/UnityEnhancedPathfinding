using System;
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

    public void Setup()
    {
        // Set material
        if (isPathFinding)
        {
            plane.GetComponent<MeshRenderer>().material = pathMaterial;
            GetComponent<BoxCollider2D>().isTrigger = false;
        }
        else
        {
            plane.GetComponent<MeshRenderer>().material = baseAgentMaterial;
        }
    }
    
    public void BeginWalk(List<Tile> newPath)
    {
        Setup();
        UpdatePath(newPath);
    }

    void FixedUpdate()
    {
        if (walking)
            Walk();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        grid.AgentsColliding();
    }

    public void ForceEndWalk()
    {
        walking = false;
        path.Clear();
    }

    public bool IsWalking()
    {
        return walking;
    }

    void Walk()
    {
        // Move toward target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime);

        // We're really close to the target
        if ((transform.position - targetPosition).magnitude < 0.005)
        {
            // Early return if path is modified
            if (path.Count == 0)
                return;
            
            // Pop off top of path
            path.RemoveAt(0);
            currentTile = tilePath.First();
            tilePath.RemoveAt(0);
            
            // Path is over
            if (path.Count == 0)
            {
                walking = false;
                transform.position = targetPosition;
                return;
            }

            // Get new target
            targetPosition = path.First();
            targetTile = tilePath.First();
            
            // Let the grid know we changed positions
            if (isPathFinding)
            {
                grid.PathfindingAgentUpdatedPosition();
                if (!grid.useTheta)
                    grid.SetVisiblePath(tilePath);
                else
                    grid.UpdateLine(tilePath);
            }
            else
            {
                // If the path finder is inactive, agent draws the path instead
                if (!grid.PathFinderActive() && !grid.useTheta)
                    grid.SetVisiblePath(tilePath);
                else if (!grid.PathFinderActive() && grid.useTheta)
                    grid.UpdateLine(tilePath);
            }
        }
    }
}
