using UnityEngine;

[System.Serializable]
public class CardData
{
    public string id;       // unique id, ví dụ: van_3_a, van_3_b, van_3_c, van_3_d
    public string name;     // ví dụ: "Tam Vạn"
    public string type;     // "van", "vanh", "sach", "yeu"
    public int rank;        // 1..9 (hoặc 0 cho quân yêu)
    public Sprite image;    // ảnh của lá bài
}
