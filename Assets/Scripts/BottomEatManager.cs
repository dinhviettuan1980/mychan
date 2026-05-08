using UnityEngine;
using System.Collections.Generic;

public class BottomEatManager : MonoBehaviour
{
    [Header("Panel để hiển thị các quân bài ăn ở bên dưới")]
    public Transform bottomEatPanel;

    [Header("Prefab chứa 25 quân bài")]
    public GameObject cardContainerPrefab;

    private List<GameObject> cardPrefabs = new List<GameObject>();
    private int cardCount = 0;

    void Start()
    {
        Debug.Log("BottomEatManager.Start() chạy");

        if (cardContainerPrefab != null)
        {
            GameObject containerInstance = Instantiate(cardContainerPrefab);
            containerInstance.SetActive(false);

            CardContainer container = containerInstance.GetComponent<CardContainer>();
            if (container != null)
            {
                cardPrefabs = new List<GameObject>(container.cardPrefabs);
                Debug.Log($"Đã load {cardPrefabs.Count} quân bài từ cardContainerPrefab");
            }
            else
            {
                Debug.LogError("Không tìm thấy component CardContainer!");
            }
        }
    }

    public void AddCardToBottom(string cardName)
    {
        if (cardCount >= 10) return;

        GameObject prefab = cardPrefabs.Find(c => c.name == cardName);
        if (prefab == null) return;

        GameObject card = Instantiate(prefab, bottomEatPanel);
        card.transform.localScale = Vector3.one * 0.4f;
        cardCount++;
    }

    public void ResetBottomEat()
    {
        foreach (Transform child in bottomEatPanel)
        {
            Destroy(child.gameObject);
        }
        cardCount = 0;
    }

    public void AddRandomCardsToBottom()
    {
        if (cardPrefabs == null || cardPrefabs.Count == 0) return;

        ResetBottomEat();

        List<GameObject> pool = new List<GameObject>(cardPrefabs);
        int added = 0;

        while (added < 10 && pool.Count > 0)
        {
            int index = Random.Range(0, pool.Count);
            GameObject card = Instantiate(pool[index], bottomEatPanel);
            card.transform.localScale = Vector3.one * 0.4f;
            pool.RemoveAt(index);
            added++;
        }

        cardCount = added;
    }
}
