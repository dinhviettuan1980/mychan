using UnityEngine;
using System.Collections.Generic;

public class RightEatManager : MonoBehaviour
{
    [Header("Panel để hiển thị các quân bài ăn ở bên phải")]
    public Transform rightEatPanel;

    [Header("Prefab chứa 25 quân bài")]
    public GameObject cardContainerPrefab;

    private List<GameObject> cardPrefabs = new List<GameObject>();
    private int cardCount = 0;

    void Start()
    {
        Debug.Log("RightEatManager.Start() chạy");

        if (cardContainerPrefab != null)
        {
            Debug.Log("Đang khởi tạo cardContainerPrefab...");

            GameObject containerInstance = Instantiate(cardContainerPrefab);
            containerInstance.SetActive(false); // không hiển thị

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

    public void AddCardToRight(string cardName)
    {
        Debug.Log($"AddCardToRight được gọi với: {cardName}");

        if (cardCount >= 10)
        {
            Debug.LogWarning("Đã đủ 10 quân, không thêm nữa.");
            return;
        }

        GameObject prefab = cardPrefabs.Find(c => c.name == cardName);
        if (prefab == null)
        {
            Debug.LogWarning($"Không tìm thấy quân bài: {cardName}");
            return;
        }

        GameObject card = Instantiate(prefab, rightEatPanel);
        card.transform.localScale = Vector3.one * 0.4f;
        cardCount++;
    }

    public void ResetRightEat()
    {
        Debug.Log("ResetRightEat() được gọi");
        foreach (Transform child in rightEatPanel)
        {
            Destroy(child.gameObject);
        }
        cardCount = 0;
    }

    public void AddRandomCardsToRight()
    {
        Debug.Log("AddRandomCardsToRight() được gọi");

        if (cardPrefabs == null || cardPrefabs.Count == 0)
        {
            Debug.LogWarning("Chưa load được danh sách quân bài");
            return;
        }

        ResetRightEat();

        List<GameObject> pool = new List<GameObject>(cardPrefabs);
        int added = 0;

        while (added < 10 && pool.Count > 0)
        {
            int index = Random.Range(0, pool.Count);
            GameObject prefab = pool[index];
            pool.RemoveAt(index);

            GameObject card = Instantiate(prefab, rightEatPanel);
            card.transform.localScale = Vector3.one * 0.4f;
            added++;

            Debug.Log($"Đã thêm quân: {prefab.name}");
        }

        cardCount = added;
    }
}
