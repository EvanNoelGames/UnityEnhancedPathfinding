using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils; 

public class Enemy : MonoBehaviour
{
    public GameObject agentPrefab;
    private EvanTestAgent _agent;
    private AStar _aStar;
    
    private RTSTile _rtsTile;
    private RTSGrid _rtsGrid;
    private RTSGame _rtsGame;
    
    private Tile _goal;
    
    private int _totalMoney = 0; 
    
    void CreateAgent(RTSTile start, RTSTile goal, RTSGrid grid)
    {
        _rtsGame = new RTSGame();
        _aStar = new AStar();

        _totalMoney = _rtsGame.GetMoney();
        
        List<Tile> path = new List<Tile>();
        //path = _aStar.FindPath(start, goal , grid);
        
        // have to check whose turn it is as well 

       
        // create the actual game object for the agent
        agentPrefab = Instantiate(agentPrefab);

        // set its position
        agentPrefab.transform.position = new Vector3(2, 2, 2);

        // set up the agent
        _agent = agentPrefab.GetComponent<EvanTestAgent>();

        _agent.SetWaypoint(goal);
        
    }
}
