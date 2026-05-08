using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class CreateQuizPopupUI
{
    [MenuItem("MyChan/Create Quiz Popup UI")]
    static void Create()
    {
        // ── Find Canvas ───────────────────────────────────────────────────────
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[CreateQuizPopupUI] Không tìm thấy Canvas trong scene!");
            return;
        }
        Transform canvasTF = canvas.transform;

        // ── Root panel ────────────────────────────────────────────────────────
        GameObject root = MakePanel("QuizPopupRoot", canvasTF,
            new Color(0f, 0f, 0f, 0.75f));
        StretchFull(root.GetComponent<RectTransform>());

        // ── Card (white box centered) ─────────────────────────────────────────
        GameObject card = MakePanel("Card", root.transform, new Color(1f, 1f, 1f, 0.97f));
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.1f, 0.25f);
        cardRT.anchorMax = new Vector2(0.9f, 0.75f);
        cardRT.offsetMin = cardRT.offsetMax = Vector2.zero;

        // ── Title ─────────────────────────────────────────────────────────────
        var title = MakeTMP("TitleText", card.transform, "Kiểu Ù — Đúng hay Sai?",
            28, FontStyles.Bold, new Color(0.1f, 0.1f, 0.5f));
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.05f, 0.72f);
        titleRT.anchorMax = new Vector2(0.95f, 0.95f);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        // ── Question text ─────────────────────────────────────────────────────
        var questionGO = MakeTMP("QuestionText", card.transform,
            "Câu hỏi sẽ hiện ở đây...", 22, FontStyles.Normal, Color.black);
        questionGO.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
        var qRT = questionGO.GetComponent<RectTransform>();
        qRT.anchorMin = new Vector2(0.05f, 0.38f);
        qRT.anchorMax = new Vector2(0.95f, 0.70f);
        qRT.offsetMin = qRT.offsetMax = Vector2.zero;

        // ── Answer panel ──────────────────────────────────────────────────────
        GameObject answerPanel = new GameObject("AnswerPanel");
        answerPanel.transform.SetParent(card.transform, false);
        var apRT = answerPanel.AddComponent<RectTransform>();
        apRT.anchorMin = new Vector2(0.05f, 0.05f);
        apRT.anchorMax = new Vector2(0.95f, 0.35f);
        apRT.offsetMin = apRT.offsetMax = Vector2.zero;

        GameObject dungBtn = MakeButton("DungButton", answerPanel.transform,
            "✅  Đúng", new Color(0.1f, 0.7f, 0.2f));
        var dbRT = dungBtn.GetComponent<RectTransform>();
        dbRT.anchorMin = new Vector2(0f, 0f);
        dbRT.anchorMax = new Vector2(0.47f, 1f);
        dbRT.offsetMin = dbRT.offsetMax = Vector2.zero;

        GameObject saiBtn = MakeButton("SaiButton", answerPanel.transform,
            "❌  Sai", new Color(0.85f, 0.15f, 0.15f));
        var sbRT = saiBtn.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(0.53f, 0f);
        sbRT.anchorMax = new Vector2(1f,    1f);
        sbRT.offsetMin = sbRT.offsetMax = Vector2.zero;

        // ── Feedback panel ────────────────────────────────────────────────────
        GameObject feedbackPanel = MakePanel("FeedbackPanel", card.transform,
            new Color(0.95f, 0.95f, 0.95f, 1f));
        var fpRT = feedbackPanel.GetComponent<RectTransform>();
        fpRT.anchorMin = new Vector2(0.05f, 0.05f);
        fpRT.anchorMax = new Vector2(0.95f, 0.68f);
        fpRT.offsetMin = fpRT.offsetMax = Vector2.zero;
        feedbackPanel.SetActive(false);

        var feedbackGO = MakeTMP("FeedbackText", feedbackPanel.transform,
            "", 20, FontStyles.Normal, Color.black);
        feedbackGO.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.TopLeft;
        var fbRT = feedbackGO.GetComponent<RectTransform>();
        fbRT.anchorMin = new Vector2(0.05f, 0.25f);
        fbRT.anchorMax = new Vector2(0.95f, 0.97f);
        fbRT.offsetMin = fbRT.offsetMax = Vector2.zero;

        GameObject closeBtn = MakeButton("CloseButton", feedbackPanel.transform,
            "Tiếp tục →", new Color(0.2f, 0.45f, 0.85f));
        var cbRT = closeBtn.GetComponent<RectTransform>();
        cbRT.anchorMin = new Vector2(0.25f, 0.02f);
        cbRT.anchorMax = new Vector2(0.75f, 0.22f);
        cbRT.offsetMin = cbRT.offsetMax = Vector2.zero;

        // ── Wire QuizPopup component ──────────────────────────────────────────
        var popup = root.AddComponent<QuizPopup>();
        var so    = new SerializedObject(popup);
        so.FindProperty("root")          .objectReferenceValue = root;
        so.FindProperty("questionText")  .objectReferenceValue = questionGO .GetComponent<TMP_Text>();
        so.FindProperty("answerPanel")   .objectReferenceValue = answerPanel;
        so.FindProperty("dungButton")    .objectReferenceValue = dungBtn    .GetComponent<Button>();
        so.FindProperty("saiButton")     .objectReferenceValue = saiBtn     .GetComponent<Button>();
        so.FindProperty("feedbackPanel") .objectReferenceValue = feedbackPanel;
        so.FindProperty("feedbackText")  .objectReferenceValue = feedbackGO .GetComponent<TMP_Text>();
        so.FindProperty("closeButton")   .objectReferenceValue = closeBtn   .GetComponent<Button>();
        so.ApplyModifiedProperties();

        root.SetActive(false);

        // ── Select in hierarchy ───────────────────────────────────────────────
        Selection.activeGameObject = root;
        EditorUtility.SetDirty(root);
        Debug.Log("[CreateQuizPopupUI] ✅ Tạo QuizPopupRoot thành công! Gán nó vào field 'quizPopup' của CardManager.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static GameObject MakePanel(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static GameObject MakeTMP(string name, Transform parent, string text,
                               float size, FontStyles style, Color color)
    {
        var go  = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text       = text;
        tmp.fontSize   = size;
        tmp.fontStyle  = style;
        tmp.color      = color;
        tmp.enableWordWrapping = true;
        return go;
    }

    static GameObject MakeButton(string name, Transform parent, string label, Color bgColor)
    {
        var go  = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();

        var cb = new ColorBlock();
        cb.normalColor      = bgColor;
        cb.highlightedColor = bgColor * 1.15f;
        cb.pressedColor     = bgColor * 0.8f;
        cb.selectedColor    = bgColor;
        cb.disabledColor    = new Color(0.5f, 0.5f, 0.5f);
        cb.colorMultiplier  = 1f;
        cb.fadeDuration     = 0.1f;
        btn.colors          = cb;

        var lblGO = new GameObject("Label", typeof(RectTransform));
        lblGO.transform.SetParent(go.transform, false);
        var rt = lblGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 22;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        return go;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
