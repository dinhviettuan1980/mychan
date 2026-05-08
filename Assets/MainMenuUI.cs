using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text walletText;
    public Button playButton;

    [System.Serializable]
    public class PlayerData
    {
        public string username;
        public int wallet;
    }

    public GameObject topPlayerItemPrefab;
    public Transform container;  // Gắn là TopPlayersPanel trong Editor

    public PlayerData[] topPlayers = new PlayerData[]
    {
        new PlayerData { username = "Player1111", wallet = 50000 },
        new PlayerData { username = "Player2", wallet = 45000 },
        new PlayerData { username = "Player3", wallet = 40000 },
        new PlayerData { username = "Player4", wallet = 35000 }
    };


    void Start()
    {
        // Giả lập thông tin user
        string username = "Tuandv";
        int wallet = 20000;

        usernameText.text = username;
        walletText.text = wallet.ToString("N0") + " gold";

        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < topPlayers.Length; i++)
        {
            var player = topPlayers[i];
            GameObject item = Instantiate(topPlayerItemPrefab, container);

            // Gán nội dung
            item.transform.Find("RankText").GetComponent<TMP_Text>().text = (i + 1).ToString() + ".";
            item.transform.Find("UsernameText").GetComponent<TMP_Text>().text = player.username;
            item.transform.Find("WalletText").GetComponent<TMP_Text>().text = player.wallet.ToString("N0");
        }

        // Gắn sự kiện cho nút
        playButton.onClick.AddListener(OnPlayClicked);
    }

    void OnPlayClicked()
    {
        SceneManager.LoadScene("Lobby"); // Scene có GameController
    }
}
