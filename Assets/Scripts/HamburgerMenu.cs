using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class HamburgerMenu : MonoBehaviour
{
    [Header("References")]
    public CardManager cardManager;
    public HelpManager helpManager;

    [Header("Panel")]
    public RectTransform menuPanel;
    public GameObject    overlay;

    [Header("Sub-menu")]
    public GameObject    subMenuContainer;
    public TMP_Text      huongDanArrow;     // "▶" / "▼"
    public float         panelHeightCollapsed = 315f;
    public float         panelHeightExpanded  = 523f;

    private bool      _isOpen;
    private bool      _subOpen;
    private float     _panelW;
    private float     _panelY;
    private Coroutine _anim;

    void Awake()
    {
        _panelW = menuPanel.sizeDelta.x;
        // Align panel top to ham button bottom edge dynamically
        var hamRT = GetComponent<RectTransform>();
        _panelY = hamRT.anchoredPosition.y - hamRT.sizeDelta.y;
        menuPanel.anchoredPosition = new Vector2(_panelW, _panelY);
        if (overlay)          overlay.SetActive(false);
        if (subMenuContainer) subMenuContainer.SetActive(false);
        SetPanelHeight(panelHeightCollapsed);
    }

    // ── Open / Close ──────────────────────────────────────────────────────────
    public void ToggleMenu() { if (_isOpen) Close(); else Open(); }

    public void Open()
    {
        _isOpen = true;
        if (overlay) overlay.SetActive(true);
        SlideX(0f);
    }

    public void Close()
    {
        _isOpen = false;
        SlideX(_panelW, () => { if (overlay) overlay.SetActive(false); });
    }

    void SlideX(float targetX, System.Action onDone = null)
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(AnimateX(targetX, onDone));
    }

    IEnumerator AnimateX(float targetX, System.Action onDone)
    {
        float startX = menuPanel.anchoredPosition.x;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.22f;
            menuPanel.anchoredPosition = new Vector2(
                Mathf.SmoothStep(startX, targetX, Mathf.Clamp01(t)), _panelY);
            yield return null;
        }
        menuPanel.anchoredPosition = new Vector2(targetX, _panelY);
        onDone?.Invoke();
    }

    // ── Sub-menu toggle ───────────────────────────────────────────────────────
    public void ToggleHuongDan()
    {
        _subOpen = !_subOpen;
        if (subMenuContainer) subMenuContainer.SetActive(_subOpen);
        if (huongDanArrow)    huongDanArrow.text = _subOpen ? "▼" : "▶";
        SetPanelHeight(_subOpen ? panelHeightExpanded : panelHeightCollapsed);
    }

    void SetPanelHeight(float h)
    {
        var sd = menuPanel.sizeDelta;
        menuPanel.sizeDelta = new Vector2(sd.x, h);
    }

    // ── Actions ───────────────────────────────────────────────────────────────
    public void OnThuLai()   { Close(); cardManager?.RetryHand(); }
    public void OnXepChuan() { Close(); cardManager?.SortPlayerHand(); }
    public void OnKiemTra()  { Close(); cardManager?.CheckResultWithPopup(); }
    public void OnCachXep()  { Close(); helpManager?.ShowHelp(); }
    public void OnCachNho()  { Close(); SceneManager.LoadScene("Help"); }
    public void OnNhanDien() { Close(); SceneManager.LoadScene("LearnCards"); }
    public void OnVideoMau() { Close(); SceneManager.LoadScene("VideoSamples"); }
}
