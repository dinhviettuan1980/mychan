using UnityEngine;
using UnityEngine.SceneManagement;

public class HelpManager : MonoBehaviour
{
    public GameObject helpPopup; // Gán trong Inspector

    [Tooltip("Margin thêm vào ngoài safe area (px)")]
    public float extraMargin = 60f;

    public void ShowHelp()
    {
        ApplySafeAreaMargin();
        helpPopup.SetActive(true);
    }

    void ApplySafeAreaMargin()
    {
        RectTransform rt = helpPopup.GetComponent<RectTransform>();
        if (rt == null) return;

        Rect safe = Screen.safeArea;
        float left   = safe.xMin + extraMargin;
        float bottom = safe.yMin + extraMargin;
        float right  = Screen.width  - safe.xMax + extraMargin;
        float top    = Screen.height - safe.yMax + extraMargin;

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    public void CloseHelp()
    {
        helpPopup.SetActive(false);
    }

    public void OnBackButtonClick()
    {
        SceneManager.LoadScene("MiniGameMenu");
    }
}
