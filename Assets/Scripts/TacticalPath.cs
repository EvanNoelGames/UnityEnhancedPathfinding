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
            
            // now need to calculate the tactical value for a money tile
            // value = PositionalAdvantage - (Distance * MovementCost) - distanceToEnemy
            int tileValue = 10; 
            
            // distance to money tile
            List<RTSTile> pathToMoneyTile = _aStar.FindPath(start, newTile, grid);
            
            Debug.Log(pathToMoneyTile);
            if (pathToMoneyTile.Count == 0)
            {
                continue;
            }
            
            float distanceToMoneyTile = _aStar.GetDistance();
            
            
            //RTSTile enemyLocation = playerAgents.GetCurrentTile();
            foreach(var players in playerLocations)
            {
                
                distance = Heuristic(tileLocations, players.GetCurrentTilePosition());
                //RTSTile playerTile = _rtsGrid.GetTileAtPosition(playerLocations);

                //float distance = Mathf.Abs(tileLocations.x - players.GetGridPosition().x) + Mathf.Abs(tileLocations.y - players.GetGridPosition().y);

                if (distance < currentMinDistance)
                {
                    currentMinDistance = distance;
       
                    //distanceToEnemy = Heuristic(newTile.GetCurrentTilePosition(), closetTile.GetCurrentTilePosition());
                }
            }
           
            float threat = 1.0f / currentMinDistance > 0 ? 1.0f / currentMinDistance : float.MaxValue;
            
            int threatWeight = 2; 

            float tacticalValue = tileValue - distanceToMoneyTile - (threat * threatWeight); 
            _usefulTiles.Add(newTile, tacticalValue);
        }

        // get the current tile that the enemy agent is on 
        //RTSTile enemyLocation = _agent.GetCurrentTile();
        //_usefulTiles.Add(enemyLocation, );
        
        // calculate the current tiles tactical value
        // if the current tile can see the enemy increases its tactical value
        /*foreach (var tile in _rtsGrid.GetTiles())
        {
            if (LineOfSight(tile, _agent.GetCurrentTile()))
            {
                //_usefulTiles.Add(tile);
               // _ptile.TacticalValue += 1;
            }
            else
            {
                //_ptile.TacticalValue = 0;
            }
        }*/

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
    
    bool LineOfSight(RTSTile from, RTSTile to)
    {
        Vector2Int startPos = from.GetGridPosition();
        Vector2Int endPos = to.GetGridPosition();
        
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
            int gridX = Mathf.RoundToInt(currentX);
            int gridY = Mathf.RoundToInt(currentY);
            
            RTSTile tile = _rtsGrid.GetTileAtPosition(new Vector2Int(gridX, gridY));
            if (tile != null && tile != from && tile != to)
            {
                if (tile.GetTileType() == RTSTile.TileType.Blocked)
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
