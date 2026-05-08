using UnityEngine;
using System.Collections.Generic;

public class CardContainer : MonoBehaviour
{
    public List<GameObject> cardPrefabs = new List<GameObject>();

    void Awake()
    {
        cardPrefabs.Clear(); // luôn reset trước
        foreach (Transform child in transform)
        {
            cardPrefabs.Add(child.gameObject);
        }
    }
}
