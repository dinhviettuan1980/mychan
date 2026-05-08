using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CardDeckGenerator
{
    [MenuItem("Chan/Generate Full Deck")]
    public static void GenerateFullDeck()
    {
        // Load Base25Cards trong Resources
        CardCollection baseCollection = Resources.Load<CardCollection>("Base25Cards");
        if (baseCollection == null)
        {
            Debug.LogError("Không tìm thấy Base25Cards trong thư mục Resources/");
            return;
        }

        List<CardData> fullDeck = new List<CardData>();
        foreach (var card in baseCollection.cards)
        {
            for (int i = 0; i < 4; i++)
            {
                CardData clone = new CardData
                {
                    id = $"{card.id}_{i + 1}",
                    name = card.name,
                    type = card.type,
                    rank = card.rank,
                    image = card.image
                };
                fullDeck.Add(clone);
            }
        }

        // Tạo asset mới
        CardCollection deckAsset = ScriptableObject.CreateInstance<CardCollection>();
        deckAsset.cards = fullDeck;

        AssetDatabase.CreateAsset(deckAsset, "Assets/Cards/Full100Deck.asset");
        AssetDatabase.SaveAssets();

        Debug.Log("✅ Đã tạo Full100Deck.asset với 100 quân bài!");
    }
}
