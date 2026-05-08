using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BetItemUI : MonoBehaviour
{
    public Image background;
    public TMP_Text betText;

    private Lobby manager;
    private int betAmount;

    public void Init(int amount, Lobby mgr)
    {
        betAmount = amount;
        manager = mgr;
        betText.text = amount.ToString("N0"); // format with commas
    }

    public void SetSelected(bool isSelected)
    {
        background.color = isSelected ? Color.yellow : Color.white;
        betText.color = isSelected ? Color.black : Color.red;
    }

    public void OnClick()
    {
        manager.OnSelectBet(this);
    }

    public int GetBetAmount()
    {
        return betAmount;
    }
}
