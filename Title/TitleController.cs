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

    #region �α���
    // �α��� â ����/�ݱ�
    public void Btn_logInSet(bool value)
    {
        // Ȱ��ȭ ���� �ؽ�Ʈ �ʱ�ȭ
        if(value)
        {
            logIn_IDField.text = "";
            logIn_pwdField.text = "";
        }

        logInSet.SetActive(value);
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
        // Ȱ��ȭ ���� �ؽ�Ʈ �ʱ�ȭ
        if (value)
        {
            create_IDField.text = "";
            create_pwdField.text = "";
        }

        createAccountSet.SetActive(value);
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
            notificationText.text = "���� ������ �����Ͽ����ϴ�!";
        // ���� ���� ����
        else
            notificationText.text = "�̹� �����ϴ� ID�Դϴ�.";
        
        notificationSet.SetActive(true);
    }
    #endregion

    // �˸� â �ݱ�
    public void Btn_NotificationOff()
    {
        notificationSet.SetActive(false);
    }
}