using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public List<Vector3> path =  new List<Vector3>();
    private Vector3 targetPosition;
    private bool walking = false;

    public void BeginWalk(List<Tile> newPath)
    {
        path.Clear();
        foreach (Tile tile in newPath)
        {
            path.Add(tile.gameObject.transform.position);
        }
        
        targetPosition = path.First();
        walking = true;
    }

    void FixedUpdate()
    {
        if (walking)
            Walk();
    }

    void Walk()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime);

        if ((transform.position - targetPosition).magnitude < 0.005)
        {
            path.RemoveAt(0);
            
            if (path.Count == 0)
            {
                walking = false;
                transform.position = targetPosition;
                return;
            }
            
            targetPosition = path.First();
        }
    }
}
