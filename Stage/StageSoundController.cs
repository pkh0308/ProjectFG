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
        StopBgm = -1,
        StageBgm = 1000
    }
    // sfx �Լ� ȣ��� ������
    public enum StageSfx 
    {
        Jump = 2000,
        JumpingPlatform,
        SavePoint,
        Reset,
        StageClear,
        GameOver
    }

    //bgm
    [Header("Bgm")]
    [SerializeField] AudioClip stageBgm;
    public static Action<StageBgm> PlayBgm;

    //sfx
    [Header("Sfx")]
    [SerializeField] AudioClip jump;
    [SerializeField] AudioClip jumpingPlatform;
    [SerializeField] AudioClip savePoint;
    [SerializeField] AudioClip reset;
    [SerializeField] AudioClip stageClear;
    [SerializeField] AudioClip gameOver;
    public static Action<StageSfx> PlaySfx;

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

        Play_Bgm(StageBgm.StageBgm);
    }

    // bgm �÷��̿� �Լ�
    // StageBgm.stopBgm ���� ����
    public void Play_Bgm(StageBgm idx)
    {
        switch (idx)
        {
            case StageBgm.StopBgm:
                bgmAudioSource.Stop();
                break;
            case StageBgm.StageBgm:
                bgmAudioSource.clip = stageBgm;
                bgmAudioSource.Play();
                break;
            default:
                // �������� ���� ���� ������ ǥ��
                Debug.Log("Check the bgmIdx");
                break;
        }
    }

    public void Play_Sfx(StageSfx idx)
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
            case StageSfx.Jump:
                curSfxSource.clip = jump;
                curSfxSource.Play();
                break;
            case StageSfx.JumpingPlatform:
                curSfxSource.clip = jump;
                curSfxSource.Play();
                break;
            case StageSfx.SavePoint:
                curSfxSource.clip = savePoint;
                curSfxSource.Play();
                break;
            case StageSfx.Reset:
                curSfxSource.clip = reset;
                curSfxSource.Play();
                break;
            case StageSfx.StageClear:
                bgmAudioSource.Stop();
                curSfxSource.clip = stageClear;
                curSfxSource.Play();
                break;
            case StageSfx.GameOver:
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