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
                //return _parents[current.Tile];
            }
            
            _closed.Add(current.Tile);
            
            List<Tile> currentNeighbors = _grid.FindNeighbors(current.Tile);

            foreach (var neighbor in currentNeighbors)
            {
                if (!_closed.Contains(neighbor) && !_openSet.Contains(neighbor))
                {
                    float currentGCost = Heuristic(start.gridPosition, neighbor.gridPosition);
                    float neighborGCost = currentGCost + Heuristic(current.Tile.gridPosition, neighbor.gridPosition);
                    TilePrioritized neighborPriortized = new TilePrioritized();
                    neighborPriortized.Tile = neighbor;
                    neighborPriortized.GCost = neighborGCost;
                    _parents[neighbor] = current.Tile;
                    neighborPriortized.Parent = current.Tile;
                    
                    
                    _open.Enqueue(neighborPriortized, neighborGCost);
                    _openSet.Add(neighbor);
                }
            }
        }

        return null;
    }
}
