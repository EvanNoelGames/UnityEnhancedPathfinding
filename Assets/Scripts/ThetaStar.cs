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
    
    public List<Tile> FindPath(Tile start, Tile goal, Grid grid)
    {
        _open =  new PriorityQueue<TilePrioritized, float>();
        _openSet = new HashSet<Tile>();
        _closed = new HashSet<Tile>();
        _parents = new Dictionary<Tile, Tile>();
        _bestGCost = new Dictionary<Tile, float>();
        _grid = grid;
        
        TilePrioritized startTile = new TilePrioritized();
        startTile.Tile = start;
        startTile.GCost = 0;
        startTile.Parent = start;
        
        _bestGCost[start] = 0;
        
        _open.Enqueue( startTile, startTile.GCost + Heuristic(start.gridPosition, goal.gridPosition));
        _openSet.Add(startTile.Tile);
        
        while (_open.Count != 0)
        {
            TilePrioritized current = _open.Dequeue();
            
            // when the goal is reached return the constructed path
            if (current.Tile == goal)
            {
                return ReconstructedPath(start, goal);
            }
            
            _closed.Add(current.Tile);
            
            List<Tile> currentNeighbors = _grid.FindNeighbors(current.Tile);

            // for all of the neighbors of the current tile check that they havent been vistied and arent about to be visted 
            foreach (var neighbor in currentNeighbors)
            {
                if (!_closed.Contains(neighbor))
                {
                    TilePrioritized neighborPrioritized = new TilePrioritized();
                    neighborPrioritized.Tile = neighbor;
                    if (!_openSet.Contains(neighbor))
                    {
                        
                        neighborPrioritized.GCost = float.MaxValue;
                        _bestGCost[neighbor] = float.MaxValue;
                        _parents[neighbor] = null;
                        _openSet.Add(neighbor);
                    }
                    UpdateVertex(current, neighborPrioritized, goal);
                }
            }
        }
        return new List<Tile>();
    }

    // construct a path from the goal to the start 
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

    // Uses the line of Sight function and Heuristics to find the best possible path
    void UpdateVertex(TilePrioritized current, TilePrioritized neighbor, Tile goal)
    {
        
        float currentToNeighborCost = Heuristic(current.Tile.gridPosition, neighbor.Tile.gridPosition);
        float parentToNeighborCost = Heuristic(current.Parent.gridPosition, neighbor.Tile.gridPosition);
        
        // if line of sight is found from the parent to the neighbor  
        if (LineOfSight(current.Parent, neighbor.Tile))
        {
            // if the path from parent to neighbor is cheaper then the best currently know path
            if (_bestGCost[current.Parent] + parentToNeighborCost < _bestGCost[neighbor.Tile])
            {
                // update the neighbors parent with the current tiles parent and its cost
                neighbor.GCost = _bestGCost[current.Parent] + parentToNeighborCost;
                neighbor.Parent = current.Parent;
                
                _parents[neighbor.Tile] = neighbor.Parent;
                _bestGCost[neighbor.Tile] = neighbor.GCost;
                
                // removes the neighbor tile from open set so it can be correctly updated within the priority queue
                if (_openSet.Contains(neighbor.Tile))
                {
                    _openSet.Remove(neighbor.Tile);
                }

                float hCost = Heuristic(neighbor.Tile.gridPosition, goal.gridPosition);
                _open.Enqueue(neighbor, neighbor.GCost +  hCost);
            }
        }
        
        // if a cheaper path wasn't found use the standard A* pathfinding
        else if (_bestGCost[current.Tile] + currentToNeighborCost < _bestGCost[neighbor.Tile])
        {
            // update the neighbors parent with the current tiles parent and its cost
            neighbor.GCost = _bestGCost[current.Tile] + currentToNeighborCost;
            neighbor.Parent = current.Tile;
            
            _parents[neighbor.Tile] = neighbor.Parent;
            _bestGCost[neighbor.Tile] = neighbor.GCost;
            
            // removes the neighbor tile from open set so it can be correctly updated within the priority queue
            if (_openSet.Contains(neighbor.Tile))
            {
                _openSet.Remove(neighbor.Tile);
            }
            float hCost = Heuristic(neighbor.Tile.gridPosition, goal.gridPosition);
            _open.Enqueue(neighbor, neighbor.GCost +  hCost);
        }
    }

    bool LineOfSight(Tile from, Tile to)
    {
        Vector2 startPos = from.gridPosition;
        Vector2 endPos = to.gridPosition;
        
        // calculate the difference between the end and start positions
        var dx = endPos.x - startPos.x;
        var dy = endPos.y - startPos.y;
        
        // find the proper amount of steps to sample
        var steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
        
        // divide the differences of x and y by the amount of steps
        var xInc = dx / steps;
        var yInc = dy / steps;

        var currentX = startPos.x;
        var currentY = startPos.y;

        // loop through all the steps checking for any obstacle between from and to
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
