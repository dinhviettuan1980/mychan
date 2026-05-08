using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GameObject cardPrefab;
    public Sprite[] cardSprites;

    private const int players = 4;
    private const int cardsPerPlayer = 2;

    void Start()
    {
        ShuffleAndDeal();
    }

    void ShuffleAndDeal()
    {
        // Shuffle
        List<Sprite> deck = new List<Sprite>(cardSprites);
        for (int i = 0; i < deck.Count; i++)
        {
            Sprite temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }

        // Góc cho mỗi player: BottomLeft, BottomRight, TopLeft, TopRight
        Vector2[] playerOrigins = new Vector2[]
        {
            new Vector2(-6, -3), // Player 1 - bottom left
            new Vector2(2, -3),  // Player 2 - bottom right
            new Vector2(-6, 2),  // Player 3 - top left
            new Vector2(2, 2)    // Player 4 - top right
        };

        int cardIndex = 0;

        for (int p = 0; p < players; p++)
        {
            Vector2 start = playerOrigins[p];
            bool horizontal = (p % 2 == 0); // Player 0 & 2: horizontal, Player 1 & 3: horizontal

            for (int c = 0; c < cardsPerPlayer; c++)
            {
                if (cardIndex >= deck.Count) return;

                Vector2 offset = horizontal ? new Vector2(c * 1.2f, 0) : new Vector2(0, -c * 1.5f);
                Vector2 pos = start + offset;

                GameObject card = Instantiate(cardPrefab, pos, Quaternion.identity);
                card.GetComponent<SpriteRenderer>().sprite = deck[cardIndex++];
            }
        }
    }
}
