using TMPro;
using UnityEngine;

public class Btn_Ready : MonoBehaviour
{
    TextMeshProUGUI readyText;
    string notReady;
    string onReady;
    bool isReady;

    void Awake()
    {
        readyText = GetComponentInChildren<TextMeshProUGUI>();

        notReady = "�غ�";
        onReady = "�غ�Ϸ�";
        isReady = false;
    }

    void OnEnable()
    {
        readyText.text = notReady;
        isReady = false;
    }

    public void ChangeReady()
    {
        isReady = !isReady;
        readyText.text = isReady ? onReady : notReady;
        NetworkManager.Instance.Ready(isReady);
    }
}