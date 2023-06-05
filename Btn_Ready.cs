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

        notReady = "준비";
        onReady = "준비완료";
        isReady = false;
    }

    void OnEnable()
    {
        readyText.text = notReady;
        isReady = false;
    }

    public void Btn_ChangeReadyText()
    {
        isReady = !isReady;
        readyText.text = isReady ? onReady : notReady;
    }
}
