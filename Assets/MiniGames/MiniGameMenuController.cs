using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameMenuController : MonoBehaviour
{

    public void OpenLearnCards()
    {
        SceneManager.LoadScene("LearnCards");
    }

    public void OpenMiniGame2()
    {
        SceneManager.LoadScene("Help");
    }

    public void OpenMiniGame3()
    {
        SceneManager.LoadScene("xep_bai");
    }

    public void OpenMiniGame4()
    {
        SceneManager.LoadScene("MiniGame4");
    }

    public void OpenLobby()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void OpenVideoSamples()
    {
        SceneManager.LoadScene("VideoSamples");
    }
}
