using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DayXepBaiController : MonoBehaviour
{
    [Header("Data")]
    public CardCollection fullDeck;

    // ── Runtime state ──────────────────────────────────────────────────────
    private List<CardData> _hand           = new List<CardData>();
    private List<int>      _selectedIdx    = new List<int>();
    private List<GameObject> _handCards    = new List<GameObject>();

    // Zone contents: 0=Chan, 1=Ca, 2=BaDau
    private List<List<CardData>> _zoneCards  = new List<List<CardData>> {
        new List<CardData>(), new List<CardData>(), new List<CardData>() };

    // ── UI references (built in Start) ────────────────────────────────────
    private Transform        _handContent;
    private Transform[]      _zoneContent    = new Transform[3];
    private GameObject       _feedbackPanel;
    private TMP_Text         _feedbackText;

    // Zone metadata
    private static readonly string[] ZoneNames  = { "Chắn",   "Cạ",   "Ba đầu" };
    private static readonly Color[]  ZoneColors = {
        new Color(0.78f, 0.96f, 0.70f),
        new Color(0.70f, 0.88f, 1.00f),
        new Color(1.00f, 0.90f, 0.60f)
    };
    private static readonly string[] ZoneHints = {
        "Chắn cần 2 cây cùng số cùng màu",
        "Cạ cần 2 cây cùng số khác màu",
        "Ba đầu cần 3 cây cùng số khác màu"
    };

    // ── Constants ─────────────────────────────────────────────────────────
    const int   DEAL_COUNT = 20;
    const float REF_W  = 390f;
    const float REF_H  = 844f;
    const float CARD_W = 54f;
    const float CARD_H = 80f;
    const float LIFT   = 22f;

    // ══════════════════════════════════════════════════════════════════════
    void Start()
    {
        BuildUI();
        Deal();
    }

    // ══════════════════════════════════════════════════════════════════════
    // UI CONSTRUCTION
    // ══════════════════════════════════════════════════════════════════════

    void BuildUI()
    {
        // ── Canvas ────────────────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_W, REF_H);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        Transform root = canvasGO.transform;

        // ── EventSystem (if not already present) ─────────────────────────
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ── Background ────────────────────────────────────────────────────
        var bg = MakeImage("Background", root, new Color(0.96f, 0.92f, 0.80f));
        Stretch(bg.GetComponent<RectTransform>());

        // ── Top bar [0.93 - 1.00] ─────────────────────────────────────────
        BuildTopBar(root);

        // ── Zones [0.50 - 0.92] ───────────────────────────────────────────
        BuildZones(root);

        // ── Hand area [0.08 - 0.49] ───────────────────────────────────────
        BuildHandArea(root);

        // ── Làm lại button [0.00 - 0.07] ─────────────────────────────────
        BuildResetButton(root);

        // ── Feedback overlay (last sibling = on top) ──────────────────────
        BuildFeedbackOverlay(root);
    }

    void BuildTopBar(Transform root)
    {
        var bar   = MakeImage("TopBar", root, new Color(0.25f, 0.15f, 0.05f));
        var barRT = bar.GetComponent<RectTransform>();
        barRT.anchorMin        = new Vector2(0f, 0.93f);
        barRT.anchorMax        = new Vector2(1f, 1.00f);
        barRT.offsetMin        = Vector2.zero;
        barRT.offsetMax        = Vector2.zero;

        // Title
        var title = MakeTMP("Title", bar.transform, "Tập xếp bài", 22,
                            FontStyles.Bold, Color.white);
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin        = new Vector2(0f, 0f);
        titleRT.anchorMax        = new Vector2(0.6f, 1f);
        titleRT.offsetMin        = new Vector2(16f, 0f);
        titleRT.offsetMax        = Vector2.zero;
        title.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.MidlineLeft;

        // Back button
        var backBtn = MakeBtn("BackBtn", bar.transform, "← Quay lại", null, 18,
                              new Color(1f, 0.85f, 0.3f));
        var backRT  = backBtn.GetComponent<RectTransform>();
        backRT.anchorMin        = new Vector2(0.62f, 0.1f);
        backRT.anchorMax        = new Vector2(1.00f, 0.9f);
        backRT.offsetMin        = Vector2.zero;
        backRT.offsetMax        = new Vector2(-8f, 0f);

        var backBtnComp = backBtn.GetComponent<Button>();
        backBtnComp.onClick.AddListener(OnBack);
    }

    void BuildZones(Transform root)
    {
        var zoneParent   = MakeImage("ZoneArea", root, Color.clear);
        var zoneParentRT = zoneParent.GetComponent<RectTransform>();
        zoneParentRT.anchorMin = new Vector2(0f, 0.50f);
        zoneParentRT.anchorMax = new Vector2(1f, 0.93f);
        zoneParentRT.offsetMin = new Vector2(4f,  4f);
        zoneParentRT.offsetMax = new Vector2(-4f, -4f);

        float colW = 1f / 3f;
        for (int z = 0; z < 3; z++)
        {
            int zIdx = z; // capture for lambda

            var zone   = MakeImage($"Zone_{ZoneNames[z]}", zoneParent.transform, ZoneColors[z]);
            var zoneRT = zone.GetComponent<RectTransform>();
            zoneRT.anchorMin = new Vector2(colW * z + 0.005f, 0f);
            zoneRT.anchorMax = new Vector2(colW * (z + 1) - 0.005f, 1f);
            zoneRT.offsetMin = Vector2.zero;
            zoneRT.offsetMax = Vector2.zero;

            // Rounded-ish appearance via outline
            var zoneImg = zone.GetComponent<Image>();
            zoneImg.color = ZoneColors[z];

            // Zone label
            var label    = MakeTMP($"ZoneLabel_{z}", zone.transform, ZoneNames[z],
                                   18, FontStyles.Bold, new Color(0.15f, 0.10f, 0.02f));
            var labelRT  = label.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0f, 0.85f);
            labelRT.anchorMax = new Vector2(1f, 1.00f);
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;
            label.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

            // HorizontalLayoutGroup scroll for placed cards
            var scrollGO = new GameObject($"ZoneScroll_{z}");
            scrollGO.transform.SetParent(zone.transform, false);
            var scrollImg = scrollGO.AddComponent<Image>();
            scrollImg.color = Color.clear;
            var scrollRT  = scrollGO.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0f, 0f);
            scrollRT.anchorMax = new Vector2(1f, 0.85f);
            scrollRT.offsetMin = new Vector2(4f, 4f);
            scrollRT.offsetMax = new Vector2(-4f, -4f);

            var scroll      = scrollGO.AddComponent<ScrollRect>();
            scroll.horizontal    = true;
            scroll.vertical      = false;
            scroll.movementType  = ScrollRect.MovementType.Clamped;

            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(scrollGO.transform, false);
            var contentImg = contentGO.AddComponent<Image>();
            contentImg.color = Color.clear;
            var contentRT = contentGO.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 0f);
            contentRT.anchorMax = new Vector2(0f, 1f);
            contentRT.pivot     = new Vector2(0f, 0.5f);
            contentRT.sizeDelta = new Vector2(0f, 0f);

            var hlg = contentGO.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment       = TextAnchor.MiddleCenter;
            hlg.spacing              = 4f;
            hlg.childControlWidth    = false;
            hlg.childControlHeight   = false;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;

            var csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit   = ContentSizeFitter.FitMode.Unconstrained;

            scroll.content = contentRT;

            _zoneContent[z] = contentRT;

            // Zone tap (on the zone background itself)
            var zoneTapBtn = zone.AddComponent<Button>();
            var cb = zoneTapBtn.colors;
            cb.normalColor      = Color.white;
            cb.highlightedColor = new Color(1f, 1f, 0.7f);
            cb.pressedColor     = new Color(0.85f, 0.85f, 0.50f);
            zoneTapBtn.colors   = cb;
            zoneTapBtn.transition = Selectable.Transition.ColorTint;
            zoneTapBtn.targetGraphic = zoneImg;
            zoneTapBtn.onClick.AddListener(() => OnZoneTap(zIdx));
        }
    }

    void BuildHandArea(Transform root)
    {
        var handBg   = MakeImage("HandBg", root, new Color(0.90f, 0.85f, 0.70f));
        var handBgRT = handBg.GetComponent<RectTransform>();
        handBgRT.anchorMin = new Vector2(0f, 0.08f);
        handBgRT.anchorMax = new Vector2(1f, 0.49f);
        handBgRT.offsetMin = new Vector2(4f, 4f);
        handBgRT.offsetMax = new Vector2(-4f, -4f);

        // Label
        var handLabel = MakeTMP("HandLabel", handBg.transform, "Tay bài", 14,
                                FontStyles.Normal, new Color(0.4f, 0.3f, 0.1f));
        var hlRT = handLabel.GetComponent<RectTransform>();
        hlRT.anchorMin = new Vector2(0f, 0.88f);
        hlRT.anchorMax = new Vector2(1f, 1.00f);
        hlRT.offsetMin = new Vector2(8f, 0f);
        hlRT.offsetMax = Vector2.zero;
        handLabel.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.MidlineLeft;

        // ScrollRect
        var scrollGO = new GameObject("HandScroll");
        scrollGO.transform.SetParent(handBg.transform, false);
        var scrollImg = scrollGO.AddComponent<Image>();
        scrollImg.color = Color.clear;
        var scrollRT  = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0f);
        scrollRT.anchorMax = new Vector2(1f, 0.88f);
        scrollRT.offsetMin = new Vector2(4f, 4f);
        scrollRT.offsetMax = new Vector2(-4f, -4f);

        var scroll     = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal   = true;
        scroll.vertical     = false;
        scroll.movementType = ScrollRect.MovementType.Elastic;

        var contentGO = new GameObject("HandContent");
        contentGO.transform.SetParent(scrollGO.transform, false);
        var contentImg = contentGO.AddComponent<Image>();
        contentImg.color = Color.clear;
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 0f);
        contentRT.anchorMax = new Vector2(0f, 1f);
        contentRT.pivot     = new Vector2(0f, 0.5f);
        contentRT.sizeDelta = new Vector2(0f, 0f);

        var hlg = contentGO.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment       = TextAnchor.MiddleCenter;
        hlg.spacing              = 2f;
        hlg.padding              = new RectOffset(4, 4, 0, 0);
        hlg.childControlWidth    = false;
        hlg.childControlHeight   = false;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;

        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit   = ContentSizeFitter.FitMode.Unconstrained;

        scroll.content = contentRT;
        _handContent   = contentRT;
    }

    void BuildResetButton(Transform root)
    {
        var btnGO = MakeBtn("ResetBtn", root, "Làm lại", null, 20,
                            new Color(0.15f, 0.10f, 0.02f));
        var btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0f, 0f);
        btnRT.anchorMax = new Vector2(1f, 0.07f);
        btnRT.offsetMin = new Vector2(8f, 4f);
        btnRT.offsetMax = new Vector2(-8f, -4f);

        var img = btnGO.GetComponent<Image>();
        img.color = new Color(0.95f, 0.80f, 0.30f);

        btnGO.GetComponent<Button>().onClick.AddListener(OnReset);
    }

    void BuildFeedbackOverlay(Transform root)
    {
        // Semi-transparent full-screen panel
        _feedbackPanel = MakeImage("FeedbackOverlay", root, new Color(0f, 0f, 0f, 0.65f));
        var fbRT = _feedbackPanel.GetComponent<RectTransform>();
        Stretch(fbRT);
        _feedbackPanel.transform.SetAsLastSibling();

        // Card-like inner panel
        var inner   = MakeImage("FeedbackCard", _feedbackPanel.transform, Color.white);
        var innerRT = inner.GetComponent<RectTransform>();
        innerRT.anchorMin = new Vector2(0.08f, 0.35f);
        innerRT.anchorMax = new Vector2(0.92f, 0.65f);
        innerRT.offsetMin = Vector2.zero;
        innerRT.offsetMax = Vector2.zero;

        // Message text
        var msgGO = MakeTMP("FeedbackText", inner.transform, "", 24,
                            FontStyles.Bold, new Color(0.1f, 0.1f, 0.1f));
        _feedbackText = msgGO.GetComponent<TMP_Text>();
        _feedbackText.alignment       = TextAlignmentOptions.Center;
        _feedbackText.enableWordWrapping = true;
        var msgRT = msgGO.GetComponent<RectTransform>();
        msgRT.anchorMin = new Vector2(0f, 0.35f);
        msgRT.anchorMax = new Vector2(1f, 1.00f);
        msgRT.offsetMin = new Vector2(12f, 0f);
        msgRT.offsetMax = new Vector2(-12f, 0f);

        // Continue button
        var contBtn = MakeBtn("ContinueBtn", inner.transform, "Tiếp tục", null, 20,
                              new Color(0.15f, 0.10f, 0.02f));
        var contRT  = contBtn.GetComponent<RectTransform>();
        contRT.anchorMin = new Vector2(0.15f, 0.05f);
        contRT.anchorMax = new Vector2(0.85f, 0.33f);
        contRT.offsetMin = Vector2.zero;
        contRT.offsetMax = Vector2.zero;
        var contImg = contBtn.GetComponent<Image>();
        contImg.color = new Color(0.95f, 0.80f, 0.30f);
        contBtn.GetComponent<Button>().onClick.AddListener(HideFeedback);

        _feedbackPanel.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════════
    // GAME LOGIC
    // ══════════════════════════════════════════════════════════════════════

    void Deal()
    {
        if (fullDeck == null || fullDeck.cards == null || fullDeck.cards.Count < DEAL_COUNT)
        {
            Debug.LogError("[DayXepBai] fullDeck not assigned or not enough cards!");
            return;
        }

        _selectedIdx.Clear();
        _hand.Clear();
        for (int z = 0; z < 3; z++) _zoneCards[z].Clear();

        var pool = new List<CardData>(fullDeck.cards);
        Shuffle(pool);
        _hand = pool.GetRange(0, Mathf.Min(DEAL_COUNT, pool.Count));

        RebuildHandUI();
        RebuildAllZoneUIs();
    }

    void RebuildHandUI()
    {
        // Clear old cards
        foreach (Transform child in _handContent) Destroy(child.gameObject);
        _handCards.Clear();

        for (int i = 0; i < _hand.Count; i++)
        {
            int idx  = i; // capture
            var card = BuildCardGO(_hand[i], _handContent, idx);
            _handCards.Add(card);
        }
    }

    GameObject BuildCardGO(CardData data, Transform parent, int idx)
    {
        // Outer container — HorizontalLayoutGroup positions this, we never move it
        var container = new GameObject($"HandCard_{idx}", typeof(RectTransform));
        container.transform.SetParent(parent, false);
        var cRT = container.GetComponent<RectTransform>();
        cRT.sizeDelta = new Vector2(CARD_W, CARD_H + LIFT); // extra height so lift has room

        // Inner card image — this is what moves up/down on selection
        var go  = new GameObject("Card", typeof(RectTransform));
        go.transform.SetParent(container.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot     = new Vector2(0.5f, 0f);
        rt.sizeDelta        = new Vector2(CARD_W, CARD_H);
        rt.anchoredPosition = new Vector2(CARD_W * 0.5f, 0f); // sit at bottom of container

        var img = go.AddComponent<Image>();
        if (data != null && data.image != null)
        {
            img.sprite = data.image;
            img.type   = Image.Type.Simple;
            img.preserveAspect = true;
        }
        else
        {
            img.color = new Color(0.95f, 0.93f, 0.88f);
        }

        // Name label at the top of the card (always shown)
        if (data != null)
        {
            var lbl   = MakeTMP("CardName", go.transform, data.name, 9,
                                FontStyles.Bold, new Color(0.1f, 0.05f, 0f));
            var lblRT = lbl.GetComponent<RectTransform>();
            lblRT.anchorMin = new Vector2(0f, 0.78f);
            lblRT.anchorMax = new Vector2(1f, 1.00f);
            lblRT.offsetMin = Vector2.zero;
            lblRT.offsetMax = Vector2.zero;
            var tmp = lbl.GetComponent<TMP_Text>();
            tmp.alignment          = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget      = false;
        }

        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1f, 1f, 0.8f);
        cb.pressedColor     = new Color(0.8f, 0.8f, 0.5f);
        btn.colors = cb;
        btn.onClick.AddListener(() => OnCardTap(idx));

        return go; // return inner card (the one we lift)
    }

    void OnCardTap(int idx)
    {
        if (idx < 0 || idx >= _handCards.Count) return;

        bool wasSelected = _selectedIdx.Contains(idx);

        if (wasSelected)
        {
            // Deselect
            _selectedIdx.Remove(idx);
            LiftCard(_handCards[idx], false);
        }
        else
        {
            // Select (max 3)
            if (_selectedIdx.Count >= 3) return;
            _selectedIdx.Add(idx);
            LiftCard(_handCards[idx], true);
        }
    }

    void LiftCard(GameObject card, bool lift)
    {
        if (card == null) return;
        var rt  = card.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(CARD_W * 0.5f, lift ? LIFT : 0f);
    }

    void OnZoneTap(int zoneIdx)
    {
        if (_selectedIdx.Count == 0) return;

        var sel = _selectedIdx.OrderBy(i => i).Select(i => _hand[i]).ToList();

        bool valid = false;
        switch (zoneIdx)
        {
            case 0: valid = IsChan(sel);  break;
            case 1: valid = IsCa(sel);    break;
            case 2: valid = IsBaDau(sel); break;
        }

        if (valid)
        {
            // Move cards to zone
            foreach (var c in sel)
                _zoneCards[zoneIdx].Add(c);

            // Remove from hand (reverse order to keep indices stable)
            foreach (int i in _selectedIdx.OrderByDescending(x => x))
            {
                Destroy(_handCards[i]);
                _handCards.RemoveAt(i);
                _hand.RemoveAt(i);
            }
            _selectedIdx.Clear();

            RebuildHandUI();
            RebuildZoneUI(zoneIdx);

            ShowFeedback(true, $"Đúng! Đây là {ZoneNames[zoneIdx]}");
        }
        else
        {
            // Return selected cards to hand (deselect)
            foreach (int i in _selectedIdx)
                LiftCard(_handCards[i], false);
            _selectedIdx.Clear();

            ShowFeedback(false, $"Sai! Cần {ZoneHints[zoneIdx]}");
        }
    }

    void OnReset()
    {
        Deal();
    }

    void OnBack()
    {
        SceneManager.LoadScene("xep_bai");
    }

    void ShowFeedback(bool correct, string message)
    {
        _feedbackText.text  = (correct ? "✅ " : "❌ ") + message;
        _feedbackText.color = correct
            ? new Color(0.05f, 0.50f, 0.05f)
            : new Color(0.70f, 0.05f, 0.05f);
        _feedbackPanel.SetActive(true);
        _feedbackPanel.transform.SetAsLastSibling();
    }

    void HideFeedback()
    {
        _feedbackPanel.SetActive(false);
    }

    void RebuildAllZoneUIs()
    {
        for (int z = 0; z < 3; z++) RebuildZoneUI(z);
    }

    void RebuildZoneUI(int zoneIdx)
    {
        var content = _zoneContent[zoneIdx];
        if (content == null) return;
        foreach (Transform child in content) Destroy(child.gameObject);

        foreach (var card in _zoneCards[zoneIdx])
        {
            var go  = new GameObject("ZoneCard", typeof(RectTransform));
            go.transform.SetParent(content, false);
            var rt  = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(CARD_W * 0.8f, CARD_H * 0.8f);
            var img = go.AddComponent<Image>();
            if (card.image != null)
            {
                img.sprite         = card.image;
                img.type           = Image.Type.Simple;
                img.preserveAspect = true;
            }
            else
            {
                img.color = new Color(0.92f, 0.90f, 0.80f);
                var lbl = MakeTMP("Label", go.transform, $"{card.rank}\n{card.type}", 9,
                                  FontStyles.Normal, new Color(0.2f, 0.1f, 0f));
                var lblRT = lbl.GetComponent<RectTransform>();
                lblRT.anchorMin = Vector2.zero;
                lblRT.anchorMax = Vector2.one;
                lblRT.offsetMin = Vector2.zero;
                lblRT.offsetMax = Vector2.zero;
                lbl.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // VALIDATION
    // ══════════════════════════════════════════════════════════════════════

    bool IsChan(List<CardData> sel)  =>
        sel.Count == 2 && sel[0].rank == sel[1].rank && sel[0].type == sel[1].type;

    bool IsCa(List<CardData> sel)    =>
        sel.Count == 2 && sel[0].rank == sel[1].rank && sel[0].type != sel[1].type;

    bool IsBaDau(List<CardData> sel) =>
        sel.Count == 3 &&
        sel.All(c => c.rank == sel[0].rank) &&
        sel.Select(c => c.type).Distinct().Count() == 3;

    // ══════════════════════════════════════════════════════════════════════
    // UTILITIES
    // ══════════════════════════════════════════════════════════════════════

    static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r   = Random.Range(i, list.Count);
            T   tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static GameObject MakeImage(string name, Transform parent, Color color)
    {
        var go  = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static GameObject MakeBtn(string name, Transform parent, string label,
                               Sprite sprite, int fontSize, Color textColor)
    {
        var go  = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Simple; }
        else                { img.color  = Color.white; }

        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1f, 1f, 0.7f);
        cb.pressedColor     = new Color(0.8f, 0.8f, 0.4f);
        btn.colors = cb;

        var lbl = MakeTMP("Label", go.transform, label, fontSize, FontStyles.Normal, textColor);
        var lblRT = lbl.GetComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero;
        lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = Vector2.zero;
        lblRT.offsetMax = Vector2.zero;
        lbl.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

        return go;
    }

    static GameObject MakeTMP(string name, Transform parent, string text,
                               int size, FontStyles style, Color color)
    {
        var go  = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text          = text;
        tmp.fontSize      = size;
        tmp.fontStyle     = style;
        tmp.color         = color;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = true;
        tmp.overflowMode  = TextOverflowModes.Ellipsis;
        return go;
    }
}
