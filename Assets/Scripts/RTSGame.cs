using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RTSGame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RTSGrid grid;
    [SerializeField] private UICanvas canvas;
    [SerializeField] private Enemy enemyPlayer;
    [SerializeField] private Slider agentCostSlider;
    [SerializeField] private Slider enemySpawnIntervalSlider;
    [SerializeField] private Slider maxEnemiesSlider;
    [Header("Prefabs")]
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private GameObject waypointPrefab;
    [SerializeField] private TacticalPath _tacticalPath;
    [Header("Values")]
    [SerializeField] private int agentCost = 3;
    [SerializeField] private int startingMoney = 50;
    [SerializeField] private int moneyPerMoneyTile = 5;
    
    private bool isGameRunning = false;
    private bool firstAgentPlaced = false;
    private int money = 0;
    
    private GameObject currentWaypoint = null;
    private List<EvanTestAgent> guidingAgents = new List<EvanTestAgent>();
    private List<EvanTestAgent> activeAgents = new List<EvanTestAgent>();
    
    public int playerAgents = 0;
    public int enemyAgents = 0;

    private bool isFiring = false;
    
    #region START/UPDATE
    private void Start()
    {
        canvas.RestartGame += ReloadScene;
        grid.MoneyTileEarned += MoneyTileEarned;
        RestartGame();
    }

    private void Update()
    {
        if (!isGameRunning) return;
        CheckForClick();
    }
    
    private void SetupEnemy()
    {
        enemyPlayer.GameStart(startingMoney, agentCost);
    }
    #endregion
    
    #region GAME STATE
    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void RestartGame()
    {
        SetupGame();
        StartGame();
    }
    
    private void SetupGame()
    {
        grid.SetupBoard();
        SetMoney(startingMoney);
        canvas.SetAgentCostText(agentCost);
    }

    private void StartGame()
    {
        isGameRunning = true;
        
    }

    private void GameOver(bool playerWon)
    {
        isGameRunning = false;
        canvas.GameOver(playerWon);
    }

    public void AgentKilled(bool friendly, GameObject agent)
    {
        EvanTestAgent agentComponent = agent.GetComponent<EvanTestAgent>();
        activeAgents.Remove(agentComponent);
        
        if (guidingAgents.Contains(agentComponent))
        {
            Destroy(currentWaypoint);
            RTSWaypoint currentWaypointComponent = currentWaypoint.GetComponent<RTSWaypoint>();
        
            grid.NewTileHovered -= currentWaypointComponent.SetTargetTile;
            grid.TileExited -= currentWaypointComponent.ClearTile;
        }
        
        if (friendly)
        {
            playerAgents--;
        }
        else
        {
            enemyAgents--;
        }

        if (playerAgents == 0 && enemyAgents == 0)
        {
            GameOver(money >= enemyPlayer.GetMoney());
        }

        if (playerAgents <= 0)
        {
            GameOver(true);
        }
        else if (enemyAgents <= 0)
        {
            GameOver(false);
        }
    }

    public bool GetIsRunning()
    {
        return isGameRunning;
    }
    #endregion
    
    #region MONEY
    private void MoneyTileEarned(RTSTile tile)
    {
        if (tile.GetOwner() && tile.GetOwner().GetComponent<EvanTestAgent>().GetIsFriendly())
            AddMoney(moneyPerMoneyTile);
        else
        {
            enemyPlayer.AddMoney(moneyPerMoneyTile);
        }
    }
    
    private void SetMoney(int value)
    {
        money = value;
        canvas.SetMoneyText(money);
    }
    
    private void AddMoney(int value)
    {
        money += value;
        canvas.SetMoneyText(money);
    }
    
    private void SubtractMoney(int value)
    {
        money -= value;
        canvas.SetMoneyText(money);
    }
    
    public int GetMoney()
    {
        return money;
    }

    public void ChangeAgentCostSlider()
    {
        agentCost = (int)agentCostSlider.value;
        enemyPlayer.SetAgentCost(agentCost);
        canvas.SetAgentCostText(agentCost);
    }
    #endregion
    
    #region PLAYER INTERACTION
    private void CheckForClick()
    {
        // Left click
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
            RaycastHit2D hit = new RaycastHit2D();
            bool foundHit = false;
            
            foreach (RaycastHit2D hitInfo in hits)
            {
                if (!foundHit && hitInfo.collider.gameObject.CompareTag("Tile"))
                {
                    hit = hitInfo;
                    foundHit = true;
                    break;
                }
            }
            
            if (foundHit)
            {
                RTSTile tileHit = hit.collider.gameObject.GetComponent<RTSTile>();
                
                // Clicked a tile
                if (tileHit)
                {
                    // Clicked tile without owner
                    if (!tileHit.GetOwner())
                    {
                        TileClicked(tileHit);
                    }
                    // Clicked tile with owner
                    else if (tileHit.GetOwner().GetComponent<EvanTestAgent>().GetIsFriendly())
                    {
                        AgentClicked(tileHit.GetOwner().GetComponent<EvanTestAgent>());
                    }
                    else
                    {
                        PlaceWaypoint();;
                    }
                }
                // Check if we clicked an agent
                else
                {
                    EvanTestAgent agentHit = hit.collider.gameObject.GetComponent<EvanTestAgent>();
                    
                    if (agentHit)
                        AgentClicked(agentHit);
                }
            }
        }
    }

    private void TileClicked(RTSTile tile)
    {
        if (!tile) return;
        
        switch (tile.GetTileType())
        {
            case RTSTile.TileType.Blocked:
            {
                return;
            }
            case RTSTile.TileType.Money:
            {
                if (currentWaypoint)
                    PlaceWaypoint();
                return;
            }
            case RTSTile.TileType.None:
            {
                if (currentWaypoint)
                    PlaceWaypoint();
                else
                    SpawnAgentOnTile(tile);
                return;
            }
        }
    }

    private void AgentClicked(EvanTestAgent agent)
    {
        if (currentWaypoint) return;

        // Make sure we are selecting the leader
        if (agent.leader != null)
        {
            agent = agent.leader.GetComponent<EvanTestAgent>();
        }
        
        guidingAgents.Add(agent);

        foreach (GameObject follower in agent.GetFollowers())
        {
            guidingAgents.Add(follower.GetComponent<EvanTestAgent>());
        }
        
        currentWaypoint = Instantiate(waypointPrefab);
        RTSWaypoint newWaypointComponent = currentWaypoint.GetComponent<RTSWaypoint>();
        
        // Allow the waypoint to update while the hovered tile changes
        grid.NewTileHovered += newWaypointComponent.SetTargetTile;
        grid.TileExited += newWaypointComponent.ClearTile;
    }

    private void PlaceWaypoint()
    {
        if (!currentWaypoint) return;
        
        RTSWaypoint currentWaypointComponent = currentWaypoint.GetComponent<RTSWaypoint>();
        RTSTile targetTile = currentWaypointComponent.GetTargetTileComponent();
        
        //tacticalPathfinding.SetAgent(guidingAgents[0]);
        //List<RTSTile> newPath = tacticalPathfinding.FindBestPath(guidingAgents[0].GetCurrentTile(), currentWaypointComponent.GetTargetTileComponent(), grid);
        
        //AStar astar = new AStar();
        //List<RTSTile> newPath = astar.FindPath(guidingAgents[0].GetCurrentTile(),
        //    currentWaypointComponent.GetTargetTileComponent(), grid);
        
        //if (newPath.Count == 0) return;
        
        grid.NewTileHovered -= currentWaypointComponent.SetTargetTile;
        grid.TileExited -= currentWaypointComponent.ClearTile;

        //guidingAgents[0].SetPath(newPath);

        // Placing on tile
        if (targetTile)
        {
            foreach (EvanTestAgent agent in guidingAgents)
            {
                agent.SetWaypoint(currentWaypointComponent.GetTargetTileComponent());
            }
        }
        else // Placing on enemy agent
        {
            foreach (EvanTestAgent agent in guidingAgents)
            {
                agent.SetWaypoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }
        
        guidingAgents.Clear();
        Destroy(currentWaypoint);
    }

    private void SpawnAgentOnTile(RTSTile tile)
    {
        // Not enough money to spawn agent
        if (money < agentCost) return;

        if (!firstAgentPlaced)
        {
            firstAgentPlaced = true;
            SetupEnemy();
        }
        
        SubtractMoney(agentCost);
        
        GameObject newAgent = Instantiate(agentPrefab);
        playerAgents++;
        newAgent.transform.position = tile.transform.position + (Vector3.back * 3);

        EvanTestAgent agentComponent = newAgent.GetComponent<EvanTestAgent>();
        agentComponent.Killed += AgentKilled;
        agentComponent.Setup();
        agentComponent.SetCurrentTile(tile);
        activeAgents.Add(agentComponent);
    }

    public List<EvanTestAgent> GetAllAgents()
    {
        return activeAgents;
    }
    #endregion
    
    #region ENEMIES

    public void UpdateMaxEnemies()
    {
        enemyPlayer.SetMaxEnemies((int)maxEnemiesSlider.value);
    }

    public void UpdateEnemySpawnInterval()
    {
        enemyPlayer.SetSpawnInterval(enemySpawnIntervalSlider.value);
    }
    #endregion
}
