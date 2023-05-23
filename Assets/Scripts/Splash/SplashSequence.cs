using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashSequence : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _splashAudioClip;
    void Start()
    {
        StartCoroutine(ToMainMenu());
    }

    private IEnumerator ToMainMenu()
    {
        _audioSource.PlayOneShot(_splashAudioClip);
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(1);
    }
}
