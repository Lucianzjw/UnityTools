using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private GameObject _soundAudioPrefab;
    public AudioSource BgmAudioSource { get; private set; }
    public AudioSource VoiceAudioSource { get; private set; }
    //private AssetBundle AudioAB;
    private List<AudioSource> _soundAudioSourceList = new List<AudioSource>();
    public List<AudioSource> CurUsingSoundAudioSourceList { get; private set; } = new List<AudioSource>();

    private List<AudioClip> _usedAudioClipList = new List<AudioClip>();

    public override void Awake()
    {
        base.Awake();
        BgmAudioSource = transform.GetChild(0).GetComponent<AudioSource>();
        VoiceAudioSource = transform.GetChild(1).GetComponent<AudioSource>();
    }

    
    /// <summary>
    /// 播放bgm，同时间最多共存一个
    /// </summary>
    /// <param name="clipName">音频名字</param>
    /// <param name="volume">音量</param>
    /// <param name="isLoop">是否循环</param>
    /// <param name="beginCallback">开始前的action</param>
    /// <param name="endCallback">回调</param>
    /// <returns></returns>
    public void PlayBGM(string clipName, float volume, bool isLoop = true, Action beginCallback = null, Action endCallback = null)
    {
        StartCoroutine(BGM(clipName, volume, isLoop, beginCallback, endCallback));
    }

    /// <summary>
    /// 播放语音，同时间最多共存一个
    /// </summary>
    /// <param name="clipName">音频名字</param>
    /// <param name="volume">音量</param>
    /// <param name="isLoop">是否循环</param>
    /// <param name="beginCallback">开始前的action</param>
    /// <param name="endCallback">回调</param>
    /// <returns></returns>
    public void PlayVoice(string clipName, float volume, bool isLoop = false, Action beginCallback = null, Action endCallback = null)
    {
        StartCoroutine(Voice(clipName, volume, isLoop, beginCallback, endCallback));
    }

    /// <summary>
    /// 播放音效，同时间可以有多个
    /// </summary>
    /// <param name="clipName">音频名字</param>
    /// <param name="volume">音量</param>
    /// <param name="isLoop">是否循环</param>
    /// <param name="beginCallback">开始前的action</param>
    /// <param name="endCallback">回调</param>
    /// <returns></returns>
    public void PlaySound(string clipName, float volume, bool isLoop = false, Action beginCallback = null, Action endCallback = null)
    {
        StartCoroutine(Sound(clipName, volume, isLoop, beginCallback, endCallback));
    }
    
    private IEnumerator BGM(string clipName, float volume, bool isLoop = true, Action beginCallback = null, Action endCallback = null)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Audios/{clipName}");
        if (_usedAudioClipList.Contains(clip) == false) _usedAudioClipList.Add(clip);
        beginCallback?.Invoke();
        //BgmAudioSource.clip =  AudioAB.LoadAsset<AudioClip>(clipName);
        BgmAudioSource.clip = clip;
        BgmAudioSource.volume = volume;
        BgmAudioSource.loop = isLoop;
        BgmAudioSource.Play();
        yield return new WaitUntil((() => BgmAudioSource.isPlaying == false));
        endCallback?.Invoke();
    }
    
    private IEnumerator Voice(string clipName, float volume, bool isLoop=false, Action beginCallback = null, Action endCallback = null)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Audios/{clipName}");
        Debug.Log(clip);
        Debug.Log(VoiceAudioSource.name);
        if (_usedAudioClipList.Contains(clip) == false) _usedAudioClipList.Add(clip);
        beginCallback?.Invoke();
        //VoiceAudioSource.clip = AudioAB.LoadAsset<AudioClip>(clipName);
        VoiceAudioSource.clip = clip;
        VoiceAudioSource.volume = volume;
        VoiceAudioSource.loop = isLoop;
        VoiceAudioSource.Play();
        yield return new WaitUntil((() => VoiceAudioSource.isPlaying == false));
        endCallback?.Invoke();
    }
    
    private IEnumerator Sound(string clipName, float volume, bool isLoop=false, Action beginCallback = null, Action endCallback = null)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Audios/{clipName}");
        if (_usedAudioClipList.Contains(clip) == false) _usedAudioClipList.Add(clip);
        AudioSource curAudio = GetSoundAudio();
        beginCallback?.Invoke();
        //curAudio.clip = AudioAB.LoadAsset<AudioClip>(clipName);
        curAudio.clip = clip;
        curAudio.volume = volume;
        curAudio.loop = isLoop;
        curAudio.Play();
        yield return new WaitUntil((() => curAudio.isPlaying == false));
        RecycleSoundAudio(curAudio);
        endCallback?.Invoke();
    }
    
    /// <summary>
    /// 获取可用SoundAudio
    /// </summary>
    /// <returns></returns>
    private AudioSource GetSoundAudio()
    {
        AudioSource soundAduio = null;
        if (_soundAudioSourceList.Count==0)
        {
            soundAduio = Instantiate(_soundAudioPrefab, transform).GetComponent<AudioSource>();
            CurUsingSoundAudioSourceList.Add(soundAduio);
            return soundAduio;
        }

        soundAduio = _soundAudioSourceList[0];
        _soundAudioSourceList.Remove(soundAduio);
        CurUsingSoundAudioSourceList.Add(soundAduio);
        soundAduio.gameObject.SetActive(true);
        return soundAduio;
    }

    /// <summary>
    /// 回收SoundAudio
    /// </summary>
    /// <param name="audioSource"></param>
    /// <returns></returns>
    void RecycleSoundAudio(AudioSource audioSource)
    {
        if (_soundAudioSourceList.Contains(audioSource)) return;
        Debug.Log($"回收soundAudio:{audioSource.name}");
        audioSource.Stop();
        audioSource.gameObject.SetActive(false);
        CurUsingSoundAudioSourceList.Remove(audioSource);
        _soundAudioSourceList.Add(audioSource);
    }

    /// <summary>
    /// 停掉指定音频播放器
    /// </summary>
    /// <param name="audioSource"></param>
    public void StopAudio(AudioSource audioSource)
    {
        audioSource.Stop();
    }
    
    /// <summary>
    /// 停掉所有音频
    /// </summary>
    public void StopAll()
    {
        BgmAudioSource.Stop();
        VoiceAudioSource.Stop();
        foreach (var soundAudioSource in CurUsingSoundAudioSourceList)
        {
            soundAudioSource.Stop();
        }
    }

    /// <summary>
    /// 获取指定音频时间
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public float GetAudioLength(string name)
    {
        //return AudioAB.LoadAsset<AudioClip>(name).length;
        foreach (var clip in _usedAudioClipList)
        {
            if (clip.name == name)
            {
                return clip.length;
            }
        }
        
        AudioClip audioClip= Resources.Load<AudioClip>(name);
        _usedAudioClipList.Add(audioClip);
        return audioClip.length;
    }

    public void UnloadAllUsedAudioClip()
    {
        foreach (var audioClip in _usedAudioClipList)
        {
            Debug.Log($">>Unload>>{audioClip}");
            Resources.UnloadAsset(audioClip);
        }
        _usedAudioClipList.Clear();
    }
}