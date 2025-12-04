using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TacticalPath
{
   struct TilePrioritized
    {
        public RTSTile Tile;
        public float Distance;
        public float TacticalValue;
        public float ObjectiveWeight;
        public float DistanceToEnemy;
        public RTSTile Parent;
    }
    
    private AStar _aStar = new AStar();
    
    private TilePrioritized _ptile = new TilePrioritized();
    
    private RTSGrid _rtsGrid =  new RTSGrid();
    private RTSGame _game = new RTSGame();
    
    private EvanTestAgent _agent = new EvanTestAgent();
    
    private RTSTile _currentTile = new RTSTile();
    
    List<RTSTile> _path = new List<RTSTile>();
    float currentMinDistance = 18;

    private float distanceToEnemy = 0.0f;
    RTSTile closetTile = null;
    private float distance;
    private int agentPenalty = 0;

    private Dictionary<RTSTile, float> _usefulTiles; 
    // find the distance from point to the point
    float Heuristic(Vector2 p1, Vector2 p2)
    {
        return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y);
    }
    
    public List<RTSTile> FindBestPath(RTSTile start, RTSGrid grid, List<EvanTestAgent> agents, Dictionary<RTSTile, int> targetTiles)
    {
        /*
           * Identify multiple candidate goal tiles (tiles near enemies, near objectives, good tactical positions)
              Run A* to each candidate from your start position
              Evaluate each complete path's tactical value
              Pick the goal with the best value
           */
        agentPenalty = 0;
        
        _usefulTiles = new Dictionary<RTSTile, float>();
        
        // add all the money tile positions to the list 
        List<Vector2Int> moneyTileLocations = grid.GetMoneyTiles(false);
        List<RTSTile> playerLocations = new List<RTSTile>();

        foreach (var agent in agents)
        {
            if (!agent.GetIsFriendly())
            {
               playerLocations.Add(agent.GetCurrentTile());
            }
        }
        
        // get all the money tiles from their positions
        foreach(var tileLocations in moneyTileLocations)
        {
            currentMinDistance = 18;
            // money tile locations
            RTSTile newTile = grid.GetTileAtPosition(tileLocations); 
            
            int tileValue = 300; 
            
            // distance to money tile
            List<RTSTile> pathToMoneyTile = _aStar.FindPath(start, newTile, grid);
            
            if (pathToMoneyTile.Count == 0)
            {
                continue;
            }
            
            float distanceToMoneyTile = _aStar.GetDistance();
            
            // loop through all the players agents to calculate the minimum distance
            foreach(var players in playerLocations)
            {
                
                distance = Heuristic(tileLocations, players.GetCurrentTilePosition());
               
                if (distance < currentMinDistance)
                {
                    currentMinDistance = distance;
                }
            }

            // if tiles are found that other agents are targeting apply a penalty
            if (targetTiles.TryGetValue(newTile, out var value))
            {
                agentPenalty = value * value * 40;
            }
            else
            {
                agentPenalty = 0;
            }
           
            // calculate the threat by using the enemies distance 
            float threat = 1.0f / currentMinDistance > 0 ? 1.0f / currentMinDistance : float.MaxValue;
            
            float threatDistanceWeight = 0.1f; 
            
            // find the tactical value of the tile from the value of the money to its distance and the amount of agents targteting it and how close enemy agents are
            float tacticalValue = tileValue - distanceToMoneyTile - agentPenalty - (threat * threatDistanceWeight); 
            _usefulTiles.Add(newTile, tacticalValue);
        }
        
        if (_usefulTiles == null || _usefulTiles.Count == 0)
        {
            return null;
        }
        
        // find the key associated with the highest tactical  value
        var associatedKey =  _usefulTiles.Aggregate((x,y) => x.Value > y.Value ? x : y);
        // return the path of the best key
        return _aStar.FindPath(start, associatedKey.Key, grid);
    }

    public void SetAgent(EvanTestAgent newAgent)
    {
        _agent = newAgent;
    }
}
