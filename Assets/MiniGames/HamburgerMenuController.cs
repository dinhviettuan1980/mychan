using UnityEngine;
using UnityEngine.UI;

public class HamburgerMenuController : MonoBehaviour
{
    [Header("Hamburger Dropdown")]
    public GameObject dropdownPanel;

    // [Header("Settings Popup")]
    // public GameObject settingsPopup;
    // public Toggle soundToggle;

    // private const string SoundPrefKey = "SoundEnabled";
    // private Image _knob;

    void Start()
    {
        if (dropdownPanel == null)
        {
            Debug.LogError("[HamburgerMenu] dropdownPanel not assigned on " + gameObject.name);
            enabled = false;
            return;
        }

        dropdownPanel.SetActive(false);
        // settingsPopup?.SetActive(false);

        // bool soundOn = PlayerPrefs.GetInt(SoundPrefKey, 1) == 1;
        // AudioListener.volume = soundOn ? 1f : 0f;
        // SetupIOSToggle();
        // soundToggle.isOn = soundOn;
        // soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
        // UpdateToggleVisual(soundOn);
    }

    // void SetupIOSToggle() { ... }
    // void UpdateToggleVisual(bool isOn) { ... }

    // ── Hamburger button ──────────────────────────────
    public void OnHamburgerClick()
    {
        bool next = !dropdownPanel.activeSelf;
        dropdownPanel.SetActive(next);

        // Đóng settings popup nếu đang mở
        // if (!next && settingsPopup != null) settingsPopup.SetActive(false);
    }

    // ── Menu item: Settings ───────────────────────────
    public void OnSettingsClick()
    {
        Debug.Log("OnSettingsClick — settings popup commented out");
        dropdownPanel.SetActive(false);
        // if (settingsPopup != null) settingsPopup.SetActive(true);
    }

    // ── Menu item: Quit ───────────────────────────────
    public void OnQuitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Settings Popup: Close ─────────────────────────
    public void OnSettingsClose()
    {
        // if (settingsPopup != null) settingsPopup.SetActive(false);
    }

    // ── Toggle Sound ──────────────────────────────────
    // private void OnSoundToggleChanged(bool isOn)
    // {
    //     AudioListener.volume = isOn ? 1f : 0f;
    //     PlayerPrefs.SetInt(SoundPrefKey, isOn ? 1 : 0);
    //     PlayerPrefs.Save();
    //     UpdateToggleVisual(isOn);
    // }

    // Click bên ngoài dropdown thì đóng
    void Update()
    {
        if (dropdownPanel != null && dropdownPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                    dropdownPanel.GetComponent<RectTransform>(),
                    Input.mousePosition,
                    null))
            {
                dropdownPanel.SetActive(false);
            }
        }
    }
}
