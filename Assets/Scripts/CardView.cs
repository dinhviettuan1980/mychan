using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    public Image cardImage;
    public Text cardName;
    public CardData data;

    public void SetCard(CardData cardData, string name, bool faceUp = true)
    {
        data = cardData;
        cardImage.sprite = faceUp ? data.image : Resources.Load<Sprite>("Cards/backcard");
        cardName.text = name;
        cardName.fontStyle = FontStyle.Bold;
    }
}
