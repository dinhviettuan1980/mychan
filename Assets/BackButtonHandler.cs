using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonHandler : MonoBehaviour
{
    public string backSceneName = "MainMenu"; // Hoặc scene bạn muốn quay về

    public void OnBackPressed()
    {
        Debug.Log("Navigate to " + backSceneName);
        SceneManager.LoadScene(backSceneName);
    }
}
