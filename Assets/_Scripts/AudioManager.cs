using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : SingletonComponent<AudioManager>
{
    [SerializeField] AudioMixer mixer;
    [SerializeField] List<SoundHolder> sounds;
    List<AudioSource> _audioSources;
    string musicMixer = "Music";
    string effectsMixer = "SFX";
    string masterMixer = "Master";
    float mute = -80f;
    float unmute = 0f;

    private void Awake()
    {
        _audioSources = new List<AudioSource>();

        foreach (var sound in sounds)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = sound.Clip;
            source.outputAudioMixerGroup = sound.MixerGroup;
            source.loop = sound.Loop;
            source.volume = sound.Volume;
            source.playOnAwake = false;
            if (sound.PlayOnAwake)
                source.Play();

            _audioSources.Add(source);
        }
    }

    private AudioSource FindByName(string name)
    {
        for (int i = 0; i < _audioSources.Count; ++i)
            if (sounds[i].Clip.name == name)
                return _audioSources[i];

        Debug.LogError("AudioSource wasn't found: " + name);
        return null;
    }

    public void Play(string name) => FindByName(name).Play();

    public void Stop(string name) => FindByName(name).Stop();

    public void Pause(string name) => FindByName(name).Pause();

    public void UnPause(string name) => FindByName(name).UnPause();

    public void SetMusic(bool v)
    {
        if (v)
            mixer.SetFloat(musicMixer, unmute);
        else
            mixer.SetFloat(musicMixer, mute);
    }

    public void SetEffects(bool v)
    {
        if (v)
            mixer.SetFloat(effectsMixer, unmute);
        else
            mixer.SetFloat(effectsMixer, mute);
    }

    public void SetSound(bool v)
    {
        if (v)
            mixer.SetFloat(masterMixer, unmute);
        else
            mixer.SetFloat(masterMixer, mute);

    }
}
