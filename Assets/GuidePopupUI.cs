using UnityEngine;

public class GuidePopupUI : MonoBehaviour
{
    public GameObject popupRoot;
    public CardManager cardManager;   // 🔗 kéo từ Inspector

    void Start()
    {
        popupRoot.SetActive(false);

        if (cardManager != null)
            cardManager.DealCards();
        else
            Debug.LogError("GuidePopupUI: CardManager chưa được gán!");
    }

    public void ClosePopup()
    {
        popupRoot.SetActive(false);
    }
}
