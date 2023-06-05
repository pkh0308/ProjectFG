using UnityEngine;

public class TitleBtnController : MonoBehaviour
{
    public void Btn_EnterLobby()
    {
        SceneController.Instance.EnterLobby();
    }
}