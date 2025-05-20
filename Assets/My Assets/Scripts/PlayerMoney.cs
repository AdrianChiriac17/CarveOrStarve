using UnityEngine;

public class PlayerMoney : MonoBehaviour
{
    public int money = 100;  // Starting money

    public void AddMoney(int amount)
    {
        money += amount;
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            return true;
        }
        return false;
    }

    public int GetMoney()
    {
        return money;
    }
}