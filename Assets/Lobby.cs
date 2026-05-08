using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Lobby : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text walletText;
    public Button playButton;

    [System.Serializable]
    public class BetData
    {
        public int bet;
    }

    public GameObject betItemPrefab;
    public Transform container;

    public BetData[] betItems = new BetData[]
    {
        new BetData { bet = 1000 },
        new BetData { bet = 2000 },
        new BetData { bet = 5000 },
        new BetData { bet = 10000 },
        new BetData { bet = 20000 }
    };

    private BetItemUI selectedItem;
    private int selectedBet;

    void Start()
    {
        Debug.Log("Lobby - Start chạy");

        string username = "Tuandv";
        int wallet = 20000;

        usernameText.text = username;
        walletText.text = wallet.ToString("N0");

        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < betItems.Length; i++)
        {
            var data = betItems[i];
            GameObject itemObj = Instantiate(betItemPrefab, container);
            BetItemUI itemUI = itemObj.GetComponent<BetItemUI>();
            itemUI.Init(data.bet, this);

            // Tìm component Button trong các con
            Button btn = itemObj.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => itemUI.OnClick());
            }
            else
            {
                Debug.LogWarning("⚠️ Không tìm thấy Button trong prefab: " + itemObj.name);
            }

            // Đặt selected sau khi gán xong để màu hiển thị đúng
            if (i == 0)
            {
                OnSelectBet(itemUI);
            }
        }

        playButton.onClick.AddListener(OnPlayClicked);
    }

    public void OnSelectBet(BetItemUI itemUI)
    {
        if (selectedItem != null)
        {
            selectedItem.SetSelected(false);
        }

        selectedItem = itemUI;
        selectedBet = itemUI.GetBetAmount();
        selectedItem.SetSelected(true);
    }

    void OnPlayClicked()
    {
        Debug.Log("Bet được chọn: " + selectedBet);
        SceneManager.LoadScene("SampleScene");
    }
}
