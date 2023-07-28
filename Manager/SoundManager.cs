using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    AudioSource bgmAudioSource;
    AudioSource[] sfxAudioSources;
    AudioSource cursfxListource;

    //��� �Լ� ȣ��� ������
    public enum Bgm 
    { 
        StageBgm
    }
    // sfx �Լ� ȣ��� ������
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

    #region �ʱ�ȭ
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
        // �븮�� ���
        PlayBgm = (bgmIdx) => { Play_Bgm(bgmIdx); };
        StopBgm = () => { Stop_Bgm(); };
        PlaySfx = (sfxIdx) => { Play_Sfx(sfxIdx); };

        // ����� �ҽ� ���
        // ù AudioSoruce�� bgm ������, �������� sfx������ ����
        AudioSource[] sources = GetComponentsInChildren<AudioSource>();
        bgmAudioSource = sources[0];

        sfxAudioSources = new AudioSource[sources.Length - 1];
        for (int i = 1; i < sources.Length; i++)
            sfxAudioSources[i - 1] = sources[i];
    }

    // ���ҽ� �ε�(bgm)
    void LoadBgm()
    {
        string[] bgmNames = Enum.GetNames(typeof(Bgm));
        bgmList = new List<AudioClip>(bgmNames.Length);
        for (int i = 0; i < bgmNames.Length; i++)
            ResourceManager.Instance.GetResource<AudioClip>(bgmNames[i], (audio) => { bgmList.Add(audio); });

        // bgm ����� �ҽ��� ���� on
        bgmAudioSource.loop = true;
    }
    // ���ҽ� �ε�(sfx)
    void LoadSfx()
    {
        string[] sfxNames = Enum.GetNames(typeof(Sfx));
        sfxList = new List<AudioClip>(sfxNames.Length); 
        for (int i = 0; i < sfxNames.Length; i++)
            ResourceManager.Instance.GetResource<AudioClip>(sfxNames[i], (audio) => { sfxList.Add(audio); });

        // sfx ����� �ҽ��� ���� off
        foreach (AudioSource sfx in sfxAudioSources)
            sfx.loop = false;
    }
    #endregion

    #region ��� �� ����
    // bgm �÷��̿� �Լ�
    // 0 �̸��� ������ ����
    public void Play_Bgm(Bgm idx, float volume = 1.0f)
    {
        int bgmIdx = Convert.ToInt32(idx);
        // �߸��� �ε����� ��� �α� ��� �� ����
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
        // �߸��� �ε����� ��� �α� ��� �� ����
        if (sfxIdx < 0 || sfxIdx >= sfxList.Count)
        {
            Debug.Log($"Wrong sfxIdx: {sfxIdx}");
            return;
        }

        //��������� ���� ����� �ҽ� ����
        for (int i = 0; i < sfxAudioSources.Length; i++)
        {
            if (sfxAudioSources[i].isPlaying) continue;

            cursfxListource = sfxAudioSources[i];
            break;
        }
        //���� ������� ��� ������ �ҽ� ���
        if (cursfxListource == null)
            cursfxListource = sfxAudioSources[sfxAudioSources.Length - 1];

        cursfxListource.clip = sfxList[sfxIdx];
        cursfxListource.volume = volume;
        cursfxListource.Play();

        cursfxListource = null; //��� �� null�� �ʱ�ȭ
    }
    #endregion
}