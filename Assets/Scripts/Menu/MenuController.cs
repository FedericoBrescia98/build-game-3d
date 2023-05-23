using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject _noSavedGameDialog = null;

    [Header("Confirmation")] [SerializeField]
    private GameObject _confirmationPrompt = null;

    [Header("Volume settings")] [SerializeField]
    private TMP_Text _volumeTextValue = null;

    [SerializeField] private Slider _volumeSlider = null;
    [SerializeField] private float _defaultVolume = 0.5f;

    [Header("Graphics settings")] [SerializeField]
    private int _qualityLevel;

    private bool _isFullscreen;

    [Header("Resolutions dropdowns")] public TMP_Dropdown ResolutionDropdown;
    private Resolution[] resolutions;

    [Space(10)] [SerializeField] private TMP_Dropdown _qualityDropdown;
    [SerializeField] private Toggle _fullScreenToggle;


    private void Start()
    {
        resolutions = Screen.resolutions;
        ResolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        ResolutionDropdown.AddOptions(options);
        ResolutionDropdown.value = currentResolutionIndex;
        ResolutionDropdown.RefreshShownValue();
    }


    public void NewGameDialogYes()
    {
        PlayerPrefs.SetInt("masterLoad", 0);
        Resources.UnloadUnusedAssets();
        SceneManager.LoadScene(2);        
    }

    public void LoadGameDialogYes()
    {
        if (File.Exists(Application.persistentDataPath + "/save.txt"))
        {
            PlayerPrefs.SetInt("masterLoad", 1);
            Resources.UnloadUnusedAssets();
            SceneManager.LoadScene(2);
        }
        else
        {
            _noSavedGameDialog.SetActive(true);
        }
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    #region Sounds

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        _volumeTextValue.text = volume.ToString("0.0");
    }

    public void VolumeApply()
    {
        PlayerPrefs.SetFloat("masterVolume", AudioListener.volume);
        StartCoroutine(ConfirmationBox());
    }

    public void ResetButton(String menuType)
    {
        if (menuType == "Audio")
        {
            AudioListener.volume = _defaultVolume;
            _volumeSlider.value = _defaultVolume;
            _volumeTextValue.text = _defaultVolume.ToString("0.0");
            VolumeApply();
        }

        if (menuType == "Graphics")
        {
            _qualityDropdown.value = 1;
            QualitySettings.SetQualityLevel(1);
            _fullScreenToggle.isOn = false;
            Screen.fullScreen = false;
            Resolution currentResolution = Screen.currentResolution;
            Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreen);
            ResolutionDropdown.value = resolutions.Length;
            GraphicsApply();
        }
    }

    #endregion

    #region Graphics

    public void SetFullscreen(bool isFullscreen)
    {
        _isFullscreen = isFullscreen;
    }

    public void SetQuality(int qualityIndex)
    {
        _qualityLevel = qualityIndex;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void GraphicsApply()
    {
        PlayerPrefs.SetInt("masterQuality", _qualityLevel);
        QualitySettings.SetQualityLevel(_qualityLevel);
        PlayerPrefs.SetInt("masterFullscreen", _isFullscreen ? 1 : 0);
        Screen.fullScreen = _isFullscreen;
        StartCoroutine(ConfirmationBox());
    }

    #endregion

    public IEnumerator ConfirmationBox()
    {
        _confirmationPrompt.SetActive(true);
        yield return new WaitForSeconds(1);
        _confirmationPrompt.SetActive(false);
    }
}
