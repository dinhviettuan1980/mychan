using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;

public class HamburgerMenuSetup : Editor
{
    [MenuItem("Tools/Setup Hamburger Menu")]
    static void Setup()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas trong scene!");
            return;
        }

        Transform old = canvas.transform.Find("HamburgerMenu");
        if (old != null) DestroyImmediate(old.gameObject);

        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject root = new GameObject("HamburgerMenu");
        root.transform.SetParent(canvas.transform, false);
        SetStretch(root);

        HamburgerMenuController ctrl = root.AddComponent<HamburgerMenuController>();

        // ── Nút hamburger (UPDATED) ───────────────────────────────────────
        GameObject btnHam = MakeButton("BtnHamburger", root.transform, "≡", 42, font);
        btnHam.GetComponent<Image>().color = new Color(0.95f, 0.9f, 0.7f, 0.7f); // alpha ~180
        btnHam.GetComponentInChildren<Text>().color = new Color(0.2f, 0.1f, 0f);

        RectTransform rtHam = btnHam.GetComponent<RectTransform>();
        rtHam.anchorMin = rtHam.anchorMax = rtHam.pivot = new Vector2(1f, 1f);
        rtHam.anchoredPosition = new Vector2(-25f, -25f);
        rtHam.sizeDelta = new Vector2(90f, 90f); // giảm ~30% từ 130

        btnHam.GetComponent<Image>().sprite = null;

        // ── Dropdown ─────────────────────────────────────────────────────
        GameObject dropdown = MakePanel("DropdownPanel", root.transform, new Color(0.12f, 0.1f, 0.08f, 0.97f));
        RectTransform rtDrop = dropdown.GetComponent<RectTransform>();
        rtDrop.anchorMin = rtDrop.anchorMax = rtDrop.pivot = new Vector2(1f, 1f);

        rtDrop.anchoredPosition = new Vector2(-25f, -140f);
        rtDrop.sizeDelta = new Vector2(260f, 100f);

        VerticalLayoutGroup vlg = dropdown.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.spacing = 5f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = true;

        // ── Quit button (UPDATED +30%) ───────────────────────────────────
        GameObject btnQuit = MakeButton("BtnQuit", dropdown.transform, "✕   Thoát", 28, font);
        btnQuit.GetComponent<Image>().color = new Color(0.6f, 0.1f, 0.1f);
        btnQuit.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleLeft;
        PadText(btnQuit, 20);

        dropdown.SetActive(false);

        // ── Bind ─────────────────────────────────────────────────────────
        var so = new SerializedObject(ctrl);
        so.FindProperty("dropdownPanel").objectReferenceValue = dropdown;
        so.ApplyModifiedPropertiesWithoutUndo();

        BindClick(btnHam, ctrl, "OnHamburgerClick");
        BindClick(btnQuit, ctrl, "OnQuitClick");

        Undo.RegisterCreatedObjectUndo(root, "Setup Hamburger Menu");

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("✅ Hamburger Menu updated!");
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    static void SetStretch(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static GameObject MakePanel(string name, Transform parent, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        return go;
    }

    static GameObject MakeButton(string name, Transform parent, string label, int fontSize, Font font)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rtBtn = go.AddComponent<RectTransform>();
        rtBtn.sizeDelta = new Vector2(100, 40);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        go.AddComponent<Button>();

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);

        var txt = textGo.AddComponent<Text>();
        txt.text = label;
        txt.font = font;
        txt.material = font.material;
        txt.fontSize = fontSize;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;

        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        var rt = textGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        return go;
    }

    static void PadText(GameObject btnGo, int leftPad)
    {
        var rt = btnGo.GetComponentInChildren<Text>().GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(leftPad, rt.offsetMin.y);
    }

    static void BindClick(GameObject btnObj, HamburgerMenuController ctrl, string methodName)
    {
        var btn = btnObj.GetComponent<Button>();
        if (btn == null) return;

        btn.onClick.RemoveAllListeners();
        var action = System.Delegate.CreateDelegate(typeof(UnityAction), ctrl, methodName) as UnityAction;
        UnityEventTools.AddPersistentListener(btn.onClick, action);
        EditorUtility.SetDirty(btn);
    }
}