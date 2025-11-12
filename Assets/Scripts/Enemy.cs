using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils; 

public class Enemy : MonoBehaviour
{
    public GameObject agentPrefab;
    private Agent _agent;
    private AStar _aStar;
    
    private RTSTile _rtsTile;
    private RTSGrid _rtsGrid;
    
    private Tile _goal;
    
    private int _totalMoney = 15; 
    
    void CreateAgent(RTSTile start, RTSTile goal, RTSGrid grid)
    {
        
        _aStar = new AStar();
        
        List<Tile> path = new List<Tile>();
        //path = _aStar.FindPath(start, goal , grid);
        
        // have to check whose turn it is as well 

        // create the actual game object for the agent
        agentPrefab = Instantiate(agentPrefab);

        // set its position
        agentPrefab.transform.position = new Vector3(0, 0, 0);

        // set up the agent
        _agent = agentPrefab.GetComponent<Agent>();
        _agent.isPathFinding = false;
        //_agent.grid = grid;

        // set its path   
        _agent.BeginWalk(path);
        
    }
}
