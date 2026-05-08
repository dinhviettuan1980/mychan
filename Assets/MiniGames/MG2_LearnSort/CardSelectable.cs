using UnityEngine;

public class CardSelectable : MonoBehaviour
{
    private bool isSelected = false;
    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    void OnMouseDown()
    {
        isSelected = !isSelected;
        transform.localPosition = isSelected ? originalPos + Vector3.up * 0.5f : originalPos;
    }

    public bool IsSelected() => isSelected;

    public void ResetSelection()
    {
        isSelected = false;
        transform.localPosition = originalPos;
    }
}
