using UnityEngine;

public class GuidePopupUI : MonoBehaviour
{
    public GameObject popupRoot;
    public CardManager cardManager;   // 🔗 kéo từ Inspector

    void Start()
    {
        // Hiện popup ngay khi vào scene
        popupRoot.SetActive(true);
    }

    public void ClosePopup()
    {
        popupRoot.SetActive(false);

        // 🔥 tự động chia bài khi đóng popup
        if (cardManager != null)
        {
            cardManager.DealCards();
        }
        else
        {
            Debug.LogError("GuidePopupUI: CardManager chưa được gán!");
        }
    }
}
