using UnityEngine;
using TMPro;  // TextMeshPro namespace

public class BalanceDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text balanceText;  // drag your TextMeshPro-UGUI component here

    private void Start()
    {
        // initialize display
        UpdateBalanceText(MoneyManager.Instance.Balance);

        // subscribe to balance changes
        MoneyManager.Instance.OnBalanceChanged += UpdateBalanceText;
    }

    private void OnDestroy()
    {
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.OnBalanceChanged -= UpdateBalanceText;
    }

    private void UpdateBalanceText(int newBalance)
    {
        balanceText.text = $"Current balance: ${newBalance}";
    }
}