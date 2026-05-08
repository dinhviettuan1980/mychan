using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class QuizPopup : MonoBehaviour
{
    [Header("Root")]
    public GameObject root;

    [Header("Question panel")]
    public TMP_Text   questionText;
    public GameObject answerPanel;
    public Button     dungButton;
    public Button     saiButton;

    [Header("Feedback panel")]
    public GameObject feedbackPanel;
    public TMP_Text   feedbackText;
    public Button     closeButton;

    private QuizQuestion    _current;
    private List<CardData>  _hand;
    private Action          _onClose;
    private bool            _listenersAdded;

    // ── Public entry point ────────────────────────────────────────────────────

    public void Show(QuizQuestion q, List<CardData> hand, Action onClose)
    {
        Resolve(); // ensure all refs are set

        _current = q;
        _hand    = hand;
        _onClose = onClose;

        questionText.text = q.Question;
        answerPanel  .SetActive(true);
        feedbackPanel.SetActive(false);
        root.SetActive(true);
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    void Resolve()
    {
        if (root          == null) root          = gameObject;
        if (questionText  == null) questionText  = Deep<TMP_Text>("QuestionText");
        if (answerPanel   == null) answerPanel   = DeepGO("AnswerPanel");
        if (feedbackPanel == null) feedbackPanel = DeepGO("FeedbackPanel");
        if (dungButton    == null) dungButton    = Deep<Button>("DungButton");
        if (saiButton     == null) saiButton     = Deep<Button>("SaiButton");
        if (feedbackText  == null) feedbackText  = Deep<TMP_Text>("FeedbackText");
        if (closeButton   == null) closeButton   = Deep<Button>("CloseButton");

        if (!_listenersAdded)
        {
            if (dungButton  != null) dungButton .onClick.AddListener(() => OnAnswer(true));
            if (saiButton   != null) saiButton  .onClick.AddListener(() => OnAnswer(false));
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            _listenersAdded = true;
        }

        // Log bất kỳ field nào vẫn còn null để dễ debug
        if (questionText  == null) Debug.LogError("[QuizPopup] QuestionText not found");
        if (answerPanel   == null) Debug.LogError("[QuizPopup] AnswerPanel not found");
        if (feedbackPanel == null) Debug.LogError("[QuizPopup] FeedbackPanel not found");
        if (dungButton    == null) Debug.LogError("[QuizPopup] DungButton not found");
        if (saiButton     == null) Debug.LogError("[QuizPopup] SaiButton not found");
        if (feedbackText  == null) Debug.LogError("[QuizPopup] FeedbackText not found");
        if (closeButton   == null) Debug.LogError("[QuizPopup] CloseButton not found");
    }

    void OnAnswer(bool playerSaysYes)
    {
        bool actualResult = _current.Check(_hand);
        bool correct      = playerSaysYes == actualResult;

        answerPanel .SetActive(false);
        feedbackPanel.SetActive(true);
        feedbackText.text = correct
            ? "✅ Bạn đã đúng!"
            : $"❌ Bạn đã sai.\n\n{_current.explanation}";
    }

    void Close()
    {
        root.SetActive(false);
        var cb = _onClose;
        _onClose = null;
        cb?.Invoke();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    GameObject DeepGO(string childName)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            if (t.name == childName) return t.gameObject;
        return null;
    }

    T Deep<T>(string childName) where T : Component
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            if (t.name == childName) { var c = t.GetComponent<T>(); if (c) return c; }
        return null;
    }
}
