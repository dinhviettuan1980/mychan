using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LearnSortGameManager : MonoBehaviour
{
    [Header("Deck Setup")]
    public List<GameObject> cardPrefabs; // 25 quân bài gốc
    public Transform handParent; // Nơi hiển thị 19 cây
    public float spacing = 1.0f;

    [Header("Zones")]
    public Transform chanZone;
    public Transform caZone;
    public Transform baDauZone;

    [Header("UI")]
    public Button btnChan;
    public Button btnCa;
    public Button btnBa;
    public Button btnDone;
    public Button btnPlayAgain;
    public Button btnBack;

    [Header("Expected Count")]
    public int expectedChan = 4;
    public int expectedCa = 8;
    public int expectedBa = 3;

    private List<GameObject> fullDeck = new List<GameObject>();
    private List<GameObject> handCards = new List<GameObject>();

    void Start()
    {
        GenerateFullDeck();
        DisplayHand();
        AssignButtonCallbacks();
    }

    void GenerateFullDeck()
    {
        fullDeck.Clear();
        int times = 4; // 25 x 4 = 100 quân
        for (int i = 0; i < times; i++)
        {
            foreach (GameObject card in cardPrefabs)
            {
                fullDeck.Add(card);
            }
        }

        // Shuffle
        for (int i = 0; i < fullDeck.Count; i++)
        {
            GameObject temp = fullDeck[i];
            int randomIndex = Random.Range(i, fullDeck.Count);
            fullDeck[i] = fullDeck[randomIndex];
            fullDeck[randomIndex] = temp;
        }
    }

    void DisplayHand()
    {
        ClearHand();
        handCards.Clear();

        // Lấy 19 quân đầu tiên
        for (int i = 0; i < 19; i++)
        {
            GameObject card = Instantiate(fullDeck[i], handParent);
            card.transform.localPosition = new Vector3(i * spacing, 0, 0);
            card.AddComponent<CardSelectable>();
            handCards.Add(card);
        }
    }

    void ClearHand()
    {
        foreach (Transform t in handParent)
        {
            Destroy(t.gameObject);
        }
    }

    void AssignButtonCallbacks()
    {
        btnChan.onClick.AddListener(() => MoveSelectedToZone(chanZone));
        btnCa.onClick.AddListener(() => MoveSelectedToZone(caZone));
        btnBa.onClick.AddListener(() => MoveSelectedToZone(baDauZone));
        btnDone.onClick.AddListener(CheckResult);
        btnPlayAgain.onClick.AddListener(PlayAgain);
        btnBack.onClick.AddListener(BackToMenu);
    }

    void MoveSelectedToZone(Transform zone)
    {
        List<GameObject> toMove = new List<GameObject>();
        foreach (GameObject card in handCards)
        {
            CardSelectable cs = card.GetComponent<CardSelectable>();
            if (cs != null && cs.IsSelected())
            {
                toMove.Add(card);
            }
        }

        for (int i = 0; i < toMove.Count; i++)
        {
            GameObject card = toMove[i];
            card.transform.SetParent(zone);
            card.transform.localPosition = new Vector3(i * spacing, 0, 0);
            card.GetComponent<CardSelectable>().ResetSelection();
            handCards.Remove(card);
        }
    }

    void CheckResult()
    {
        int chanCount = chanZone.childCount;
        int caCount = caZone.childCount;
        int baCount = baDauZone.childCount;

        if (chanCount == expectedChan && caCount == expectedCa && baCount == expectedBa)
        {
            Debug.Log("🎉 Bạn đã xếp đúng!");
        }
        else
        {
            Debug.Log("❌ Chưa đúng! Xếp hộ sẽ tự động sắp xếp đúng số lượng.");
            AutoArrange();
        }
    }

    void AutoArrange()
    {
        List<GameObject> allCards = new List<GameObject>();

        // gom tất cả bài từ hand và các zone
        allCards.AddRange(handCards);
        handCards.Clear();

        allCards.AddRange(GetChildren(chanZone));
        allCards.AddRange(GetChildren(caZone));
        allCards.AddRange(GetChildren(baDauZone));

        // Clear các zone
        ClearZone(chanZone);
        ClearZone(caZone);
        ClearZone(baDauZone);

        int index = 0;

        // Xếp Chắn
        for (int i = 0; i < expectedChan; i++)
        {
            GameObject card = allCards[index++];
            card.transform.SetParent(chanZone);
            card.transform.localPosition = new Vector3(i * spacing, 0, 0);
        }

        // Xếp Cạ
        for (int i = 0; i < expectedCa; i++)
        {
            GameObject card = allCards[index++];
            card.transform.SetParent(caZone);
            card.transform.localPosition = new Vector3(i * spacing, 0, 0);
        }

        // Xếp Ba đầu
        for (int i = 0; i < expectedBa; i++)
        {
            GameObject card = allCards[index++];
            card.transform.SetParent(baDauZone);
            card.transform.localPosition = new Vector3(i * spacing, 0, 0);
        }

        // Các quân còn lại về hand
        handCards = new List<GameObject>();
        for (; index < allCards.Count; index++)
        {
            GameObject card = allCards[index];
            card.transform.SetParent(handParent);
            card.transform.localPosition = new Vector3(handCards.Count * spacing, 0, 0);
            handCards.Add(card);
            if (card.GetComponent<CardSelectable>() == null)
                card.AddComponent<CardSelectable>();
        }
    }

    List<GameObject> GetChildren(Transform parent)
    {
        List<GameObject> list = new List<GameObject>();
        foreach (Transform t in parent)
        {
            list.Add(t.gameObject);
        }
        return list;
    }

    void ClearZone(Transform zone)
    {
        foreach (Transform t in zone)
        {
            t.SetParent(null);
        }
    }

    void PlayAgain()
    {
        // Trả bài về hand
        List<GameObject> allCards = new List<GameObject>();
        allCards.AddRange(GetChildren(chanZone));
        allCards.AddRange(GetChildren(caZone));
        allCards.AddRange(GetChildren(baDauZone));

        ClearZone(chanZone);
        ClearZone(caZone);
        ClearZone(baDauZone);

        foreach (GameObject card in allCards)
        {
            card.transform.SetParent(handParent);
        }

        handCards = new List<GameObject>(allCards);

        // Reset vị trí
        for (int i = 0; i < handCards.Count; i++)
        {
            handCards[i].transform.localPosition = new Vector3(i * spacing, 0, 0);
            CardSelectable cs = handCards[i].GetComponent<CardSelectable>();
            if (cs != null) cs.ResetSelection();
        }
    }

    void BackToMenu()
    {
        SceneManager.LoadScene("MiniGameMenu"); // đổi thành tên scene menu của bạn
    }
}
