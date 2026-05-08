using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserProfileDialog : MonoBehaviour
{
    public GameObject modalPanel;
    public TMP_InputField usernameInput;
    public TMP_Text walletText;

    private void Start()
    {
        modalPanel.SetActive(false);
    }

    public void OpenDialog()
    {
        // Load từ PlayerPrefs
        string username = PlayerPrefs.GetString("username", "Guest123");
        int wallet = PlayerPrefs.GetInt("wallet", 20000);

        usernameInput.text = username;
        walletText.text = wallet.ToString("N0") + " gold";

        modalPanel.SetActive(true);
    }

    public void CloseDialog()
    {
        // Save username
        PlayerPrefs.SetString("username", usernameInput.text);
        PlayerPrefs.Save();

        modalPanel.SetActive(false);
    }
}
