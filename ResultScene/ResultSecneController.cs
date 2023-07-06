using TMPro;
using UnityEngine;

public class ResultSecneController : MonoBehaviour
{
    bool isWinner;

    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] GameObject playerModel;
    GameObject model;

    void Awake()
    {
        isWinner = GameManager.Instance.IsWinner;
    }

    void Start()
    {
        resultText.text = isWinner ? "Winner!" : "Loose...";

        model = Instantiate(playerModel, Vector3.zero, Quaternion.Euler(0, 180, 0));
        model.GetComponent<ResultModel>().SetAnimation(isWinner);
    }

    void OnDestroy()
    {
        Destroy(model);
    }
}