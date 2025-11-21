using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
public class TacticalPathfinding : MonoBehaviour
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

    private RTSGrid _grid;
    
    private EvanTestAgent _agent = new EvanTestAgent();
    
    private RTSTile _currentTile = new RTSTile();
    
    List<RTSTile> _path = new List<RTSTile>();

    private void Start()
    {
        _grid = transform.gameObject.GetComponent<RTSGrid>();
    }

    // find the distance from point to the point
    float Heuristic(Vector2 p1, Vector2 p2)
    {
        return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y);
    }


    /*
     * the base cost without money would be cost = PositionalAdvantage - (Distance * MovementCost) - distanceToEnemy

       PositionalAdvantage depends on if you can see the enemy from there
       so my example would be like: 
       at the certain Pos use the line of sight algorthim i already got to check  for the Agent
       if it cant look for another position
       calculate the distance to enemy: maybe using the heuristic with the agentsCurrentPos
       or i could sub whether or not were at the goal point for distanceToEnemy
     */
    
    
    public List<RTSTile> FindBestPath(RTSTile start,  RTSTile end, RTSGrid grid)
    {
        _path = _aStar.FindPath(start, end, grid);
        float bestTileValue = 0;

        foreach (var tile in _path)
        {

            _ptile.Tile = tile;
            // find the distance calculated by A*
            _ptile.Distance = _aStar.GetDistance();
            
            
            // calculate the current tiles tactical value
            // if the current tile can see the enemy increases its tactical value
            if (LineOfSight(tile, _agent.GetCurrentTile()))
            {
                _ptile.TacticalValue += 1; 
            }
            else
            {
                _ptile.TacticalValue = 0;
            }
            
            // if is the goal tile should also have an effect on it 
            //TacticalValue = CombatAdvantage + (ObjectiveWeight / DistanceToObjective) - TravelCost - Threat
            
            // find the best suitable path
            // tact value is if we can see the enemy
            // obj weight is if were close to the goal tile or on it
            // threat is distance to enemy 
            _ptile.DistanceToEnemy = Heuristic(_ptile.Tile.GetGridPosition(), _agent.GetCurrentTile().GetGridPosition());
            float currentPosValue = (_ptile.TacticalValue + ( _ptile.ObjectiveWeight / _ptile.Distance) - _ptile.DistanceToEnemy);

            if (bestTileValue > currentPosValue)
            {
                // add all the paths with the best value to a list and return the list
                
            }
            return new List<RTSTile>();
        }
        
        // if we see the agent from our current tile
        /*if (LineOfSight(_current.Tile, _agent.currentTile))
        {
            
            //_distanceToEnemy = Heuristic(_current.Tile.gridPosition, _agent.currentTile.gridPosition);
        }
        //float distance = Heuristic(_start.gridPosition, _goal.gridPosition);
        return distance;*/
        return null;
    }

    public void SetAgent(EvanTestAgent newAgent)
    {
        _agent = newAgent;
    }
    
    bool LineOfSight(RTSTile from, RTSTile to)
    {
        Vector2 startPos = from.GetGridPosition();
        Vector2 endPos = to.GetGridPosition();
        
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
            var gridX = (int)Mathf.Round(currentX);
            var gridY = (int)Mathf.Round(currentY);
            
            RTSTile tile = _grid.GetTileAtPosition(new Vector2Int(gridX, gridY));
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
