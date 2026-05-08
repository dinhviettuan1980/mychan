using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DashboardController : MonoBehaviour
{
    public Text usernameText;
    public Text walletText;
    public Button playNowButton;
    public Button helpButton;

    private string username = "Player123";
    private int wallet = 20000;

    void Awake()
    {
        Debug.Log("Awake chạy");
        if (playNowButton != null)
        {
            Debug.Log("Button OK, add listener");
            playNowButton.onClick.RemoveAllListeners();
            playNowButton.onClick.AddListener(OnPlayNowClicked);

        }
        else
        {
            Debug.LogError("❌ playNowButton = NULL");
        }

        if (helpButton != null)
        {
            Debug.Log("HelpButton OK, add listener");
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(OnHelpClicked);
        }
        else
        {
            Debug.LogError("❌ helpButton = NULL");
        }

    }

    void Start()
    {
        Debug.Log("Start chạy");
    }

    void OnEnable()
    {
        Debug.Log("OnEnable chạy");

        if (usernameText != null)
            usernameText.text = username;

        if (walletText != null)
            walletText.text = wallet.ToString() + " gold";
    }

    public void OnPlayNowClicked()
    {
        Debug.Log("✅ Chơi ngay clicked");
        SceneManager.LoadScene("Tables");
    }

    void OnHelpClicked()
    {
        SceneManager.LoadScene("Help");
    }
}
