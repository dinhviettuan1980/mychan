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
    public float         panelHeightCollapsed = 457f;
    public float         panelHeightExpanded  = 713f;

    [Header("Audio")]
    public TMP_Text audioLabel;             // shows "Âm thanh: On/Off"

    private bool      _isOpen;
    private bool      _subOpen;
    private bool      _audioOn;
    private float     _panelW;
    private float     _panelY;
    private Coroutine _anim;

    void Awake()
    {
        // Push button down by safe-area top inset (clears notch / Dynamic Island)
        var hamRT  = GetComponent<RectTransform>();
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            float safeTopPx   = Screen.height - Screen.safeArea.yMax;
            float canvasH     = canvas.GetComponent<RectTransform>().rect.height;
            float safeTopUnit = safeTopPx * (canvasH / Screen.height);
            var pos = hamRT.anchoredPosition;
            pos.y  -= safeTopUnit;
            hamRT.anchoredPosition = pos;
        }

        _panelW = menuPanel.sizeDelta.x;
        _panelY = hamRT.anchoredPosition.y - hamRT.sizeDelta.y + 50;
        menuPanel.anchoredPosition = new Vector2(_panelW, _panelY);
        if (overlay)          overlay.SetActive(false);
        if (subMenuContainer) subMenuContainer.SetActive(false);
        SetPanelHeight(panelHeightCollapsed);

        // Init audio state (default On)
        _audioOn = PlayerPrefs.GetInt("audio_on", 1) == 1;
        AudioListener.volume = _audioOn ? 1f : 0f;
        UpdateAudioLabel();
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

    public void OnAmThanh()
    {
        _audioOn = !_audioOn;
        AudioListener.volume = _audioOn ? 1f : 0f;
        PlayerPrefs.SetInt("audio_on", _audioOn ? 1 : 0);
        PlayerPrefs.Save();
        UpdateAudioLabel();
    }

    public void OnThuChonBai() { Close(); SceneManager.LoadScene("day_xep_bai"); }

    public void OnThoat()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void UpdateAudioLabel()
    {
        if (audioLabel != null)
            audioLabel.text = _audioOn ? "Âm thanh: On" : "Âm thanh: Off";
    }
}
