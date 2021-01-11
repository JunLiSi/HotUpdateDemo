using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager instance {
        get {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType<AudioManager>();
                if (!_instance)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                    _instance.audioSource = go.AddComponent<AudioSource>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private AudioSource audioSource;
    private AudioClip clip {
        set {
            audioSource.clip = value;
            audioTime = value.length;
        }
    }
    private float audioTime;//AudioClip总时间
    private float pauseTime;//暂停时间
    private float needPlayTime;//需要播放多长时间
    public bool isLoop {
        get {
            return audioSource.loop;
        }
        set {
            audioSource.loop = value;
        }
    }//是否循环播放
    
    public Action<float> OnProgressEvent;//播放进度事件
    public Action OnAudioEndEvent;//播放结束事件
    
    /// <summary>
    /// 播放音频
    /// </summary>
    /// <param name="_clip">AudioClip</param>
    /// <param name="_progressAct">进度委托</param>
    /// <param name="_endAct">结束委托</param>
    /// <param name="force">是否强制播放</param>
    public void Play(AudioClip _clip,Action<float> _progressAct,Action _endAct,bool force=false) {
        if (!force)//非强制播放
        {
            if (isPlaying)
            {
                return;
            }
            clip = _clip;
            OnProgressEvent = _progressAct;
            OnAudioEndEvent = _endAct;
            Play();
        }
        else {
            clip = _clip;
            OnProgressEvent = _progressAct;
            OnAudioEndEvent = _endAct;
            Play();
        }
    }
    
    //继续播放
    public void Continue() {
        if (isPlaying)
        {
            return;
        }
        audioSource.time = pauseTime;
        audioSource?.Play();
        needPlayTime = audioTime - pauseTime;
        if (isPlaying)
        {
            StartCoroutine("AudioProgressEventIE");
            if (!isLoop)
            {
                StartCoroutine("AudioEndEventIE");
            }
        }
    }

    //停止播放
    public void Stop() {
        if (isPlaying)
        {
            pauseTime = audioSource.time;
            audioSource.Stop();
            StopCoroutine("AudioEndEventIE");
            StopCoroutine("AudioProgressEventIE");
        }
    }

    //播放
    public void Play(float time=0) {
        if (isPlaying)
        {
            StopCoroutine("AudioProgressEventIE");
            StopCoroutine("AudioEndEventIE");
        }
        audioSource.time = time;
        audioSource?.Play();
        if (isPlaying)
        {
            needPlayTime = audioTime-time;
            StartCoroutine("AudioProgressEventIE");
            if (!isLoop)
            {
                StartCoroutine("AudioEndEventIE");
            }
        }
       
    }

    //是否正在播放
    public bool isPlaying {
        get {
            if (!GetClip())
            {
                return false;
            }
            return audioSource.isPlaying;
        }
    }

    //得到AudioClip
    public AudioClip GetClip() {
        if (!audioSource)
        {
            return null;
        }
        return audioSource.clip;
    }

    //得到当前进度
    private float GetProgressValue() {
        if (!GetClip())
        {
            return 0;
        }
        float curTime = audioSource.time;
        return curTime / audioTime;
        
    }

    //执行结束委托
    private IEnumerator AudioEndEventIE() {
        yield return new WaitForSeconds(needPlayTime);
        pauseTime = 0;
        OnAudioEndEvent?.Invoke();
    }

    //执行进度委托
    private IEnumerator AudioProgressEventIE() {
        while (isPlaying)
        {
            OnProgressEvent?.Invoke(GetProgressValue());
            yield return null;
        }
    }

}
