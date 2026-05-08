using UnityEngine;
using UnityEngine.SceneManagement;

public class HelpSceneManager : MonoBehaviour
{
    // 🔙 Gọi khi click vào nút "Back"
    public void OnBackButtonClick()
    {
        // Quay về scene DashBoard
        Debug.Log("Quay về scene MiniGameMenu ");
        SceneManager.LoadScene("xep_bai");
    }

    // 🀄 Gọi khi click vào nút "XepBaiButton"
    public void OnXepBaiButtonClick()
    {
        // Sang scene xep_bai (hướng dẫn xếp bài)
        SceneManager.LoadScene("xep_bai");
    }
}
