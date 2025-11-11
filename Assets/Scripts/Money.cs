using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Money : MonoBehaviour
{
    int money;
    int startingMoney = 15;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addMoney(int amount)
    {
        money += amount;
    }

    public bool buy(int price)
    {
        if(money - price >= 0)
        {
            money -= price;
            return true;
        }
        return false;
    }
}
