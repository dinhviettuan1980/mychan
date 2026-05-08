using UnityEngine;

public class ResultPopup1 : MonoBehaviour
{
    public GameObject root;
    public TMPro.TMP_Text messageText;
    public UnityEngine.UI.Button closeButton;

    private System.Action onClose;

    public void Show(string msg, System.Action onCloseCallback = null)
    {
        root.SetActive(true);
        messageText.text = msg;
        onClose = onCloseCallback;
    }

    public void Close()
    {
        root.SetActive(false);
        onClose?.Invoke();
        onClose = null;
    }
}
