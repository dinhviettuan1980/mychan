using UnityEngine;
using UnityEngine.UI;

public class ResultPopup : MonoBehaviour
{
    public Text resultText;
    public Button closeButton;

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void Show(string message)
    {
        resultText.text = message;
        gameObject.SetActive(true);
    }
}
