using UnityEngine;

public class ResultSoundController : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] AudioClip winnerBgm;
    [SerializeField] AudioClip loserBgm;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void StartBgm(bool isWinner)
    {
        audioSource.loop = true;
        audioSource.clip = isWinner ? winnerBgm : loserBgm;
        audioSource.Play();
    }
}