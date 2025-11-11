using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;


public class AStar 
{
    struct TilePrioritized
    {
        public Tile Tile;
        public float GCost;
        public float HCost;
        public Tile Parent;
    }
    
    private PriorityQueue<TilePrioritized, float> _frontier;
    private Dictionary<Tile, Tile> _cameFrom;
    private Dictionary<Tile, float> _costSoFar; 
    
    List<Tile> _neighbors;
    
    Grid _grid;
    
    Tile _start;
    Tile _goal;
    TilePrioritized _current;

    private TilePrioritized _ptile; 
    
    // find the distance from start to the goal
    float Heuristic(Vector2 p1, Vector2 p2)
    {
        return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y);
    }

    public List<Tile> FindPath(Tile start, Tile goal, Grid grid)
    {
        // initialise the _priorityQueue
        _frontier = new PriorityQueue<TilePrioritized, float>();
        _cameFrom = new Dictionary<Tile, Tile>();
        _costSoFar = new Dictionary<Tile, float>();
        _neighbors = new List<Tile>();
        
        _costSoFar.Add(start, 0);
        
        _ptile = new TilePrioritized();
        _ptile.Tile = start;
        _ptile.GCost = 0;
        _ptile.HCost = Heuristic(grid.GetTilePosition(start), grid.GetTilePosition(goal));
        
        _frontier.Enqueue(_ptile, 0);
        
        while (_frontier.Count != 0)
        {
            _current = _frontier.Dequeue();
    
            _neighbors = grid.FindNeighbors(_current.Tile);

            if (_current.Tile == goal)
            {
                break;
            }
            
            // loop through all posible neighbors of the current tile calculating cost and setting the priority 
            foreach(var next in _neighbors)
            {
               
                float newcost = _costSoFar[_current.Tile] + 1;

                if (!_costSoFar.ContainsKey(next) || newcost < _costSoFar[next])
                {
                    TilePrioritized newptile = new TilePrioritized();
                    newptile.Tile = next;
                    newptile.GCost = newcost;
                    newptile.HCost = Heuristic(grid.GetTilePosition(goal), grid.GetTilePosition(next));
                    newptile.Parent = _current.Tile;
                    
                    _costSoFar[next] = newcost;
                    float priority =  newptile.GCost + newptile.HCost;
                    _frontier.Enqueue(newptile, priority);
                    _cameFrom[next] = _current.Tile;
                }
            }
        }

        if (_current.Tile != goal)
        {
            return new List<Tile>();
        }
        
        
        List<Tile> path = new List<Tile>();

        Tile currentTile = _current.Tile;

        while (currentTile != start)
        {
            path.Add(currentTile);
            currentTile = _cameFrom[currentTile];
        }

        path.Add(start);
        path.Reverse();
        return path;
        
    }

    public float GetDistance()
    {
        return _current.GCost;
    }
}
