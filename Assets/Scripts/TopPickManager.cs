using UnityEngine;
using System.Collections.Generic;

public class TopPickManager : MonoBehaviour
{
    public Transform topPickPanel;
    public GameObject cardContainerPrefab;

    private List<GameObject> cardPrefabs = new List<GameObject>();
    private int cardCount = 0;

    void Start()
    {
        if (cardContainerPrefab != null)
        {
            GameObject instance = Instantiate(cardContainerPrefab);
            instance.SetActive(false);
            var container = instance.GetComponent<CardContainer>();
            if (container != null)
            {
                cardPrefabs = new List<GameObject>(container.cardPrefabs);
                Debug.Log($"[TopPick] Loaded {cardPrefabs.Count} cards.");
            }
        }
    }

    public void AddCardToTopPick(string cardName)
    {
        if (cardCount >= 10) return;

        GameObject prefab = cardPrefabs.Find(c => c.name == cardName);
        if (prefab == null)
        {
            Debug.LogWarning($"[TopPick] Card not found: {cardName}");
            return;
        }

        GameObject card = Instantiate(prefab, topPickPanel);
        card.transform.localScale = Vector3.one * 0.4f;
        cardCount++;
    }

    public void ResetTopPick()
    {
        foreach (Transform child in topPickPanel)
        {
            Destroy(child.gameObject);
        }
        cardCount = 0;
    }

    public void AddRandomCardsToTopPick()
    {
        ResetTopPick();

        List<GameObject> pool = new List<GameObject>(cardPrefabs);
        int added = 0;

        while (added < 10 && pool.Count > 0)
        {
            int index = Random.Range(0, pool.Count);
            GameObject prefab = pool[index];
            pool.RemoveAt(index);

            GameObject card = Instantiate(prefab, topPickPanel);
            card.transform.localScale = Vector3.one * 0.4f;
            added++;

            Debug.Log($"[TopPick] Added card: {prefab.name}");
        }

        cardCount = added;
    }
}
