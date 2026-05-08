using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardInteract : MonoBehaviour, IPointerClickHandler
{
    public int cardIndex;
    public bool isSelected = false;

    private RectTransform rt;
    private Vector2 originalPos;
    private CardManager _manager;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void Init(int index, CardManager manager = null)
    {
        cardIndex = index;
        _manager  = manager;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleSelect();
        _manager?.OnCardClick(cardIndex, isSelected);
    }

    public void ToggleSelect()
    {
        isSelected = !isSelected;

        if (isSelected)
        {
            rt.anchoredPosition = originalPos + new Vector2(0, 40);
            GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
        }
        else
        {
            rt.anchoredPosition = originalPos;
            GetComponent<Image>().color = Color.white;
        }
    }

    public void SaveOriginalPos(Vector2 pos)
    {
        originalPos = pos;
    }

    public void ResetToOriginal()
    {
        isSelected = false;
        rt.anchoredPosition = originalPos;
        GetComponent<Image>().color = Color.white;
    }

    public Vector2 GetOriginalPos() => originalPos;
}
