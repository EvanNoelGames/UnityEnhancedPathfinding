using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSWaypoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject plane;

    private GameObject targetTile;
    private Vector3 targetPosition;
    
    public void ShowWaypoint()
    {
        plane.SetActive(true);
    }
    
    public void HideWaypoint()
    {
        plane.SetActive(false);
    }

    public void SetTargetTile(RTSTile newTargetTile)
    {
        if (newTargetTile.GetOwner()) return;
        
        targetTile = newTargetTile.gameObject;
        transform.position = targetTile.transform.position + (Vector3.back * 3);
        
        ShowWaypoint();
    }

    public GameObject GetTargetTile()
    {
        return targetTile;
    }

    public void ClearTile()
    {
        HideWaypoint();
        targetTile = null;
    }
}
