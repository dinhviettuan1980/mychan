using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoSamplesController : MonoBehaviour
{
    [SerializeField] private VideoClip[] videoClips;
    [SerializeField] private string[] videoDisplayNames;

    private GameObject        listPanel;
    private GameObject        playerPanel;
    private RawImage          videoDisplay;
    private RectTransform     videoDisplayRT;
    private VideoPlayer       videoPlayer;
    private RenderTexture     renderTex;

    private Sprite _btnSprite;
    private Sprite _bgSprite;
    private Font   _font;

    void Start()
    {
        _btnSprite = Resources.Load<Sprite>("button_yellow_bg");
        _font      = Resources.Load<Font>("UTM ThuPhap Thien An");
        _bgSprite  = Resources.Load<Sprite>("background");

        GameObject canvasGO = new GameObject("VSCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;

        BuildListPanel(canvas.transform);
        BuildPlayerPanel(canvas.transform);
    }

    // ── List panel ────────────────────────────────────────────────────────
    void BuildListPanel(Transform canvasT)
    {
        listPanel = UIPanel(canvasT, "ListPanel", Color.white);
        FillParent(listPanel);
        if (_bgSprite != null)
        {
            Image bg = listPanel.GetComponent<Image>();
            bg.sprite = _bgSprite;
            bg.type   = Image.Type.Simple;
        }

        // Title — top ~21% of screen (3× taller than before)
        GameObject titleGO = UIText(listPanel.transform, "Title",
            "Video chơi mẫu", 120, FontStyle.Bold,
            new Color(0f, 0f, 0f), TextAnchor.MiddleCenter, _font);
        RectTransform tRT = titleGO.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0.05f, 0.76f);
        tRT.anchorMax = new Vector2(0.95f, 0.97f);
        tRT.offsetMin = Vector2.zero;
        tRT.offsetMax = Vector2.zero;

        // Buttons — fixed width centred, evenly spaced (startY pushed down to clear bigger title)
        int   n      = videoClips != null ? videoClips.Length : 0;
        float startY = -300f;
        float step   = 160f;

        for (int i = 0; i < n; i++)
        {
            if (videoClips[i] == null) continue;
            string lbl = videoDisplayNames != null && i < videoDisplayNames.Length
                         && !string.IsNullOrEmpty(videoDisplayNames[i])
                         ? videoDisplayNames[i] : videoClips[i].name;
            int captured = i;
            AddButton(listPanel.transform, lbl,
                new Vector2(0, startY - step * i), new Vector2(580, 120),
                new Color(1f, 0.843f, 0.471f, 0.784f),
                () => PlayVideo(videoClips[captured]),
                textOffsetY: -20f);
        }

        // Back button
        AddButton(listPanel.transform, "← Quay lại",
            new Vector2(0, startY - step * n), new Vector2(580, 110),
            new Color(0.25f, 0.65f, 1f),
            () => SceneManager.LoadScene("MiniGameMenu"),
            textOffsetY: -20f);
    }

    // ── Player panel ──────────────────────────────────────────────────────
    void BuildPlayerPanel(Transform canvasT)
    {
        playerPanel = UIPanel(canvasT, "PlayerPanel", Color.black);
        FillParent(playerPanel);

        GameObject rawGO = new GameObject("VideoDisplay", typeof(RectTransform), typeof(RawImage));
        rawGO.transform.SetParent(playerPanel.transform, false);

        // Rotated 90°: local X = visual height, local Y = visual width
        videoDisplayRT                 = rawGO.GetComponent<RectTransform>();
        videoDisplayRT.anchorMin        = new Vector2(0.5f, 0.5f);
        videoDisplayRT.anchorMax        = new Vector2(0.5f, 0.5f);
        videoDisplayRT.pivot            = new Vector2(0.5f, 0.5f);
        videoDisplayRT.anchoredPosition = Vector2.zero;
        videoDisplayRT.sizeDelta        = new Vector2(Screen.height, Screen.height); // placeholder
        // rotation is set per-video in PlayVideo based on clip orientation

        videoDisplay       = rawGO.GetComponent<RawImage>();
        videoDisplay.color = Color.white;

        // Close button: top-right corner
        AddButton(playerPanel.transform, "Đóng",
          new Vector2(-60, -60), new Vector2(160, 100),
          new Color(1f, 0.92f, 0.016f, 0.71f),
          ClosePlayer,
          new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
          Color.black, -20f);

        playerPanel.SetActive(false);
    }

    // ── Actions ───────────────────────────────────────────────────────────
    void PlayVideo(VideoClip clip)
    {
        listPanel.SetActive(false);
        playerPanel.SetActive(true);

        if (videoPlayer != null) Destroy(videoPlayer);
        if (renderTex   != null) { renderTex.Release(); Destroy(renderTex); }

        int w = clip.width  > 0 ? (int)clip.width  : 1920;
        int h = clip.height > 0 ? (int)clip.height : 1080;
        renderTex = new RenderTexture(w, h, 0);

        if (h > w)
        {
            // Portrait video: rotate -90° so it appears landscape, height fills screen
            videoDisplayRT.localRotation = Quaternion.Euler(0, 0, -90);
            // After -90° rotation: sizeDelta.x = visual height, sizeDelta.y = visual width
            float aspect = (float)h / Mathf.Max(w, 1);
            videoDisplayRT.sizeDelta = new Vector2(Screen.height, Screen.height * aspect);
        }
        else
        {
            // Landscape video: no rotation needed, fit screen height
            videoDisplayRT.localRotation = Quaternion.identity;
            float aspect = (float)w / Mathf.Max(h, 1);
            videoDisplayRT.sizeDelta = new Vector2(Screen.height * aspect, Screen.height);
        }

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake     = false;
        videoPlayer.isLooping       = false;
        videoPlayer.renderMode      = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture   = renderTex;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.clip            = clip;

        videoDisplay.texture = renderTex;
        videoPlayer.Play();
    }

    void ClosePlayer()
    {

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            Destroy(videoPlayer);
            videoPlayer = null;
        }
        if (renderTex != null)
        {
            renderTex.Release();
            Destroy(renderTex);
            renderTex = null;
        }
        videoDisplay.texture = null;
        playerPanel.SetActive(false);
        listPanel.SetActive(true);
    }

    void OnDestroy()
    {
        if (renderTex != null) { renderTex.Release(); Destroy(renderTex); }
    }

    // ── UI helpers ────────────────────────────────────────────────────────
    static void FillParent(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static GameObject UIPanel(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    static GameObject UIText(Transform parent, string name,
        string text, int size, FontStyle style, Color color, TextAnchor align,
        Font overrideFont = null)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text t = go.GetComponent<Text>();
        t.text                 = text;
        t.font                 = overrideFont ?? Font.CreateDynamicFontFromOSFont("Arial", size);
        t.fontSize             = size;
        t.fontStyle            = style;
        t.alignment            = align;
        t.color                = color;
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize    = 8;
        t.resizeTextMaxSize    = 300;
        return go;
    }

    void AddButton(Transform parent, string label,
                   Vector2 pos, Vector2 size, Color color,
                   UnityEngine.Events.UnityAction onClick,
                   Vector2 anchorMin  = default, Vector2 anchorMax = default,
                   Vector2 pivot      = default,
                   Color labelColor   = default,
                   float textOffsetY  = 0f)
    {
        if (anchorMin   == default) anchorMin   = new Vector2(0.5f, 1f);
        if (anchorMax   == default) anchorMax   = new Vector2(0.5f, 1f);
        if (pivot       == default) pivot       = new Vector2(0.5f, 0.5f);
        if (labelColor  == default) labelColor  = new Color(0.15f, 0.08f, 0f);

        GameObject go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.pivot            = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;

        ApplyButtonStyle(go, color, onClick);
        GameObject lblGO = UIText(go.transform, "Lbl", label, 28, FontStyle.Bold,
            labelColor, TextAnchor.MiddleCenter, _font);
        FillParent(lblGO);
        if (textOffsetY != 0f)
        {
            RectTransform lblRT = lblGO.GetComponent<RectTransform>();
            lblRT.offsetMin = new Vector2(0, textOffsetY);
            lblRT.offsetMax = new Vector2(0, textOffsetY);
        }
    }

    void AddButtonAnchored(Transform parent, string label,
                           Vector2 anchorMin, Vector2 anchorMax,
                           Color color,
                           UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        ApplyButtonStyle(go, color, onClick);
        FillParent(UIText(go.transform, "Lbl", label, 40, FontStyle.Bold,
            new Color(0.15f, 0.08f, 0f), TextAnchor.MiddleCenter, _font));
    }

    void ApplyButtonStyle(GameObject go, Color color,
                          UnityEngine.Events.UnityAction onClick)
    {
        Image img = go.GetComponent<Image>();
        img.color = color;
        if (_btnSprite != null)
        {
            img.sprite = _btnSprite;
            img.type   = Image.Type.Sliced;
        }

        Button btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.normalColor      = color;
        cb.highlightedColor = Color.Lerp(color, Color.white, 0.25f);
        cb.pressedColor     = Color.Lerp(color, Color.black, 0.25f);
        btn.colors = cb;
        btn.onClick.AddListener(onClick);
    }
}
