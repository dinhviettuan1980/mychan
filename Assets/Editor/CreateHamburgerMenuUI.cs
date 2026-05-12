using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class CreateHamburgerMenuUI
{
    // ── Layout constants ──────────────────────────────────────────────────────
    const float PANEL_W   = 280f;
    const float H_HEADER  = 56f;
    const float H_ITEM    = 64f;
    const float H_DIV     = 1f;
    const float H_PADDING = 12f;
    const float INDENT    = 16f;

    // collapsed = header + 4 items + 4 dividers + huong dan row + 2 dividers + 2 items + padding
    const float H_COLLAPSED = H_HEADER + H_ITEM*4 + H_DIV*4 + H_ITEM + H_DIV*2 + H_ITEM*2 + H_PADDING;
    // expanded  = collapsed + 4 sub-items
    const float H_EXPANDED  = H_COLLAPSED + H_ITEM * 4;

    // ── Colors ────────────────────────────────────────────────────────────────
    static Color C_PANEL   = new Color(0f, 0f, 0f, 0f);
    static Color C_HEADER  = new Color(0f, 0f, 0f, 0f);
    static Color C_OVERLAY = new Color(0f, 0f, 0f, 0.45f);
    static Color C_DIVIDER = new Color(0.80f, 0.75f, 0.55f, 1f);
    static Color C_TEXT    = new Color(0.15f, 0.10f, 0.02f, 1f);
    static Color C_SECTION = new Color(0.45f, 0.28f, 0.00f, 1f);
    static Color C_HOVER   = new Color(1.00f, 0.95f, 0.70f, 1f);
    static Color C_PRESS   = new Color(0.85f, 0.78f, 0.45f, 1f);
    static Color C_HAM_BG  = new Color(0.95f, 0.85f, 0.40f, 1f);

    // ── Entry point ───────────────────────────────────────────────────────────
    [MenuItem("MyChan/Create Hamburger Menu UI")]
    static void Create()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("[HamburgerMenu] Canvas not found!"); return; }
        Transform root = canvas.transform;

        Sprite btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/button_yellow_bg.png");
        TMP_FontAsset tmpFont = null; // use TMP default

        // ── Overlay ───────────────────────────────────────────────────────────
        var overlay = MakeImage("MenuOverlay", root, C_OVERLAY);
        Stretch(overlay.GetComponent<RectTransform>());
        var overlayBtn = overlay.AddComponent<Button>();
        overlayBtn.transition = Selectable.Transition.None;
        overlay.SetActive(false);

        // ── Slide panel (anchored top-right, fixed size) ───────────────────────
        var panel = MakeImage("HamburgerPanel", root, C_PANEL);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(1f, 1f);
        panelRT.anchorMax        = new Vector2(1f, 1f);
        panelRT.pivot            = new Vector2(1f, 1f);
        panelRT.sizeDelta        = new Vector2(PANEL_W, H_COLLAPSED);
        panelRT.anchoredPosition = new Vector2(PANEL_W, 0f); // Y computed dynamically at runtime

        // ── Header ────────────────────────────────────────────────────────────
        var header = MakeImage("Header", panel.transform, C_HEADER);
        AnchorTop(header.GetComponent<RectTransform>(), 0f, H_HEADER);
        MakeTMP("Title", header.transform, "Menu", 22, FontStyles.Bold, C_TEXT)
            .GetComponent<RectTransform>().Set(0f, 1f, 0f, 1f, new Vector2(12f, 0f), Vector2.zero);

        var closeBtn = MakeBtn("CloseBtn", header.transform, "✕", btnSprite, 22, C_TEXT);
        closeBtn.GetComponent<RectTransform>()
                .Set(1f, 0f, 1f, 1f, Vector2.zero, new Vector2(-H_HEADER, 0f));

        // ── Items layout (y goes downward from top of panel) ──────────────────
        float y = -H_HEADER;

        var btn1 = AddItem(panel.transform, btnSprite, "Thử lại",       ref y, H_ITEM, 0f, C_TEXT);
        AddDivider(panel.transform, ref y);
        var btn2 = AddItem(panel.transform, btnSprite, "Xếp chuẩn",    ref y, H_ITEM, 0f, C_TEXT);
        AddDivider(panel.transform, ref y);
        var btn3 = AddItem(panel.transform, btnSprite, "Kiểm tra",     ref y, H_ITEM, 0f, C_TEXT);
        AddDivider(panel.transform, ref y);
        var btn4 = AddItem(panel.transform, btnSprite, "Thủ chọn bài", ref y, H_ITEM, 0f, C_TEXT);
        AddDivider(panel.transform, ref y);

        // ── Hướng dẫn toggle row ─────────────────────────────────────────────
        var hdRow = MakeImage("HuongDanRow", panel.transform, new Color(1f,1f,1f,0f));
        AnchorTop(hdRow.GetComponent<RectTransform>(), y, H_ITEM);
        y -= H_ITEM;

        // Arrow label (right side)
        var arrow = MakeTMP("HuongDanArrow", hdRow.transform, "▶", 20, FontStyles.Normal, C_SECTION);
        arrow.GetComponent<RectTransform>()
             .Set(1f, 0f, 1f, 1f, Vector2.zero, new Vector2(-12f, 0f));
        arrow.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.MidlineRight;

        var hdBtn = MakeBtn("HuongDanBtn", hdRow.transform, "Hướng dẫn", btnSprite, 20, C_SECTION);
        hdBtn.GetComponent<RectTransform>().Set(0f, 0f, 1f, 1f, Vector2.zero, Vector2.zero);
        var hdLabel = hdBtn.GetComponentInChildren<TMP_Text>();
        if (hdLabel) { hdLabel.alignment = TextAlignmentOptions.Center;
                       hdLabel.GetComponent<RectTransform>().Set(0f,0f,1f,1f, Vector2.zero, Vector2.zero); }

        // ── Sub-menu container (starts inactive) ──────────────────────────────
        var subContainer = new GameObject("SubMenuContainer");
        subContainer.transform.SetParent(panel.transform, false);
        var subRT = subContainer.AddComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0f, 1f);
        subRT.anchorMax = new Vector2(1f, 1f);
        subRT.pivot     = new Vector2(0.5f, 1f);
        subRT.sizeDelta = new Vector2(0f, H_ITEM * 4);
        subRT.anchoredPosition = new Vector2(0f, y);

        float sy = 0f;
        var sb1 = AddSubItem(subContainer.transform, btnSprite, "Cách xếp",            ref sy, H_ITEM);
        var sb2 = AddSubItem(subContainer.transform, btnSprite, "Cách nhớ quân bài",   ref sy, H_ITEM);
        var sb3 = AddSubItem(subContainer.transform, btnSprite, "Nhận diện quân bài",  ref sy, H_ITEM);
        var sb4 = AddSubItem(subContainer.transform, btnSprite, "Video mẫu",           ref sy, H_ITEM);
        subContainer.SetActive(false);

        // ── Âm thanh + Thoát — anchored from BOTTOM so sub-menu pushes them down ─
        // From bottom: padding | Thoát | divider | Âm thanh | divider
        var btnThoat = AddBottomItem(panel.transform, btnSprite, "Thoát",       H_PADDING,                       H_ITEM, C_TEXT);
        AddBottomDivider(panel.transform,                                        H_PADDING + H_ITEM);
        var btnAm    = AddBottomItem(panel.transform, btnSprite, "Âm thanh: On", H_PADDING + H_ITEM + H_DIV,     H_ITEM, C_TEXT);
        AddBottomDivider(panel.transform,                                        H_PADDING + H_ITEM*2 + H_DIV);

        // ── Hamburger trigger button (top-right, always visible) ──────────────
        var hamGO = MakeBtn("HamburgerBtn", root, "☰", btnSprite, 28, C_TEXT);
        var hamImg = hamGO.GetComponent<Image>();
        hamImg.color = C_HAM_BG;
        var hamRT = hamGO.GetComponent<RectTransform>();
        hamRT.anchorMin = hamRT.anchorMax = new Vector2(1f, 1f);
        hamRT.pivot     = new Vector2(1f, 1f);
        hamRT.sizeDelta = new Vector2(68f, 68f);
        hamRT.anchoredPosition = new Vector2(-10f, -40f);

        // ── Wire HamburgerMenu component ──────────────────────────────────────
        var hm = hamGO.AddComponent<HamburgerMenu>();
        var so = new SerializedObject(hm);
        so.FindProperty("menuPanel")          .objectReferenceValue = panelRT;
        so.FindProperty("overlay")            .objectReferenceValue = overlay;
        so.FindProperty("subMenuContainer")   .objectReferenceValue = subContainer;
        so.FindProperty("huongDanArrow")      .objectReferenceValue = arrow.GetComponent<TMP_Text>();
        so.FindProperty("audioLabel")         .objectReferenceValue = btnAm.GetComponentInChildren<TMP_Text>();
        so.FindProperty("panelHeightCollapsed").floatValue = H_COLLAPSED;
        so.FindProperty("panelHeightExpanded") .floatValue = H_EXPANDED;
        so.ApplyModifiedProperties();

        // ── Wire button clicks ────────────────────────────────────────────────
        Wire(hamGO .GetComponent<Button>(), hm, "ToggleMenu");
        Wire(overlayBtn,                    hm, "Close");
        Wire(closeBtn.GetComponent<Button>(), hm, "Close");
        Wire(btn1.GetComponent<Button>(), hm, "OnThuLai");
        Wire(btn2.GetComponent<Button>(), hm, "OnXepChuan");
        Wire(btn3.GetComponent<Button>(), hm, "OnKiemTra");
        Wire(btn4.GetComponent<Button>(), hm, "OnThuChonBai");
        Wire(hdBtn.GetComponent<Button>(), hm, "ToggleHuongDan");
        Wire(sb1.GetComponent<Button>(), hm, "OnCachXep");
        Wire(sb2.GetComponent<Button>(), hm, "OnCachNho");
        Wire(sb3.GetComponent<Button>(), hm, "OnNhanDien");
        Wire(sb4.GetComponent<Button>(), hm, "OnVideoMau");
        Wire(btnAm   .GetComponent<Button>(), hm, "OnAmThanh");
        Wire(btnThoat.GetComponent<Button>(), hm, "OnThoat");

        Selection.activeGameObject = hamGO;
        EditorUtility.SetDirty(canvas.gameObject);
        Debug.Log($"[HamburgerMenu] Done! H_COLLAPSED={H_COLLAPSED} H_EXPANDED={H_EXPANDED}. Assign CardManager + HelpManager in Inspector.");
    }

    // ── Item helpers ──────────────────────────────────────────────────────────

    static GameObject AddItem(Transform parent, Sprite sprite, string label,
                               ref float y, float h, float indent, Color textColor)
    {
        var btn = MakeBtn($"Item_{label.Replace(" ","_")}", parent, label, sprite, 22, textColor);
        var rt  = btn.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(-indent * 2f, h);
        rt.anchoredPosition = new Vector2(indent, y);
        y -= h;
        return btn;
    }

    static GameObject AddSubItem(Transform parent, Sprite sprite, string label,
                                  ref float y, float h)
    {
        var btn = MakeBtn($"Sub_{label.Replace(" ","_")}", parent, label, sprite, 22, C_TEXT);
        var rt  = btn.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(-INDENT * 2f, h);
        rt.anchoredPosition = new Vector2(INDENT, y);
        y -= h;
        var tmp = btn.GetComponentInChildren<TMP_Text>();
        if (tmp) {
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.GetComponent<RectTransform>().Set(0f,0f,1f,1f, Vector2.zero, Vector2.zero);
        }
        // same background style as top items
        return btn;
    }

    static void AddDivider(Transform parent, ref float y)
    {
        var go = MakeImage("Divider", parent, C_DIVIDER);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 1f); rt.anchorMax = new Vector2(0.95f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(0f, H_DIV);
        rt.anchoredPosition = new Vector2(0f, y);
        y -= H_DIV;
    }

    // Items anchored from the BOTTOM of the panel — stay at bottom as panel grows
    static GameObject AddBottomItem(Transform parent, Sprite sprite, string label,
                                    float fromBottom, float h, Color textColor)
    {
        var btn = MakeBtn($"Item_{label.Replace(" ","_")}", parent, label, sprite, 22, textColor);
        var rt  = btn.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 0f);
        rt.anchorMax        = new Vector2(1f, 0f);
        rt.pivot            = new Vector2(0.5f, 0f);
        rt.sizeDelta        = new Vector2(0f, h);
        rt.anchoredPosition = new Vector2(0f, fromBottom);
        return btn;
    }

    static void AddBottomDivider(Transform parent, float fromBottom)
    {
        var go = MakeImage("Divider", parent, C_DIVIDER);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.05f, 0f);
        rt.anchorMax        = new Vector2(0.95f, 0f);
        rt.pivot            = new Vector2(0.5f, 0f);
        rt.sizeDelta        = new Vector2(0f, H_DIV);
        rt.anchoredPosition = new Vector2(0f, fromBottom);
    }

    // ── UI primitives ─────────────────────────────────────────────────────────

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
        if (sprite) { img.sprite = sprite; img.type = Image.Type.Simple; }
        img.color = Color.white;

        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = C_HOVER;
        cb.pressedColor     = C_PRESS;
        cb.selectedColor    = Color.white;
        cb.colorMultiplier  = 1f;
        btn.colors = cb;

        var lbl = MakeTMP("Label", go.transform, label, fontSize, FontStyles.Normal, textColor);
        lbl.GetComponent<RectTransform>().Set(0f, 0f, 1f, 1f, Vector2.zero, Vector2.zero);
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
        tmp.enableWordWrapping = false;
        tmp.overflowMode  = TextOverflowModes.Ellipsis;
        return go;
    }

    // ── Layout helpers ────────────────────────────────────────────────────────

    static void AnchorTop(RectTransform rt, float y, float h)
    {
        rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(0f, h);
        rt.anchoredPosition = new Vector2(0f, y);
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void Wire(Button btn, HamburgerMenu target, string method)
    {
        if (btn == null) return;
        var so     = new SerializedObject(btn);
        var calls  = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        calls.arraySize++;
        var call   = calls.GetArrayElementAtIndex(calls.arraySize - 1);
        call.FindPropertyRelative("m_Target")                 .objectReferenceValue = target;
        call.FindPropertyRelative("m_TargetAssemblyTypeName") .stringValue =
            target.GetType().AssemblyQualifiedName;
        call.FindPropertyRelative("m_MethodName")             .stringValue = method;
        call.FindPropertyRelative("m_Mode")                   .enumValueIndex = 1;
        call.FindPropertyRelative("m_CallState")              .enumValueIndex = 2;
        so.ApplyModifiedProperties();
    }
}

// Extension to set RectTransform quickly
static class RectTransformExt
{
    public static void Set(this RectTransform rt,
                           float anchorMinX, float anchorMinY,
                           float anchorMaxX, float anchorMaxY,
                           Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = new Vector2(anchorMinX, anchorMinY);
        rt.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }
}
