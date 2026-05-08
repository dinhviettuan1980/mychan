using UnityEngine;

public class CardClickHandler : MonoBehaviour
{
    private bool isSelected = false;
    private Vector3 originalPosition;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        if (isSelected)
        {
            transform.position = originalPosition;
            spriteRenderer.color = Color.white; // 👉 Reset về màu gốc
            isSelected = false;
        }
        else
        {
            transform.position = originalPosition + new Vector3(0, 0.2f, 0); // Nhô lên 20px
            spriteRenderer.color = Color.yellow; // 👉 Highlight bằng màu vàng
            isSelected = true;
        }
    }
}
