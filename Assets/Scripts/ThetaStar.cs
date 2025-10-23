using System.Collections.Generic;
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
                        TilePrioritized neighborPrioritized = new TilePrioritized();
                        neighborPrioritized.GCost = float.MaxValue;
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
        if (LineOfSight(current.Parent, neighbor))
        {
            
        }
        return  new List<Tile>();
    }

    bool LineOfSight(Tile from, Tile to)
    {
        Vector2 startPos = from.gridPosition;
        Vector2 endPos = to.gridPosition;
        
        // calculate the difference
        var dx = endPos.x - startPos.x;
        var dy = endPos.y - startPos.y;
        
        var steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
        
        var xInc = dx / steps;
        var yInc = dy / steps;

        var currentX = startPos.x;
        var currentY = startPos.y;

        for (int i = 0; i < steps; ++i)
        {
            var gridX = Mathf.Round(currentX);
            var gridY = Mathf.Round(currentY);
            
            Tile tile = _grid.GetTileAtPosition(new Vector2(gridX, gridY));
            if (tile != null && tile != from && tile != to)
            {
                if (tile.GetFill())
                {
                    return false;
                }
                
         
            }
            currentX += xInc;
            currentY += yInc;
        }
        return true;
    }
}
