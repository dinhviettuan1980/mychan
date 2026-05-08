using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CardManager : MonoBehaviour
{
    public CardCollection fullDeck;
    public GameObject cardPrefab;
    public Transform handArea;
    public ResultPopup1 resultPopup1;
    public GameObject sortExplanationPopup;

    private List<CardData> shuffledDeck  = new List<CardData>();
    private List<CardData> playerHand    = new List<CardData>();
    private List<CardData> remainingDeck = new List<CardData>();
    private Dictionary<int, bool> selectedMap = new Dictionary<int, bool>();
    // Tracks which cards the player tapped, in tap order, for GroupSelectedCards.
    private List<CardInteract> _selectionOrderCI = new List<CardInteract>();

    private enum GType { Chan, Ca, BaDau, Rac }

    // ── Lifecycle ──────────────────────────────────────────────────────────

    void Start()
    {
        shuffledDeck = new List<CardData>(fullDeck.cards);
        if (sortExplanationPopup != null) sortExplanationPopup.SetActive(false);
    }

    // ── Deal ───────────────────────────────────────────────────────────────

    public void DealCards()
    {
        ClearCardOverlays();
        _selectionOrderCI.Clear();
        Shuffle(shuffledDeck);
        playerHand    = shuffledDeck.GetRange(0, 19);
        remainingDeck = shuffledDeck.GetRange(19, shuffledDeck.Count - 19);
        Debug.Log($"Tay bài: {playerHand.Count} quân, Còn lại: {remainingDeck.Count} quân");
        DisplayPlayerHand();
        UpdateCardIndexes();
    }

    void DisplayPlayerHand()
    {
        foreach (Transform child in handArea) Destroy(child.gameObject);

        int count = playerHand.Count;
        if (count == 0) return;

        RectTransform handRect = handArea as RectTransform;
        if (handRect == null) { Debug.LogError("HandArea phải là RectTransform!"); return; }

        float h        = handRect.rect.height;
        float fanAngle = Mathf.Min(180f, 50f + count * 8f);
        float radius   = (h * 0.25f) / Mathf.Sin(Mathf.Deg2Rad * (fanAngle / 2f));
        float scale    = count > 22 ? 1.0f : count > 18 ? 1.1f : 1.2f;
        float start    = -fanAngle / 2f;
        float step     = fanAngle / (count - 1);

        for (int i = 0; i < count; i++)
        {
            GameObject card = Instantiate(cardPrefab, handArea);
            card.transform.localScale = Vector3.one * scale;

            var rt = card.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);

            card.GetComponent<CardView>()?.SetCard(playerHand[i], playerHand[i].name, true);

            var interact = card.GetComponent<CardInteract>();
            interact?.Init(i, this);

            float angle = start + i * step;
            float rad   = Mathf.Deg2Rad * angle;
            Vector2 pos = new Vector2(
                Mathf.Sin(rad) * radius,
                Mathf.Cos(rad) * radius - radius + h * 0.25f);

            rt.anchoredPosition = pos;
            rt.localRotation    = Quaternion.Euler(0, 0, -angle);
            rt.SetSiblingIndex(i);

            interact?.SaveOriginalPos(pos);
        }
    }

    // ── Sort (XepButton) ───────────────────────────────────────────────────

    // Called by XepButton: sorts and shows the explanation popup.
    public void SortPlayerHand()
    {
        ClearCardOverlays();
        SortHandInternal();
        if (sortExplanationPopup != null) sortExplanationPopup.SetActive(true);
    }

    private void SortHandInternal()
    {
        if (playerHand == null || playerHand.Count == 0) return;
        _selectionOrderCI.Clear();

        var grouped  = playerHand.GroupBy(c => c.rank).OrderBy(g => g.Key).ToList();
        var chanList = new List<CardData>();
        var caList   = new List<CardData>();
        var bdList   = new List<CardData>();
        var racList  = new List<CardData>();

        foreach (var group in grouped)
        {
            List<CardData> all = group.ToList();

            // Chắn: pairs with same rank + same type
            var byType = all.GroupBy(c => c.type).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var kv in byType.ToList())
            {
                int pairs = kv.Value.Count / 2;
                if (pairs > 0)
                {
                    var taken = kv.Value.Take(pairs * 2).ToList();
                    chanList.AddRange(taken);
                    all.RemoveAll(c => taken.Contains(c));
                }
            }

            // Cạ: pairs with same rank + different type
            var rem = all.GroupBy(c => c.type).ToDictionary(g => g.Key, g => new Queue<CardData>(g.ToList()));
            while (true)
            {
                var types = rem.Where(kv => kv.Value.Count > 0).Select(kv => kv.Key).ToList();
                if (types.Count < 2) break;
                var a = rem[types[0]].Dequeue();
                var b = rem[types[1]].Dequeue();
                caList.Add(a); caList.Add(b);
                all.Remove(a); all.Remove(b);
            }

            // Ba đầu: 3 same rank, 3 different types
            if (all.Count == 3 && all.Select(c => c.type).Distinct().Count() == 3)
            { bdList.AddRange(all); all.Clear(); }

            racList.AddRange(all);
        }

        playerHand = chanList.Concat(caList).Concat(bdList).Concat(racList).ToList();
        Debug.Log($"✅ Xếp: Chan={chanList.Count/2} cặp | Ca={caList.Count/2} cặp | BaDau={bdList.Count/3} bộ | Rac={racList.Count} lá");
        DisplayPlayerHand();
        UpdateCardIndexes();
    }

    // ── Check result (DoneButton) ──────────────────────────────────────────

    // Reads the current UI card order and evaluates it using the sequential
    // phase algorithm: chắn pairs first → cạ pairs → ba đầu triples → rác.
    // Compares against the machine's canonical grouping and shows % correct.
    public void CheckResultWithPopup()
    {
        // Read current UI order
        var uiCards = new List<CardData>();
        foreach (Transform t in handArea)
        {
            if (t.GetComponent<CardInteract>() == null) continue; // skip GroupFrames
            var cv = t.GetComponent<CardView>();
            if (cv != null && cv.data != null) uiCards.Add(cv.data);
        }

        // Machine's canonical groups (classified by type)
        var machine     = GetCanonicalGroupsClassified(playerHand);
        var macChan     = machine.Where(g => g.type == GType.Chan) .Select(g => g.key).ToList();
        var macCa       = machine.Where(g => g.type == GType.Ca)   .Select(g => g.key).ToList();
        var macBaDau    = machine.Where(g => g.type == GType.BaDau).Select(g => g.key).ToList();
        int machineTotal = macChan.Count + macCa.Count + macBaDau.Count;

        // Player's groups (sequential: chan → ca → ba đầu → rác)
        var playerGroups = ParsePlayerGroupsSequential(uiCards);

        // Greedy match each player group against the machine's bucket of the same type
        var remChan  = new List<string>(macChan);
        var remCa    = new List<string>(macCa);
        var remBaDau = new List<string>(macBaDau);
        var results  = new List<(int startIdx, int count, bool isCorrect, GType type)>();

        foreach (var pg in playerGroups)
        {
            if (pg.type == GType.Rac)
            {
                results.Add((pg.startIdx, 1, false, GType.Rac));
                continue;
            }

            List<string> bucket = pg.type == GType.Chan  ? remChan
                                 : pg.type == GType.Ca    ? remCa
                                 : remBaDau;

            int idx = bucket.IndexOf(pg.key);
            if (idx >= 0)
            {
                bucket.RemoveAt(idx);
                results.Add((pg.startIdx, pg.count, true,  pg.type));
            }
            else
            {
                results.Add((pg.startIdx, pg.count, false, pg.type));
            }
        }

        int correct = results.Count(r => r.type != GType.Rac && r.isCorrect);
        float pct   = machineTotal > 0 ? (correct * 100f / machineTotal) : 0f;

        ClearCardOverlays();
        ApplyGroupOverlays(results);

        resultPopup1.Show($"% đúng: {correct}/{machineTotal} bộ ({pct:F0}%)");
    }

    // ── Group parsing algorithms ───────────────────────────────────────────

    // Sequential phase parser: scans left→right through uiCards.
    // Phase 0 collects consecutive chắn pairs; on the first non-chắn, advances
    // to phase 1 (cạ), then phase 2 (ba đầu), then phase 3 (rác).
    // A group that breaks the streak does NOT advance i — it is re-evaluated
    // in the next phase.
    private List<(string key, int startIdx, int count, GType type)> ParsePlayerGroupsSequential(
        List<CardData> cards)
    {
        string CardKey(CardData c) => $"{c.rank}-{c.type}";
        string GroupKey(List<CardData> g) =>
            string.Join("|", g.Select(CardKey).OrderBy(x => x));

        bool IsChan(int i) =>
            i + 1 < cards.Count &&
            cards[i].rank == cards[i + 1].rank &&
            cards[i].type == cards[i + 1].type;

        bool IsCa(int i) =>
            i + 1 < cards.Count &&
            cards[i].rank == cards[i + 1].rank &&
            cards[i].type != cards[i + 1].type;

        bool IsBaDau(int i) =>
            i + 2 < cards.Count &&
            cards[i].rank == cards[i + 1].rank &&
            cards[i].rank == cards[i + 2].rank &&
            cards[i].type != cards[i + 1].type &&
            cards[i].type != cards[i + 2].type &&
            cards[i + 1].type != cards[i + 2].type;

        var result = new List<(string, int, int, GType)>();
        int phase = 0; // 0=Chan 1=Ca 2=BaDau 3=Rac
        int idx   = 0;

        while (idx < cards.Count)
        {
            switch (phase)
            {
                case 0: // Chắn
                    if (IsChan(idx))
                    {
                        var g = new List<CardData> { cards[idx], cards[idx + 1] };
                        result.Add((GroupKey(g), idx, 2, GType.Chan));
                        idx += 2;
                    }
                    else { phase = 1; } // don't advance idx
                    break;

                case 1: // Cạ
                    if (IsCa(idx))
                    {
                        var g = new List<CardData> { cards[idx], cards[idx + 1] };
                        result.Add((GroupKey(g), idx, 2, GType.Ca));
                        idx += 2;
                    }
                    else { phase = 2; }
                    break;

                case 2: // Ba Đầu
                    if (IsBaDau(idx))
                    {
                        var g = new List<CardData> { cards[idx], cards[idx + 1], cards[idx + 2] };
                        result.Add((GroupKey(g), idx, 3, GType.BaDau));
                        idx += 3;
                    }
                    else { phase = 3; }
                    break;

                case 3: // Rác
                    result.Add((CardKey(cards[idx]), idx, 1, GType.Rac));
                    idx++;
                    break;
            }
        }
        return result;
    }

    // Returns the machine's canonical groups with their types.
    // Mirrors SortHandInternal without modifying playerHand.
    private List<(string key, GType type)> GetCanonicalGroupsClassified(List<CardData> hand)
    {
        string CardKey(CardData c) => $"{c.rank}-{c.type}";
        string GroupKey(List<CardData> g) =>
            string.Join("|", g.Select(CardKey).OrderBy(x => x));

        var grouped = hand.GroupBy(c => c.rank).OrderBy(g => g.Key).ToList();
        var result  = new List<(string, GType)>();

        foreach (var group in grouped)
        {
            List<CardData> all = group.ToList();

            // Chắn
            var byType = all.GroupBy(c => c.type).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var kv in byType.ToList())
            {
                int pairs = kv.Value.Count / 2;
                if (pairs > 0)
                {
                    var taken = kv.Value.Take(pairs * 2).ToList();
                    for (int i = 0; i < pairs; i++)
                        result.Add((GroupKey(new List<CardData> { taken[i * 2], taken[i * 2 + 1] }), GType.Chan));
                    all.RemoveAll(c => taken.Contains(c));
                }
            }

            // Cạ
            var rem = all.GroupBy(c => c.type).ToDictionary(g => g.Key, g => new Queue<CardData>(g.ToList()));
            while (true)
            {
                var types = rem.Where(kv => kv.Value.Count > 0).Select(kv => kv.Key).ToList();
                if (types.Count < 2) break;
                var a = rem[types[0]].Dequeue();
                var b = rem[types[1]].Dequeue();
                result.Add((GroupKey(new List<CardData> { a, b }), GType.Ca));
                all.Remove(a); all.Remove(b);
            }

            // Ba đầu
            if (all.Count == 3 && all.Select(c => c.type).Distinct().Count() == 3)
            { result.Add((GroupKey(all), GType.BaDau)); all.Clear(); }

            // Rác
            foreach (var c in all) result.Add((CardKey(c), GType.Rac));
        }

        return result;
    }

    // ── Overlays and border frames ─────────────────────────────────────────

    void ClearCardOverlays()
    {
        // Remove per-card fill overlays
        foreach (Transform child in handArea)
        {
            var ov = child.Find("GroupOverlay");
            if (ov != null) Destroy(ov.gameObject);
        }
        // Remove group border frame panels (direct children of handArea)
        var frames = new List<GameObject>();
        foreach (Transform child in handArea)
            if (child.name == "GroupArc") frames.Add(child.gameObject);
        foreach (var f in frames) Destroy(f);
    }

    void ApplyGroupOverlays(List<(int startIdx, int count, bool isCorrect, GType type)> groups)
    {
        // ClearCardOverlays was called just before, so handArea contains only cards.
        if (handArea.childCount == 0) return;

        // Recompute fan layout parameters (same formula as DisplayPlayerHand / LayoutFan).
        int cardCount = 0;
        foreach (Transform t in handArea)
            if (t.GetComponent<CardInteract>() != null) cardCount++;
        if (cardCount < 2) return;

        var sampleRT  = handArea.GetChild(0).GetComponent<RectTransform>();
        float cardH   = sampleRT.rect.height * sampleRT.localScale.y;

        var   handRect  = handArea as RectTransform;
        float h         = handRect.rect.height;
        float fanAngle  = Mathf.Min(180f, 50f + cardCount * 8f);
        float radius    = (h * 0.25f) / Mathf.Sin(Mathf.Deg2Rad * (fanAngle / 2f));
        float startAng  = -fanAngle / 2f;
        float angStep   = fanAngle / (cardCount - 1);

        // The fan's circle center in handArea-space (anchor bottom-center).
        float fanCY = -radius + h * 0.25f;

        // Arc sits slightly above the card face, near the name-text area.
        float arcRadius = radius + cardH * 0.88f + 50f;
        const float arcThickness = 9f;

        foreach (var (startIdx, count, isCorrect, gtype) in groups)
        {
            if (gtype == GType.Rac) continue;

            // ── Colored fill on each card ──────────────────────────────────
            Color fill = isCorrect
                ? new Color(0f, 0.85f, 0.2f, 0.35f)
                : new Color(1f, 0.15f, 0.15f, 0.35f);

            for (int i = startIdx; i < startIdx + count && i < handArea.childCount; i++)
            {
                var card = handArea.GetChild(i);
                var ov   = new GameObject("GroupOverlay");
                ov.transform.SetParent(card, false);
                var img  = ov.AddComponent<Image>();
                img.color = fill;
                img.raycastTarget = false;
                var rt   = ov.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            }

            // ── Concentric arc above the group ─────────────────────────────
            // Color encodes group type: chắn = darkest blue, cạ = mid, ba đầu = light.
            Color arcColor;
            if      (gtype == GType.Chan)  arcColor = new Color(0.00f, 0.15f, 0.85f, 1f);
            else if (gtype == GType.Ca)    arcColor = new Color(0.20f, 0.50f, 1.00f, 1f);
            else                           arcColor = new Color(0.55f, 0.78f, 1.00f, 1f);

            // Arc spans from half a step before the first card to half a step after the last.
            float aFirst = startAng + (startIdx           - 0.22f) * angStep;
            float aLast  = startAng + (startIdx + count-1 + 0.22f) * angStep;

            DrawArcSegments(fanCY, arcRadius, aFirst, aLast, arcColor, arcThickness);
        }
    }

    // Draws a smooth arc as a series of short rectangular segments, each rotated
    // tangentially to the circle.  All segments are parented to handArea and named
    // "GroupArc" so ClearCardOverlays can destroy them.
    void DrawArcSegments(float fanCY, float arcRadius,
                         float angleStart, float angleEnd,
                         Color color, float thickness)
    {
        float span    = angleEnd - angleStart;
        float arcLen  = Mathf.Abs(span * Mathf.Deg2Rad * arcRadius);
        int   segs    = Mathf.Max(3, Mathf.RoundToInt(arcLen / 5f));
        float segAng  = span / segs;
        // +2 px overlap so adjacent segments never show a gap.
        float segLen  = Mathf.Abs(segAng * Mathf.Deg2Rad * arcRadius) + 2f;

        for (int i = 0; i < segs; i++)
        {
            float a  = angleStart + (i + 0.5f) * segAng;
            float ar = Mathf.Deg2Rad * a;

            var seg = new GameObject("GroupArc", typeof(RectTransform));
            seg.transform.SetParent(handArea, false);

            var img = seg.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;

            var rt = seg.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(
                Mathf.Sin(ar) * arcRadius,
                fanCY + Mathf.Cos(ar) * arcRadius);
            rt.sizeDelta    = new Vector2(segLen, thickness);
            // Rotate tangentially: same convention as cards (-angle = radial outward).
            rt.localRotation = Quaternion.Euler(0f, 0f, -a);
        }
    }

    // ── Card interaction ───────────────────────────────────────────────────

    public void OnCardClick(int index, bool selected)
    {
        selectedMap[index] = selected;

        if (selected)
        {
            foreach (Transform child in handArea)
            {
                var ci = child.GetComponent<CardInteract>();
                if (ci != null && ci.cardIndex == index && !_selectionOrderCI.Contains(ci))
                { _selectionOrderCI.Add(ci); break; }
            }
        }
        else
        {
            _selectionOrderCI.RemoveAll(ci => ci == null || ci.cardIndex == index);
        }
    }

    void RearrangeAfterSelection()
    {
        // Only consider actual card children (have CardInteract); skip GroupFrames.
        var cards = new List<RectTransform>();
        foreach (Transform t in handArea)
            if (t.GetComponent<CardInteract>() != null)
                cards.Add(t.GetComponent<RectTransform>());

        if (cards.Count == 0) return;

        var sel   = cards.Where(c =>  c.GetComponent<CardInteract>().isSelected)
                         .OrderBy(c => c.GetComponent<CardInteract>().cardIndex).ToList();
        var nosel = cards.Where(c => !c.GetComponent<CardInteract>().isSelected)
                         .OrderBy(c => c.GetComponent<CardInteract>().cardIndex).ToList();

        var order = nosel.Concat(sel).ToList();
        for (int i = 0; i < order.Count; i++) order[i].SetSiblingIndex(i);

        LayoutFan();
    }

    void LayoutFan()
    {
        // Count only actual card children (skip GroupFrames).
        int count = 0;
        foreach (Transform t in handArea)
            if (t.GetComponent<CardInteract>() != null) count++;

        if (count == 0) return;

        var handRect = handArea as RectTransform;
        float h        = handRect.rect.height;
        float fanAngle = Mathf.Min(180f, 50f + count * 8f);
        float radius   = (h * 0.25f) / Mathf.Sin(Mathf.Deg2Rad * (fanAngle / 2f));
        float start    = -fanAngle / 2f;
        float step     = fanAngle / (count - 1);

        int i = 0;
        foreach (Transform t in handArea)
        {
            var ci = t.GetComponent<CardInteract>();
            if (ci == null) continue; // skip GroupFrames

            var rt    = t.GetComponent<RectTransform>();
            float ang = start + i * step;
            float rad = Mathf.Deg2Rad * ang;
            Vector2 pos = new Vector2(
                Mathf.Sin(rad) * radius,
                Mathf.Cos(rad) * radius - radius + h * 0.25f);

            rt.anchoredPosition = pos;
            rt.localRotation    = Quaternion.Euler(0, 0, -ang);
            ci.SaveOriginalPos(pos);

            if (ci.isSelected) rt.anchoredPosition = pos + new Vector2(0, 40);

            i++;
        }
    }

    public void GroupSelectedCards()
    {
        _selectionOrderCI.RemoveAll(ci => ci == null);
        if (_selectionOrderCI.Count == 0) return;

        // Collect all live card children.
        var allCI = new List<CardInteract>();
        foreach (Transform child in handArea)
        {
            var ci = child.GetComponent<CardInteract>();
            if (ci != null) allCI.Add(ci);
        }

        var selSet      = new HashSet<CardInteract>(_selectionOrderCI);
        int anchorIdx   = _selectionOrderCI[0].cardIndex; // first-tapped card's position

        // Build new order:
        //   1. Unselected cards whose cardIndex < anchor  (original left neighbours)
        //   2. Selected cards in tap order (anchor first, then the rest)
        //   3. Unselected cards whose cardIndex >= anchor (original right neighbours)
        var before = allCI
            .Where(ci => !selSet.Contains(ci) && ci.cardIndex < anchorIdx)
            .OrderBy(ci => ci.cardIndex)
            .Select(ci => ci.transform).ToList();

        var middle = _selectionOrderCI.Select(ci => ci.transform).ToList();

        var after = allCI
            .Where(ci => !selSet.Contains(ci) && ci.cardIndex >= anchorIdx)
            .OrderBy(ci => ci.cardIndex)
            .Select(ci => ci.transform).ToList();

        var newOrder = before.Concat(middle).Concat(after).ToList();
        for (int i = 0; i < newOrder.Count; i++) newOrder[i].SetSiblingIndex(i);

        // Deselect all grouped cards.
        foreach (var ci in _selectionOrderCI)
        {
            if (ci.isSelected) ci.ToggleSelect();
            selectedMap[ci.cardIndex] = false;
        }
        _selectionOrderCI.Clear();

        // Sync cardIndex with the new sibling order so subsequent groupings work correctly.
        UpdateCardIndexes();

        LayoutFan();
    }

    void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            CardData temp = list[i];
            int rand = Random.Range(i, list.Count);
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    void UpdateCardIndexes()
    {
        selectedMap.Clear();
        for (int i = 0; i < handArea.childCount; i++)
        {
            var ci = handArea.GetChild(i).GetComponent<CardInteract>();
            if (ci != null) { ci.cardIndex = i; selectedMap[i] = ci.isSelected; }
        }
    }
}
