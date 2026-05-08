using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TableDisplayManager : MonoBehaviour
{
    public GameObject tableButtonPrefab; // Prefab Button cho 1 bàn chơi
    public Transform gridParent; // BottomRightPanel
    public int maxTables = 12; // Số lượng bàn tối đa
    public int columns = 4; // Cột của Grid
    public int rows = 3; // Hàng của Grid

    private List<GameObject> currentTables = new List<GameObject>();

    void Start()
    {
        ShowTables(1000); // Mặc định mức cược 1000
    }

    public void ShowTables(int bet)
    {
        ClearTables();

        int tableCount = Random.Range(1, maxTables + 1);
        for (int i = 0; i < tableCount; i++)
        {
            GameObject table = Instantiate(tableButtonPrefab, gridParent);
            int players = Random.Range(0, 5); // 0–4
            table.GetComponent<TableButton>().Setup(i + 1, players, bet);
            table.GetComponent<Button>().onClick.AddListener(() => table.GetComponent<TableButton>().OnClickTable());
            currentTables.Add(table);
        }
    }

    public void ClearTables()
    {
        foreach (var table in currentTables)
        {
            Destroy(table);
        }
        currentTables.Clear();
    }
}
