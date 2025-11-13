using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RTSGame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RTSGrid grid;
    [SerializeField] private UICanvas canvas;
    [Header("Prefabs")]
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private GameObject waypointPrefab;
    [Header("Values")]
    [SerializeField] private int agentCost = 3;
    [SerializeField] private int startingMoney = 50;
    [SerializeField] private int moneyPerMoneyTile = 5;
    
    private bool isGameRunning = false;
    private int money = 0;
    
    private GameObject currentWaypoint = null;
    private List<EvanTestAgent> guidingAgents = new List<EvanTestAgent>();
    
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
        canvas.GameOver(playerWon);
    }
    #endregion
    
    #region MONEY
    private void MoneyTileEarned()
    {
        AddMoney(moneyPerMoneyTile);
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
    #endregion
    
    #region PLAYER INTERACTION
    private void CheckForClick()
    {
        // Left click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
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
                    else
                    {
                        AgentClicked(tileHit.GetOwner().GetComponent<EvanTestAgent>());
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
        RTSWaypoint currentWaypointComponent = currentWaypoint.GetComponent<RTSWaypoint>();
        
        grid.NewTileHovered -= currentWaypointComponent.SetTargetTile;
        grid.TileExited -= currentWaypointComponent.ClearTile;

        foreach (EvanTestAgent agent in guidingAgents)
        {
            agent.SetWaypoint(currentWaypointComponent.GetTargetTile().GetComponent<RTSTile>());
        }
        
        guidingAgents.Clear();
        Destroy(currentWaypoint);
    }

    private void SpawnAgentOnTile(RTSTile tile)
    {
        // Not enough money to spawn agent
        if (money < agentCost) return;
        SubtractMoney(agentCost);
        
        GameObject newAgent = Instantiate(agentPrefab);
        tile.SetOwner(newAgent);
        newAgent.transform.position = tile.transform.position + (Vector3.back * 3);

        EvanTestAgent agentComponent = newAgent.GetComponent<EvanTestAgent>();
        agentComponent.Setup();
        agentComponent.SetCurrentTile(tile);
    }
    #endregion
}
