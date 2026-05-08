using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TableButton : MonoBehaviour
{
    public int tableNumber;
    public int playerCount;
    public int betAmount;

    public void Setup(int tableNum, int players, int bet)
    {
        tableNumber = tableNum;
        playerCount = players;
        betAmount = bet;
        GetComponentInChildren<Text>().text = $"Bàn {tableNumber}\nNgười: {playerCount}";
    }

    public void OnClickTable()
    {
        // Lưu thông tin vào PlayerPrefs hoặc static class để truyền giữa scenes
        PlayerPrefs.SetInt("BetAmount", betAmount);
        PlayerPrefs.SetInt("TableNumber", tableNumber);
        PlayerPrefs.SetInt("PlayerCount", playerCount);

        // Load scene mới
        SceneManager.LoadScene("SampleScene");
    }
}
