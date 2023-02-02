using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SoundHolder
{
    [SerializeField] AudioClip clip;
    [SerializeField] AudioMixerGroup mixerGroup;
    [SerializeField] bool loop;
    [SerializeField] bool playOnAwake;
    [Range(0f, 1f)] [SerializeField] float volume = 1f;

    public AudioClip Clip { get => clip; set => clip = value; }
    public AudioMixerGroup MixerGroup { get => mixerGroup; set => mixerGroup = value; }
    public bool Loop { get => loop; set => loop = value; }
    public float Volume { get => volume; set => volume = value; }
    public bool PlayOnAwake { get => playOnAwake; set => playOnAwake = value; }
}
