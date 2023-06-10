using System;
using UnityEngine;

public class StageSoundController : MonoBehaviour
{
    AudioSource bgmAudioSource;
    AudioSource[] sfxAudioSources;
    AudioSource curSfxSource;

    //재생 함수 호출용 열거형
    public enum StageBgm 
    { 
        stopBgm = -1,
        stageBgm = 1000
    }
    // sfx 함수 호출용 열거형
    public enum StageSfx 
    {
        jump = 2000,
        savePoint,
        reset,
        stageClear,
        gameOver
    }

    //bgm
    [Header("Bgm")]
    [SerializeField] AudioClip stageBgm;
    public static Action<int> PlayBgm;

    //sfx
    [Header("Sfx")]
    [SerializeField] AudioClip jump;
    [SerializeField] AudioClip savePoint;
    [SerializeField] AudioClip reset;
    [SerializeField] AudioClip stageClear;
    [SerializeField] AudioClip gameOver;
    public static Action<int> PlaySfx;

    void Awake()
    {
        PlayBgm = (bgmIdx) => { Play_Bgm(bgmIdx); };
        PlaySfx = (sfxIdx) => { Play_Sfx(sfxIdx); };

        AudioSource[] sources = GetComponentsInChildren<AudioSource>();
        // 첫 AudioSoruce는 bgm 용으로, 나머지는 sfx용으로 저장
        bgmAudioSource = sources[0];
        sfxAudioSources = new AudioSource[sources.Length - 1];
        for (int i = 1; i < sources.Length; i++)
            sfxAudioSources[i - 1] = sources[i];
    }

    void Start()
    {
        // loop 설정은 bgm은 true, sfx는 false로 설정
        bgmAudioSource.loop = true;
        foreach (AudioSource sfx in sfxAudioSources)
            sfx.loop = false;

        Play_Bgm((int)StageBgm.stageBgm);
    }

    // bgm 플레이용 함수
    // StageBgm.stopBgm 으로 정지
    public void Play_Bgm(int idx)
    {
        switch (idx)
        {
            case (int)StageBgm.stopBgm:
                bgmAudioSource.Stop();
                break;
            case (int)StageBgm.stageBgm:
                bgmAudioSource.clip = stageBgm;
                bgmAudioSource.Play();
                break;
            default:
                // 설정되지 않은 값이 들어오면 표시
                Debug.Log("Check the bgmIdx");
                break;
        }
    }

    public void Play_Sfx(int idx)
    {
        //재생중이지 않은 오디오 소스 선택
        for (int i = 0; i < sfxAudioSources.Length; i++)
        {
            if (sfxAudioSources[i].isPlaying) continue;

            curSfxSource = sfxAudioSources[i];
            break;
        }
        //전부 재생중일 경우 마지막 소스 사용
        if (curSfxSource == null)
            curSfxSource = sfxAudioSources[sfxAudioSources.Length - 1];

        switch (idx)
        {
            case (int)StageSfx.jump:
                curSfxSource.clip = jump;
                curSfxSource.Play();
                break;
            case (int)StageSfx.savePoint:
                curSfxSource.clip = savePoint;
                curSfxSource.Play();
                break;
            case (int)StageSfx.reset:
                curSfxSource.clip = reset;
                curSfxSource.Play();
                break;
            case (int)StageSfx.stageClear:
                bgmAudioSource.Stop();
                curSfxSource.clip = stageClear;
                curSfxSource.Play();
                break;
            case (int)StageSfx.gameOver:
                bgmAudioSource.Stop();
                curSfxSource.clip = gameOver;
                curSfxSource.Play();
                break;
            default:
                // 설정되지 않은 값이 들어오면 표시
                Debug.Log("Check the sfxIdx");
                break;
        }
        curSfxSource = null; //재생 후 null로 초기화
    }
}