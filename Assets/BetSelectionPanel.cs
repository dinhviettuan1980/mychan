using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BetSelectionPanel : MonoBehaviour
{
    public List<Button> betButtons;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public TableDisplayManager tableManager; // Tham chiếu trong Inspector

    private int selectedIndex = 0;
    public int bet_number = 1000;

    void Start()
    {
        for (int i = 0; i < betButtons.Count; i++)
        {
            int index = i;
            betButtons[i].onClick.AddListener(() => OnBetButtonClick(index));
        }
        UpdateButtonColors();
    }

    void OnBetButtonClick(int index)
    {
        selectedIndex = index;
        bet_number = int.Parse(betButtons[index].GetComponentInChildren<Text>().text);
        UpdateButtonColors();
        Debug.Log("Selected Bet: " + bet_number);
        tableManager.ShowTables(bet_number); // Hiển thị bàn mới
    }

    void UpdateButtonColors()
    {
        for (int i = 0; i < betButtons.Count; i++)
        {
            Image img = betButtons[i].GetComponent<Image>();
            img.color = (i == selectedIndex) ? highlightColor : normalColor;
        }
    }
}
