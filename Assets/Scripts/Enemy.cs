using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Utils; 

public class Enemy : MonoBehaviour
{
    public GameObject agentPrefab;
    public int maxAgents = 8;
    private EvanTestAgent _agent;
    private AStar _aStar;
    private TacticalPath _tacticalPath;
    
    private RTSTile _rtsTile;
    public RTSGrid _rtsGrid;
    public RTSGame _rtsGame;
    
    private Tile _goal;
    RTSTile closetTile = null;
    
    private int money = 0;
    private int agentCost = 0;
    private float spawnInterval = 1.0f;
    int numAgents = 0;
    
    Dictionary<RTSTile, int> _tilesBeingTargeted =  new Dictionary<RTSTile, int>();

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
        if (maxAgents < _rtsGame.enemyAgents) return;
        
        money -= agentCost;
        
        GameObject newEnemy = Instantiate(agentPrefab);
        _rtsGame.enemyAgents++;
        newEnemy.transform.position = tile.transform.position + (Vector3.back * 3);

        //List<RTSTile> path = new List<RTSTile>();
       

        
        
        EvanTestAgent agentComponent = newEnemy.GetComponent<EvanTestAgent>();
        agentComponent.Killed += _rtsGame.AgentKilled;
        agentComponent.SetIsFriendly(false);
        agentComponent.Setup();
        agentComponent.SetCurrentTile(tile);

        List<Vector2Int> moneyTileLocations = _rtsGrid.GetMoneyTiles(false);
        
        

        List<EvanTestAgent> agents = _rtsGame.GetAllAgents();
        
        //EvanTestAgent agent = _rtsGame.GetAgent();
        
        // find the best path
        _tacticalPath = new TacticalPath();
        List<RTSTile> bestPath =  _tacticalPath.FindBestPath(tile, _rtsGrid, agents, _tilesBeingTargeted);
        
       

        if (bestPath != null && bestPath.Count > 0)
        {
            var size = bestPath.Count;

            // find the tile picked from best path increment its value based on the agents tracking it 
            if (_tilesBeingTargeted.ContainsKey(bestPath.ElementAt(size - 1)))
            {
                if (_tilesBeingTargeted.TryGetValue(bestPath.ElementAt(size - 1), out var value))
                {
                    _tilesBeingTargeted[bestPath.ElementAt(size - 1)] = value + 1;
                }
            }
            else
            {
                _tilesBeingTargeted.Add(bestPath.ElementAt(size - 1), 1);
            }
            
            RTSTile goalTile = bestPath[bestPath.Count - 1];
            agentComponent.SetPath(bestPath);
        }
        else
        {
            if (closetTile == null)
            {
                closetTile =  _rtsGrid.GetEmptyTiles()[Random.Range(0, _rtsGrid.GetEmptyTiles().Count)];
            }
            
            agentComponent.SetWaypoint(closetTile);
        }
        
        /*if (closetTile == null)
        {
            closetTile =  _rtsGrid.GetEmptyTiles()[Random.Range(0, _rtsGrid.GetEmptyTiles().Count)];
        }*/

        /*if (closetTile != null)
        {
            agentComponent.SetWaypoint(closetTile);
        }*/
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
        print(spawnInterval);
    }
    

    public int GetMoney()
    {
        return money;
    }
}
