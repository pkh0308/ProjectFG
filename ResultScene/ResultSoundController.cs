using UnityEngine;

public class ResultSoundController : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] AudioClip resultBgm;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        audioSource.loop = true;
        audioSource.clip = resultBgm;
        audioSource.Play();
    }
}