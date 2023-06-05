using System;
using UnityEngine;

public class StageSoundController : MonoBehaviour
{
    AudioSource bgmAudioSource;
    AudioSource[] sfxAudioSources;
    AudioSource curSfxSource;

    //��� �Լ� ȣ��� ������
    public enum StageBgm 
    { 
        stopBgm = -1,
        stageBgm = 1000
    }
    // sfx �Լ� ȣ��� ������
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
        // ù AudioSoruce�� bgm ������, �������� sfx������ ����
        bgmAudioSource = sources[0];
        sfxAudioSources = new AudioSource[sources.Length - 1];
        for (int i = 1; i < sources.Length; i++)
            sfxAudioSources[i - 1] = sources[i];
    }

    void Start()
    {
        // loop ������ bgm�� true, sfx�� false�� ����
        bgmAudioSource.loop = true;
        foreach (AudioSource sfx in sfxAudioSources)
            sfx.loop = false;

        Play_Bgm((int)StageBgm.stageBgm);
    }

    // bgm �÷��̿� �Լ�
    // StageBgm.stopBgm ���� ����
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
                // �������� ���� ���� ������ ǥ��
                Debug.Log("Check the bgmIdx");
                break;
        }
    }

    public void Play_Sfx(int idx)
    {
        //��������� ���� ����� �ҽ� ����
        for (int i = 0; i < sfxAudioSources.Length; i++)
        {
            if (sfxAudioSources[i].isPlaying) continue;

            curSfxSource = sfxAudioSources[i];
            break;
        }
        //���� ������� ��� ������ �ҽ� ���
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
                // �������� ���� ���� ������ ǥ��
                Debug.Log("Check the sfxIdx");
                break;
        }
        curSfxSource = null; //��� �� null�� �ʱ�ȭ
    }
}