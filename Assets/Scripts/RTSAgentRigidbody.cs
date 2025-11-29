using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSAgentRigidbody : MonoBehaviour
{
    public event Action<Collision2D> CollisionEnter;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionEnter?.Invoke(collision);
    }
}
