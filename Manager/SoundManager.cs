using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    AudioSource bgmAudioSource;
    AudioSource[] sfxAudioSources;
    AudioSource cursfxListource;

    //재생 함수 호출용 열거형
    public enum Bgm 
    { 
        StageBgm
    }
    // sfx 함수 호출용 열거형
    public enum Sfx 
    {
        Jump,
        JumpingPlatform,
        SavePoint,
        Reset,
        StageClear,
        GameOver
    }

    //bgm
    List<AudioClip> bgmList;
    public static Action<Bgm> PlayBgm;
    public static Action StopBgm;

    //sfx
    List<AudioClip> sfxList;
    public static Action<Sfx> PlaySfx;

    #region 초기화
    void Awake()
    {
        Initialize();
    }

    void Start()
    {
        LoadBgm();
        LoadSfx();
    }

    void Initialize()
    {
        // 대리자 등록
        PlayBgm = (bgmIdx) => { Play_Bgm(bgmIdx); };
        StopBgm = () => { Stop_Bgm(); };
        PlaySfx = (sfxIdx) => { Play_Sfx(sfxIdx); };

        // 오디오 소스 등록
        // 첫 AudioSoruce는 bgm 용으로, 나머지는 sfx용으로 저장
        AudioSource[] sources = GetComponentsInChildren<AudioSource>();
        bgmAudioSource = sources[0];

        sfxAudioSources = new AudioSource[sources.Length - 1];
        for (int i = 1; i < sources.Length; i++)
            sfxAudioSources[i - 1] = sources[i];
    }

    // 리소스 로드(bgm)
    void LoadBgm()
    {
        string[] bgmNames = Enum.GetNames(typeof(Bgm));
        bgmList = new List<AudioClip>(bgmNames.Length);
        for (int i = 0; i < bgmNames.Length; i++)
            ResourceManager.Instance.GetResource<AudioClip>(bgmNames[i], (audio) => { bgmList.Add(audio); });

        // bgm 오디오 소스는 루프 on
        bgmAudioSource.loop = true;
    }
    // 리소스 로드(sfx)
    void LoadSfx()
    {
        string[] sfxNames = Enum.GetNames(typeof(Sfx));
        sfxList = new List<AudioClip>(sfxNames.Length); 
        for (int i = 0; i < sfxNames.Length; i++)
            ResourceManager.Instance.GetResource<AudioClip>(sfxNames[i], (audio) => { sfxList.Add(audio); });

        // sfx 오디오 소스는 루프 off
        foreach (AudioSource sfx in sfxAudioSources)
            sfx.loop = false;
    }
    #endregion

    #region 재생 및 종료
    // bgm 플레이용 함수
    // 0 미만의 값으로 정지
    public void Play_Bgm(Bgm idx, float volume = 1.0f)
    {
        int bgmIdx = Convert.ToInt32(idx);
        // 잘못된 인덱스일 경우 로그 출력 후 종료
        if (bgmIdx < 0 || bgmIdx >= bgmList.Count)
        {
            Debug.Log($"Wrong bgmIdx: {bgmIdx}");
            return;
        }

        bgmAudioSource.clip = bgmList[bgmIdx];
        bgmAudioSource.volume = volume;
        bgmAudioSource.Play();
    }

    public void Stop_Bgm()
    {
        bgmAudioSource.Stop();
    }

    public void Play_Sfx(Sfx idx, float volume = 1.0f)
    {
        int sfxIdx = Convert.ToInt32(idx);
        // 잘못된 인덱스일 경우 로그 출력 후 종료
        if (sfxIdx < 0 || sfxIdx >= sfxList.Count)
        {
            Debug.Log($"Wrong sfxIdx: {sfxIdx}");
            return;
        }

        //재생중이지 않은 오디오 소스 선택
        for (int i = 0; i < sfxAudioSources.Length; i++)
        {
            if (sfxAudioSources[i].isPlaying) continue;

            cursfxListource = sfxAudioSources[i];
            break;
        }
        //전부 재생중일 경우 마지막 소스 사용
        if (cursfxListource == null)
            cursfxListource = sfxAudioSources[sfxAudioSources.Length - 1];

        cursfxListource.clip = sfxList[sfxIdx];
        cursfxListource.volume = volume;
        cursfxListource.Play();

        cursfxListource = null; //재생 후 null로 초기화
    }
    #endregion
}