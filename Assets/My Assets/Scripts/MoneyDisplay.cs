using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoneyDisplay : MonoBehaviour
{
    public TMP_Text moneyText;  // The UI Text component that will display the money
    public PlayerMoney playerMoney;  // Reference to the player's money

    void Update()
    {
        moneyText.text = "Money: " + playerMoney.GetMoney().ToString();
    }
}