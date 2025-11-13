using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
public class TacticalPathfinding
{
    struct TilePrioritized
    {
        public Tile Tile;
        public float Distance;
        public float TacticalValue;
        public float ObjectiveWeight;
        public float DistanceToEnemy;
        public Tile Parent;
    }
    
    private AStar _aStar = new AStar();
    
    private TilePrioritized _ptile = new TilePrioritized();
   
    private Grid _grid =  new Grid();
    
    private Agent _agent = new Agent();
    
    private Tile _currentTile = new Tile();
    
    List<Tile> _path = new List<Tile>();
    
    

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
    
    
    List<Tile> FindBestPath(Tile start,  Tile end, Grid grid)
    {
       // _path = _aStar.FindPath(start, end, grid);
        float bestTileValue = 0;

        foreach (var tile in _path)
        {

            _ptile.Tile = tile;
            // find the distance calculated by A*
            _ptile.Distance = _aStar.GetDistance();
            
            
            // calculate the current tiles tactical value
            // if the current tile can see the enemy increases its tactical value
            if (LineOfSight(tile, _agent.currentTile))
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
            _ptile.DistanceToEnemy = Heuristic(_ptile.Tile.gridPosition, _agent.currentTile.gridPosition);
            float currentPosValue = (_ptile.TacticalValue + ( _ptile.ObjectiveWeight / _ptile.Distance) - _ptile.DistanceToEnemy);

            if (bestTileValue > currentPosValue)
            {
                // add all the paths with the best value to a list and return the list
                
            }
            return new List<Tile>();
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
