using TMPro;
using UnityEngine;

public class ResultSecneController : MonoBehaviour
{
    bool isWinner;

    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] GameObject[] playerModels;
    [SerializeField] ResultSoundController soundController;
    GameObject model;

    void Awake()
    {
        isWinner = GameManager.Instance.IsWinner;
    }

    void Start()
    {
        resultText.text = isWinner ? "Winner!" : "Loose...";

        // ��Ƽ �÷��̶�� ���� �ε���, �̱� �÷��̶�� 0
        int idx = NetworkManager.Instance.InRoom ? NetworkManager.Instance.GetMyIdx() : 0;
        model = Instantiate(playerModels[idx], Vector3.zero, Quaternion.Euler(0, 180, 0));
        model.GetComponent<ResultModel>().SetAnimation(isWinner);

        // Bgm �÷���
        soundController.StartBgm(isWinner);
    }

    void OnDestroy()
    {
        Destroy(model);
    }
}