using System;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    public int Balance { get; private set; }


    [Header("Starting Funds")]
    [SerializeField]
    private int startingBalance = 1000;    // <-- tweak this in the inspector before play

    // Event fired whenever Balance changes
    public event Action<int> OnBalanceChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Balance = startingBalance;      // starting money
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Try to spend 'amount'. Returns true if successful.
    /// </summary>
    public bool TrySpend(int amount)
    {
        if (amount > Balance) return false;
        Balance -= amount;
        OnBalanceChanged?.Invoke(Balance);
        return true;
    }

    /// <summary>
    /// Add money to your balance.
    /// </summary>
    public void AddFunds(int amount)
    {
        Balance += amount;
        OnBalanceChanged?.Invoke(Balance);
    }
}