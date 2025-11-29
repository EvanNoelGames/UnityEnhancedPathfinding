using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utils; 

public class Enemy : MonoBehaviour
{
    public GameObject agentPrefab;
    public int maxAgents = 8;
    private EvanTestAgent _agent;
    private AStar _aStar;
    
    private RTSTile _rtsTile;
    public RTSGrid _rtsGrid;
    public RTSGame _rtsGame;
    
    private Tile _goal;
    
    private int money = 0;
    private int agentCost = 0;
    private float spawnInterval = 1.0f;

    public void GameStart(int startingMoney, int newAgentCost)
    {
        money = startingMoney;
        agentCost = newAgentCost;

        StartCoroutine(EnemySpawning());
    }

    IEnumerator EnemySpawning()
    {
        yield return new WaitForSeconds(1 * spawnInterval);
        
        while (_rtsGame.GetIsRunning())
        {
            SpawnEnemy(_rtsGrid.GetEmptyTiles()[Random.Range(0, _rtsGrid.GetEmptyTiles().Count)]);
            yield return new WaitForSeconds(Random.Range(0.2f * spawnInterval, 5.0f * spawnInterval));
        }
    }
    
    public void AddMoney(int amount)
    {
        money += amount;
    }
    
    private void SpawnEnemy(RTSTile tile)
    {
        if (money < agentCost) return;
        if (maxAgents < _rtsGame.enemyAgents.Count) return;
        
        money -= agentCost;
        
        GameObject newEnemy = Instantiate(agentPrefab);
        newEnemy.transform.position = tile.transform.position + (Vector3.back * 3);

        EvanTestAgent agentComponent = newEnemy.GetComponent<EvanTestAgent>();
        
        foreach (EvanTestAgent agent in _rtsGame.enemyAgents)
        {
            Collider2D currentCollider = agent.GetComponentInChildren<Collider2D>();
            Physics2D.IgnoreCollision(currentCollider, newEnemy.GetComponentInChildren<Collider2D>());
        }
        
        _rtsGame.enemyAgents.Add(agentComponent);
        agentComponent.Killed += _rtsGame.AgentKilled;
        agentComponent.SetIsFriendly(false);
        agentComponent.Setup();
        agentComponent.SetCurrentTile(tile);

        List<Vector2Int> moneyTileLocations = _rtsGrid.GetMoneyTiles(false);
        RTSTile closetTile = null;
        float currentMinDistance = 18;
        
        foreach(var tileLocations in moneyTileLocations)
        {
            RTSTile newTile = _rtsGrid.GetTileAtPosition(tileLocations); 
            
            float distance = Mathf.Abs(tileLocations.x - tile.GetGridPosition().x) + Mathf.Abs(tileLocations.y - tile.GetGridPosition().y);
            
            if (distance < currentMinDistance)
            {
                currentMinDistance = distance;
                closetTile = newTile;
            }
        }

        if (closetTile == null)
        {
            closetTile =  _rtsGrid.GetEmptyTiles()[Random.Range(0, _rtsGrid.GetEmptyTiles().Count)];
        }

        if (closetTile != null)
        {
            agentComponent.SetWaypoint(closetTile);
        }
    }

    public void SetAgentCost(int newValue)
    {
        agentCost = newValue;
    }

    public void SetMaxEnemies(int newValue)
    {
        maxAgents = newValue;
    }

    public void SetSpawnInterval(float newInterval)
    {
        spawnInterval = newInterval;
    }
    
    void CreateAgent(RTSTile start, RTSTile goal, RTSGrid grid)
    {
        _aStar = new AStar();
        
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

    public int GetMoney()
    {
        return money;
    }
}
