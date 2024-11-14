using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioSource effect;
    [SerializeField] private Dictionary<string, AudioClip> audioPool = new Dictionary<string, AudioClip>();

    [HideInInspector] public bool isInit;
    public void Init()
    {
    }

    public async void PlayBgm(string key, bool isLoop = true)
    {
        if (!audioPool.ContainsKey(key))
            audioPool.Add(key, await ResourceManager.instance.LoadAsset<AudioClip>(key, eAddressableType.Audio));
        source.clip = audioPool[key];
        source.loop = isLoop;
        source.Play();
    }

    public async void PlayOneShot(string key)
    {
        if (!audioPool.ContainsKey(key))
            audioPool.Add(key, await ResourceManager.instance.LoadAsset<AudioClip>(key, eAddressableType.Audio));
        effect.PlayOneShot(audioPool[key]);
    }

    public void StopBgm()
    {
        source.Stop();
    }

    public void SetBgmVolume(float volume)
    {
        source.volume = volume;
    }

    public void SetEffectVolume(float volume)
    {
        effect.volume = volume;
    }
}