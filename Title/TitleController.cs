using UnityEngine;
using TMPro;

public class TitleController : MonoBehaviour
{
    // 로그인 창
    [Header("로그인")]
    [SerializeField] GameObject logInSet;
    [SerializeField] TMP_InputField logIn_IDField;
    [SerializeField] TMP_InputField logIn_pwdField;

    // 계정 생성 창
    [Header("계정 생성")]
    [SerializeField] GameObject createAccountSet;
    [SerializeField] TMP_InputField create_IDField;
    [SerializeField] TMP_InputField create_pwdField;

    // 알림 창
    [Header("알림 창")]
    [SerializeField] GameObject notificationSet;
    [SerializeField] TextMeshProUGUI notificationText;

    #region 로그인
    // 로그인 창 열기/닫기
    public void Btn_logInSet(bool value)
    {
        // 활성화 전에 텍스트 초기화
        if(value)
        {
            logIn_IDField.text = "";
            logIn_pwdField.text = "";
        }

        logInSet.SetActive(value);
    }
    // 로그인 시도
    public void Btn_LogIn()
    {
        // 로그인 성공
        if(DBManager.Instance.LogIn(logIn_IDField.text, logIn_pwdField.text, out string res))
        {
            SceneController.Instance.EnterLobby();
        }
        // 로그인 실패
        else
        {
            notificationText.text = res;
            notificationSet.SetActive(true);
        }
    }
    #endregion

    #region 계정 생성
    // 계정 생성 창 열기/닫기
    public void Btn_CreateSet(bool value)
    {
        // 활성화 전에 텍스트 초기화
        if (value)
        {
            create_IDField.text = "";
            create_pwdField.text = "";
        }

        createAccountSet.SetActive(value);
    }
    // 계정 생성 시도
    public void Btn_CreateAccount()
    {
        // 아이디가 2글자 미만일 경우
        if(create_IDField.text.Length < 2)
        {
            notificationText.text = "아이디는 2글자 이상이어야 합니다.";
            notificationSet.SetActive(true);
            return;
        }
        // 비밀번호가 4글자 미만일 경우
        if (create_pwdField.text.Length < 2)
        {
            notificationText.text = "비밀번호는 4글자 이상이어야 합니다.";
            notificationSet.SetActive(true);
            return;
        }

        // 계정 생성 성공
        if(DBManager.Instance.CreateAccount(create_IDField.text, create_pwdField.text))
            notificationText.text = "계정 생성에 성공하였습니다!";
        // 계정 생성 실패
        else
            notificationText.text = "이미 존재하는 ID입니다.";
        
        notificationSet.SetActive(true);
    }
    #endregion

    // 알림 창 닫기
    public void Btn_NotificationOff()
    {
        notificationSet.SetActive(false);
    }
}