using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class AStar : MonoBehaviour
{
    //private PriorityQueue<Vector2, int> _priorityQueue = new PriorityQueue<Vector2, int>(); 
    private PriorityQueue<Tile, int> _frontier;
    private Dictionary<Tile, Tile> _cameFrom;
    private Dictionary<Tile, int> _costSoFar; 
    
    List<Tile> _neighbors;
    
    Grid _grid;
    
    Tile _start;
    Tile _goal;
    private Tile _current; 
    
    
    // Start is called before the first frame update
    void Start()
    {
        // initialise the _priorityQueue
        _frontier = new PriorityQueue<Tile, int>();
        _cameFrom = new Dictionary<Tile, Tile>();
        _costSoFar = new Dictionary<Tile, int>();
        _neighbors = new List<Tile>();
        
        _frontier.Enqueue(_start, 0);
       
        _costSoFar.Add(_start, 0);
    }
    
    
    // Update is called once per frame
    void Update()
    {
        
        while (_frontier.Count != 0)
        {
            _current = _frontier.Peek();
            _frontier.Dequeue(); 
            
            _neighbors = _grid.FindNeighbors(_current);

            if (_current == _goal)
            {
                break;
            }
            
        }
    }


 
}
