using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EvanTestAgent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoxCollider2D groupDetector;
    public GameObject leader;
    public List<GameObject> followers = new List<GameObject>();
    private Vector3 waypoint;
    private RTSTile currentTile;

    public void Setup()
    {
        
    }

    #region Group
    private void OnTriggerEnter2D(Collider2D other)
    {
        //TODO groups don't typically merge
        if (leader) return;
        
        EvanTestAgent otherAgent = other.gameObject.GetComponent<EvanTestAgent>();
        if (otherAgent)
        {
            GameObject otherAgentLeader = otherAgent.GetLeader();
            
            // Other agent doesn't have a leader
            if (!otherAgentLeader)
            {
                Follow(otherAgent.gameObject);
            }
            // Otherwise follow the other agent's leader
            else
            {
                Follow(otherAgentLeader.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        EvanTestAgent otherAgent = other.gameObject.GetComponent<EvanTestAgent>();
        
        // Leader
        if (followers.Count != 0)
        {
            if (followers.Contains(other.gameObject))
            {
                followers.Remove(other.gameObject);
                other.GetComponent<EvanTestAgent>().leader = null;
            }
        }
        // Follower
        else if (otherAgent)
        {
            if (leader)
            {
                // We lost a fellow follower
                if (other.gameObject != leader && otherAgent.leader == leader)
                {
                    List<Collider2D> overlaps = new List<Collider2D>();
                    groupDetector.OverlapCollider(new ContactFilter2D().NoFilter(), overlaps);
                    overlaps.Remove(other);
                    overlaps.Remove(gameObject.GetComponent<BoxCollider2D>());
                    
                    // Check if we are still overlapping someone with the same leader
                    foreach (Collider2D overlap in overlaps)
                    {
                        EvanTestAgent overlapAgent = overlap.GetComponent<EvanTestAgent>();
                        if (overlapAgent && overlapAgent.leader == leader)
                        {
                            return;
                        }
                    }
                    
                    // We lost connection to the leader
                    EvanTestAgent leaderAgent = leader.GetComponent<EvanTestAgent>();
                    leaderAgent.RemoveFollower(gameObject);
                    leader = null;
                }
            }
        }
    }
    
    public void Follow(GameObject newLeader)
    {
        // Don't follow ourselves
        if (newLeader == gameObject) return;
        
        newLeader.GetComponent<EvanTestAgent>().AddFollower(gameObject);
        leader = newLeader;
    }

    public void AddFollower(GameObject newFollower)
    {
        followers.Add(newFollower);
    }
    
    public void RemoveFollower(GameObject oldFollower)
    {
        followers.Remove(oldFollower);
    }

    public GameObject GetLeader()
    {
        return leader;
    }

    public List<GameObject> GetFollowers()
    {
        return followers;
    }
    #endregion
    
    #region Target
    public void SetWaypoint(RTSTile tile)
    {
        waypoint = tile.transform.position;
        
        currentTile.ResetOwner();
        SetCurrentTile(tile);
        tile.SetOwner(gameObject);
        transform.position = waypoint + Vector3.back * 3;
    }
    #endregion
    
    #region Tile
    public void SetCurrentTile(RTSTile tile)
    {
        currentTile = tile;
    }
    #endregion
}
