using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioPlayer : MonoBehaviour
{    
    public AudioSource AudioSource;
    public AudioClip ClickSound;    
    public AudioClip BuildSound;
    public AudioClip DestroySound;
    public AudioClip ErrorSound;

    public static AudioPlayer instance;

    public float MusicVolume = 0.5f;
    public float FXVolume = 0.5f;



    private void Awake()
    {
        if(instance == null)
            instance = this;
        else if(instance != this)
            Destroy(this.gameObject);
    }

    void Start()
    {
        AudioSource.Play();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
        {
            AudioSource.PlayOneShot(ClickSound, FXVolume);
        }
    }

    public void PlayBuildSound()
    {
        AudioSource.PlayOneShot(BuildSound, FXVolume);
    }

    public void PlayDestroySound()
    {
        AudioSource.PlayOneShot(DestroySound, FXVolume);
    }

    public void PlayErrorSound()
    {
        AudioSource.PlayOneShot(ErrorSound, FXVolume);
    }
}
