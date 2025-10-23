using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Utils;

public class ThetaStar 
{

    struct TilePrioritized
    {
        public Tile Tile;
        public float GCost;
        public float HCost;
        public Tile Parent;
    }
    
    // find the distance from start to the goal
    float Heuristic(Vector2 p1, Vector2 p2)
    {
        return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y);
    }
    
    private PriorityQueue<TilePrioritized, float> _open;
    private HashSet<Tile> _closed;
    private HashSet<Tile> _openSet;
    private List<Tile> _reconstructedPath;
    private Dictionary<Tile, float> _bestGCost; 
    private Dictionary<Tile, float> _bestHCost;
    private Dictionary<Tile, Tile> _parents;
    Grid _grid;
    
    List<Tile> FindPath(Tile start, Tile goal)
    {
        _open =  new PriorityQueue<TilePrioritized, float>();
        _openSet = new HashSet<Tile>();
        _closed = new HashSet<Tile>();
        _parents = new Dictionary<Tile, Tile>();
        
        TilePrioritized startTile = new TilePrioritized();
        startTile.Tile = start;
        startTile.GCost = 0;
        startTile.Parent = start;
        
        _open.Enqueue( startTile, startTile.GCost + Heuristic(start.gridPosition, goal.gridPosition));
        
        while (_open.Count != 0)
        {
            TilePrioritized current = _open.Dequeue();

            if (current.Tile == goal)
            {
                return ReconstructedPath(start, goal);
            }
            
            _closed.Add(current.Tile);
            
            List<Tile> currentNeighbors = _grid.FindNeighbors(current.Tile);

            foreach (var neighbor in currentNeighbors)
            {
                if (!_closed.Contains(neighbor))
                {
                    if (!_openSet.Contains(neighbor))
                    {
                        TilePrioritized neighborPriortized = new TilePrioritized();
                        neighborPriortized.GCost = float.MaxValue;
                        _parents[neighbor] = null;
                    }
                    UpdateVertex(current, neighbor);
                }
            }
        }
        return null;
    }

    List<Tile> ReconstructedPath (Tile start, Tile goal)
    {
        List<Tile> path = new List<Tile>();

        Tile currentTile = goal;

        while (currentTile != start)
        {
            path.Add(currentTile);
            currentTile = _parents[currentTile];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }

    List<Tile> UpdateVertex(TilePrioritized current, Tile neighbor)
    {
        if (lineOfSight(current.Parent, neighbor))
        {
            
        }
        return  new List<Tile>();
    }

    bool lineOfSight(Tile from, Tile to)
    {
        
        
        return false;
    }
}
