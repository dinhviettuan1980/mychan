using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardCollection", menuName = "Chan/Card Collection", order = 1)]
public class CardCollection : ScriptableObject
{
    public List<CardData> cards = new List<CardData>();
}
