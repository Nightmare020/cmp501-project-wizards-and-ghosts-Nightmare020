using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class NavegationSounds : MonoBehaviour
{
    //parameters
    [SerializeField] private AudioClip Select, Scroll, slider;
    [SerializeField] private AudioMixerGroup _audioMixerGroup;

    private AudioSource _audioSource;

    //data

    // Start is called before the first frame update
    void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();

        //initialize audio source
        _audioSource.outputAudioMixerGroup = _audioMixerGroup;
        _audioSource.loop = false;
        _audioSource.playOnAwake = false;
    }

    public void PlaySelectSound()
    {
        _audioSource.clip = Select;
        _audioSource.Play();
    }

    public void PlayScrollSound()
    {
        _audioSource.clip = Scroll;
        _audioSource.Play();
    }

    public void PlaySliderSound()
    {
        _audioSource.clip = slider;
        _audioSource.Play();
    }
}