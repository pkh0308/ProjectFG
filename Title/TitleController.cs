using UnityEngine;
using TMPro;

public class TitleController : MonoBehaviour
{
    // �α��� â
    [Header("�α���")]
    [SerializeField] GameObject logInSet;
    [SerializeField] TMP_InputField logIn_IDField;
    [SerializeField] TMP_InputField logIn_pwdField;

    // ���� ���� â
    [Header("���� ����")]
    [SerializeField] GameObject createAccountSet;
    [SerializeField] TMP_InputField create_IDField;
    [SerializeField] TMP_InputField create_pwdField;

    // �˸� â
    [Header("�˸� â")]
    [SerializeField] GameObject notificationSet;
    [SerializeField] TextMeshProUGUI notificationText;

    void Start()
    {
        // ��й�ȣ Ÿ������ ����
        logIn_pwdField.contentType = TMP_InputField.ContentType.Password;
        create_pwdField.contentType = TMP_InputField.ContentType.Password;
    }

    #region �α���
    // �α��� â ����/�ݱ�
    public void Btn_logInSet(bool value)
    {
        logInSet.SetActive(value);

        // �ؽ�Ʈ �ʱ�ȭ
        if (value)
        {
            logIn_IDField.text = "";
            logIn_pwdField.text = "";
            logIn_IDField.ActivateInputField();
        }
    }
    // �α��� �õ�
    public void Btn_LogIn()
    {
        // �α��� ����
        if(DBManager.Instance.LogIn(logIn_IDField.text, logIn_pwdField.text, out string res))
        {
            SceneController.Instance.EnterLobby();
        }
        // �α��� ����
        else
        {
            notificationText.text = res;
            notificationSet.SetActive(true);
        }
    }
    #endregion

    #region ���� ����
    // ���� ���� â ����/�ݱ�
    public void Btn_CreateSet(bool value)
    {
        createAccountSet.SetActive(value);

        // �ؽ�Ʈ �ʱ�ȭ
        if (value)
        {
            create_IDField.text = "";
            create_pwdField.text = "";
            create_IDField.ActivateInputField();
        }
    }
    // ���� ���� �õ�
    public void Btn_CreateAccount()
    {
        // ���̵� 2���� �̸��� ���
        if(create_IDField.text.Length < 2)
        {
            notificationText.text = "���̵�� 2���� �̻��̾�� �մϴ�.";
            notificationSet.SetActive(true);
            return;
        }
        // ��й�ȣ�� 4���� �̸��� ���
        if (create_pwdField.text.Length < 2)
        {
            notificationText.text = "��й�ȣ�� 4���� �̻��̾�� �մϴ�.";
            notificationSet.SetActive(true);
            return;
        }

        // ���� ���� ����
        if(DBManager.Instance.CreateAccount(create_IDField.text, create_pwdField.text))
        {
            notificationText.text = "���� ������ �����Ͽ����ϴ�!";
            createAccountSet.SetActive(false);
        }
        // ���� ���� ����
        else
            notificationText.text = "�̹� �����ϴ� ID�Դϴ�.";
        
        notificationSet.SetActive(true);
    }
    #endregion

    #region ���� �Է�
    // Tab �Է�
    void OnTab()
    {
        // �α��� â ���� + ID �Է�ĭ ���� ����
        if(logInSet.activeSelf && logIn_IDField.isFocused)
            logIn_pwdField.ActivateInputField();
        // ���� ���� â ���� + ID �Է�ĭ ���� ����
        else if(createAccountSet.activeSelf && create_IDField.isFocused)
            create_pwdField.ActivateInputField();
    }

    // Enter �Է�
    void OnEnter()
    {
        // �˸� â
        if (notificationSet.activeSelf)
            Btn_NotificationOff();
        // �α��� â
        else if (logInSet.activeSelf)
            Btn_LogIn();
        // ���� ���� â
        else if (createAccountSet.activeSelf)
            Btn_CreateAccount();
    }
    #endregion

    #region ��Ÿ
    // �˸� â �ݱ�
    public void Btn_NotificationOff()
    {
        notificationSet.SetActive(false);
    }
    #endregion
}