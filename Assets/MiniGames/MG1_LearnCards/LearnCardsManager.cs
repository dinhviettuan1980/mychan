using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LearnCardsManager : MonoBehaviour
{
    [Header("References")]
    public CardDatabase database;
    public Image cardImage;
    public Text feedbackText;
    public Button nextButton;

    [Header("Option Buttons")]
    public Button[] optionButtons;

    private CardData1 currentCard;

    void Start()
    {
        nextButton.gameObject.SetActive(false);
        feedbackText.text = "";
        LoadNewCard();
    }

    void LoadNewCard()
    {
        // Clear feedback
        feedbackText.text = "";
        nextButton.gameObject.SetActive(false);

        // Random card
        currentCard = database.cards[Random.Range(0, database.cards.Length)];
        cardImage.sprite = currentCard.sprite;

        // Create answer set
        List<string> options = new List<string>();
        options.Add(currentCard.cardName);

        // Add 3 random wrong answers
        while (options.Count < 4)
        {
            var randomCard = database.cards[Random.Range(0, database.cards.Length)];
            if (randomCard.cardName != currentCard.cardName && !options.Contains(randomCard.cardName))
            {
                options.Add(randomCard.cardName);
            }
        }

        // Shuffle
        for (int i = 0; i < options.Count; i++)
        {
            string temp = options[i];
            int r = Random.Range(i, options.Count);
            options[i] = options[r];
            options[r] = temp;
        }

        // Apply to buttons
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].GetComponentInChildren<Text>().text = options[i];

            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() =>
            {
                OnSelectAnswer(options[index]);
            });
        }
    }

    void OnSelectAnswer(string selected)
    {
        if (selected == currentCard.cardName)
        {
            feedbackText.text = "<color=green>✔ Đúng rồi!</color>";
        }
        else
        {
            feedbackText.text = "<color=red>✖ Sai rồi!</color>\nĐáp án: " + currentCard.cardName;
        }

        nextButton.gameObject.SetActive(true);
    }

    public void OnClickNext()
    {
        LoadNewCard();
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("xep_bai");
    }
}
