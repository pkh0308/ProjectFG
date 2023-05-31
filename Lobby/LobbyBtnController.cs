using UnityEngine;

public class LobbyBtnController : MonoBehaviour
{
    public void Btn_SinglePlay()
    {
        GameManager.Instance.GameStart(GameManager.GameMode.SingleGame);
    }

    public void Btn_MultiPlay()
    {
        GameManager.Instance.GameStart(GameManager.GameMode.MultiGame);
    }
}