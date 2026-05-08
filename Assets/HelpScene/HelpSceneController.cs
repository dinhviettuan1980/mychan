using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class HelpSceneController : MonoBehaviour
{
    [Header("UI References")]
    public Image cardHolder;
    public Text cardNameText;
    public Button nextButton;
    public Toggle autoPlayToggle;

    [Header("Card Data")]
    public Sprite[] cardSprites;
    public string[] cardNames;
    private string[] audioNames = {
        "nhisach", "tamsach", "tusach", "ngusach", "lucsach", "thatsach", "batsach", "cuusach",
        "nhivanj", "tamvanj", "tuvanj", "nguvanj", "lucvanj", "thatvanj", "batvanj", "cuuvanj",
        "nhivan", "tamvan", "tuvan", "nguvan", "lucvan", "thatvan", "batvan", "cuuvan", "chichi"
    };

    [Header("Audio")]
    private AudioSource audioSource;

    private int currentIndex = 0;
    private bool isAutoPlaying = false;
    private float autoPlayDelay = 3f;

    private List<int> shuffledIndices = new List<int>();

    // --- Hiệu ứng ---
    private CanvasGroup canvasGroup;
    private RectTransform cardRect;
    private float fadeDuration = 0.4f;
    private float slideDistance = 80f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        canvasGroup = cardHolder.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = cardHolder.gameObject.AddComponent<CanvasGroup>();

        cardRect = cardHolder.GetComponent<RectTransform>();

        ShuffleCards();
        ShowCardInstant(currentIndex);

        nextButton.onClick.AddListener(NextCard);
        autoPlayToggle.onValueChanged.AddListener(OnAutoPlayChanged);
    }

    void ShuffleCards()
    {
        shuffledIndices.Clear();
        for (int i = 0; i < cardSprites.Length; i++)
            shuffledIndices.Add(i);

        for (int i = 0; i < shuffledIndices.Count; i++)
        {
            int rand = Random.Range(i, shuffledIndices.Count);
            (shuffledIndices[i], shuffledIndices[rand]) = (shuffledIndices[rand], shuffledIndices[i]);
        }

        currentIndex = 0;
    }

    void ShowCardInstant(int index)
    {
        int cardIndex = shuffledIndices[index];
        cardHolder.sprite = cardSprites[cardIndex];
        cardNameText.text = cardNames[cardIndex];
        PlayCardAudio(audioNames[cardIndex]);
    }

    IEnumerator ShowCardAnimated(int index)
    {
        int cardIndex = shuffledIndices[index];

        // Giai đoạn 1: fade out & slide left
        float elapsed = 0f;
        Vector2 startPos = cardRect.anchoredPosition;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            cardRect.anchoredPosition = startPos + Vector2.left * slideDistance * t;
            yield return null;
        }

        // Đổi ảnh và text
        cardHolder.sprite = cardSprites[cardIndex];
        cardNameText.text = cardNames[cardIndex];

        // Reset vị trí về phải
        cardRect.anchoredPosition = startPos + Vector2.right * slideDistance;
        PlayCardAudio(audioNames[cardIndex]);

        // Giai đoạn 2: fade in & slide về giữa
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            cardRect.anchoredPosition = Vector2.Lerp(
                startPos + Vector2.right * slideDistance,
                startPos,
                t
            );
            yield return null;
        }

        canvasGroup.alpha = 1f;
        cardRect.anchoredPosition = startPos;
    }

    void PlayCardAudio(string cardName)
    {
        string normalized = cardName.ToLower().Trim();
        AudioClip clip = Resources.Load<AudioClip>($"Audio/{normalized}");
        if (clip != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy file âm thanh cho {normalized}");
        }
    }

    void NextCard()
    {
        if (isAutoPlaying) return;

        currentIndex++;
        if (currentIndex >= shuffledIndices.Count)
            ShuffleCards();

        StopAllCoroutines();
        StartCoroutine(ShowCardAnimated(currentIndex));
    }

    void OnAutoPlayChanged(bool isOn)
    {
        isAutoPlaying = isOn;
        nextButton.interactable = !isOn;

        if (isAutoPlaying)
            StartCoroutine(AutoPlayCards());
        else
            StopAllCoroutines();
    }

    IEnumerator AutoPlayCards()
    {
        while (isAutoPlaying)
        {
            currentIndex++;
            if (currentIndex >= shuffledIndices.Count)
                ShuffleCards();

            yield return StartCoroutine(ShowCardAnimated(currentIndex));
            yield return new WaitForSeconds(autoPlayDelay);
        }
    }
}
