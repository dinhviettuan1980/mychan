using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardSpawner : MonoBehaviour
{
    [Header("Danh sách 25 quân bài")]
    public List<GameObject> cardPrefabs;

    [Header("Prefab bài úp lưng")]
    public GameObject backCardPrefab;

    [Header("Prefab đĩa")]
    public GameObject diaPrefab;

    [Header("Text hiển thị số bài trong đĩa")]
    public TMP_Text diaCountText;

    [Header("Vị trí của 4 người chơi")]
    public Transform playerBottomPos;
    public Transform playerLeftPos;
    public Transform playerRightPos;
    public Transform playerTopPos;

    private Queue<GameObject> deckQueue = new Queue<GameObject>();
    private int totalCards = 100;

    private List<string> bottomCards = new List<string>();
    private List<string> leftCards = new List<string>();
    private List<string> rightCards = new List<string>();
    private List<string> topCards = new List<string>();

    void Start()
    {
        int bet = PlayerPrefs.GetInt("BetAmount", 0);
        int tableNum = PlayerPrefs.GetInt("TableNumber", 0);
        int players = PlayerPrefs.GetInt("PlayerCount", 0);

        Debug.Log($"Bet: {bet}, Table: {tableNum}, Players: {players}");

        InitDeck();
        SetupDia();
        UpdateDiaCountText();
    }

    void InitDeck()
    {
        List<GameObject> tempDeck = new List<GameObject>();
        for (int i = 0; i < 4; i++)
        {
            foreach (GameObject card in cardPrefabs)
            {
                tempDeck.Add(card);
            }
        }

        for (int i = 0; i < tempDeck.Count; i++)
        {
            int rand = Random.Range(0, tempDeck.Count);
            var tmp = tempDeck[i];
            tempDeck[i] = tempDeck[rand];
            tempDeck[rand] = tmp;
        }

        foreach (var card in tempDeck)
        {
            deckQueue.Enqueue(card);
        }

        Debug.Log($"== Bộ bài đã tạo: {deckQueue.Count} quân");
    }

    void SetupDia()
    {
        Vector3 centerScreen = new Vector3(-3, 3, 0);
        GameObject dia = Instantiate(diaPrefab, centerScreen, Quaternion.identity);
        dia.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

        Vector3 backCardPos = centerScreen + new Vector3(1, 0.07f, 0);
        GameObject cardOnDia = Instantiate(backCardPrefab, backCardPos, Quaternion.identity);
        SpriteRenderer sr = cardOnDia.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 10;
        }
        cardOnDia.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
        cardOnDia.transform.rotation = Quaternion.Euler(0, 0, 90f);
    }

    public void StartDealing()
    {
        StartCoroutine(DealCardsWithDelay());
    }

    IEnumerator DealCardsWithDelay()
    {
        int cardsPerPlayer = 19;
        Vector3[] positions = new Vector3[] {
            playerBottomPos.position,
            playerLeftPos.position,
            playerTopPos.position,
            playerRightPos.position
        };

        float delay = 0.1f;
        int dealtCount = 0;

        for (int i = 0; i < cardsPerPlayer; i++)
        {
            for (int p = 0; p < 4; p++)
            {
                if (deckQueue.Count == 0) yield break;

                GameObject originalCard = deckQueue.Dequeue();
                GameObject prefabToSpawn = (p == 0) ? originalCard : backCardPrefab;
                GameObject card = Instantiate(prefabToSpawn, new Vector3(-3, 3, 0), Quaternion.identity);

                Vector3 target = positions[p];

                if (p == 0)
                {
                    float angle = -45f + (i * (90f / (cardsPerPlayer - 1)));
                    float radius = 7f;
                    Vector3 offset = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad), 0) * radius;
                    offset.x -= 2.5f;
                    target = playerBottomPos.position + offset;
                    card.transform.rotation = Quaternion.Euler(0, 0, -angle);
                    card.AddComponent<BoxCollider2D>();
                    var toggle = card.AddComponent<CardClickHandlerToggle>();
                    toggle.SetOriginalPosition(target);
                    bottomCards.Add(originalCard.name);
                }
                else
                {
                    if (p == 1) leftCards.Add(originalCard.name);
                    else if (p == 2) topCards.Add(originalCard.name);
                    else if (p == 3) rightCards.Add(originalCard.name);
                }

                card.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                StartCoroutine(MoveCard(card, target, 0.1f));

                dealtCount++;
                totalCards--;
                UpdateDiaCountText();

                yield return new WaitForSeconds(delay);
            }
        }

        Debug.Log("== Bottom: " + string.Join(", ", bottomCards));
        Debug.Log("== Left:   " + string.Join(", ", leftCards));
        Debug.Log("== Top:    " + string.Join(", ", topCards));
        Debug.Log("== Right:  " + string.Join(", ", rightCards));
        Debug.Log("== Remaining in Dia: " + totalCards);
    }

    IEnumerator MoveCard(GameObject card, Vector3 target, float duration)
    {
        Vector3 start = card.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            card.transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        card.transform.position = target;

        // Ẩn backcard nếu là của top/left/right (không phải player chính)
        if (card.CompareTag("BackCard"))
        {
            var sr = card.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }

        var toggle = card.GetComponent<CardClickHandlerToggle>();
        if (toggle != null)
        {
            toggle.SetOriginalPosition(target);
        }
    }

    void UpdateDiaCountText()
    {
        if (diaCountText != null)
        {
            diaCountText.text = totalCards.ToString();
        }
    }
}

public class CardClickHandlerToggle : MonoBehaviour
{
    private Vector3 originalPosition;
    private bool isRaised = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetOriginalPosition(Vector3 pos)
    {
        originalPosition = pos;
    }

    void OnMouseDown()
    {
        if (!isRaised)
        {
            transform.position = originalPosition + new Vector3(0, 0.2f, 0);
            if (sr != null) sr.color = Color.yellow;
        }
        else
        {
            transform.position = originalPosition;
            if (sr != null) sr.color = Color.white;
        }
        isRaised = !isRaised;
    }
}
