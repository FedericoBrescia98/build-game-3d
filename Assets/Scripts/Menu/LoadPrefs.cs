using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadPrefs : MonoBehaviour
{
    [Header("General settings")] [SerializeField]
    private bool _canUse = false;

    [SerializeField] private MenuController _menuController;

    [Header("Volume settings")] [SerializeField]
    private TMP_Text _volumeTextValue = null;

    [SerializeField] private Slider _volumeSlider = null;

    [Header("Quality settings")] [SerializeField]
    private TMP_Dropdown _qualityDropdown;

    [Header("Fullscreen settings")] [SerializeField]
    private Toggle _fullscreenToggle;

    public void Awake()
    {
        if (_canUse)
        {
            if (PlayerPrefs.HasKey("masterVolume"))
            {
                float localVolume = PlayerPrefs.GetFloat("masterVolume");
                _volumeTextValue.text = localVolume.ToString("0.0");
                _volumeSlider.value = localVolume;
                AudioListener.volume = localVolume;
            }
            else
            {
                _menuController.ResetButton("Audio");
            }

            if (PlayerPrefs.HasKey("masterQuality"))
            {
                int localQuality = PlayerPrefs.GetInt("masterQuality");
                _qualityDropdown.value = localQuality;
                QualitySettings.SetQualityLevel(localQuality);
            }

            if (PlayerPrefs.HasKey("masterFullscreen"))
            {
                int localFullscreen = PlayerPrefs.GetInt("masterFullscreen");
                if (localFullscreen == 1)
                {
                    Screen.fullScreen = true;
                    _fullscreenToggle.isOn = true;
                }
                else
                {
                    Screen.fullScreen = false;
                    _fullscreenToggle.isOn = false;
                }
            }
        }
    }
}
