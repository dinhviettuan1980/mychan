using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class QuizQuestion
{
    public string kieuU;       // "Tôm", "Lèo", ...
    public string explanation; // giải thích luật — hiện khi sai
    // Hàm kiểm tra bài thực tế, trả về true nếu thoả điều kiện ù
    public System.Func<List<CardData>, bool> Check;

    public string Question => $"Bạn có Ù {kieuU} không?";
}

// ── Thêm câu hỏi mới vào danh sách Questions bên dưới ─────────────────────
public static class QuizBank
{
    static bool HasChan(List<CardData> hand, int rank, string type) =>
        hand.Count(c => c.rank == rank && c.type == type) >= 2;

    static bool IsRed(CardData c) => c.type == "van" || c.type == "vanj";

    public static readonly List<QuizQuestion> Questions = new List<QuizQuestion>
    {
        // ── Tôm ──────────────────────────────────────────────────────────────
        new QuizQuestion {
            kieuU       = "Tôm",
            explanation = "Tôm: Trên bài có đôi Thất Văn (7 Văn), đôi Tam Sách (3 Sách) và đôi Tam Vạn (3 Vạn).",
            Check = hand =>
                HasChan(hand, 7, "van") &&
                HasChan(hand, 3, "sach") &&
                HasChan(hand, 3, "vanj")
        },

        // ── Lèo ──────────────────────────────────────────────────────────────
        new QuizQuestion {
            kieuU       = "Lèo",
            explanation = "Lèo: Trên bài có đôi Chi Chi, đôi Cửu Vạn (9 Vạn) và đôi Bát Sách (8 Sách).",
            Check = hand =>
                HasChan(hand, 10, "chi") &&
                HasChan(hand, 9,  "vanj") &&
                HasChan(hand, 8,  "sach")
        },

        // ── Bạch định ─────────────────────────────────────────────────────────
        new QuizQuestion {
            kieuU       = "Bạch Định",
            explanation = "Bạch Định: Tay bài không có cây đỏ nào — toàn cây đen (Sách và Chi Chi).",
            Check = hand => hand.All(c => !IsRed(c))
        },

        // ── Tám đỏ ───────────────────────────────────────────────────────────
        new QuizQuestion {
            kieuU       = "Tám Đỏ",
            explanation = "Tám Đỏ: Tay bài có đúng 8 cây màu đỏ (Văn và Vạn).",
            Check = hand => hand.Count(IsRed) == 8
        },

        // ── Kính tứ Chi ──────────────────────────────────────────────────────
        new QuizQuestion {
            kieuU       = "Kính Tứ Chi",
            explanation = "Kính Tứ Chi: Tay bài có cả 4 con Chi Chi và toàn bộ các cây còn lại đều là cây đen.",
            Check = hand =>
                hand.Count(c => c.type == "chi") == 4 &&
                hand.Where(c => c.type != "chi").All(c => !IsRed(c))
        },

        // ── Thiên khai ───────────────────────────────────────────────────────
        new QuizQuestion {
            kieuU       = "Thiên Khai",
            explanation = "Thiên Khai: Ngay khi chia bài đã có sẵn 4 cây giống hệt nhau (cùng rank và cùng loại).",
            Check = hand =>
                hand.GroupBy(c => c.rank + "|" + c.type).Any(g => g.Count() >= 4)
        },
    };

    public static QuizQuestion GetRandom() =>
        Questions[Random.Range(0, Questions.Count)];
}
