using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class OptionsController : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const string FullscreenKey = "Fullscreen";
    private const string ResolutionKey = "Resolution";

    [Header("References")]
    [SerializeField] private Button buttonGame;
    [SerializeField] private Button buttonControls;
    [SerializeField] private Button buttonVideo;
    [SerializeField] private Button buttonKeybindings;
    [SerializeField] private Button buttonReturn;
    [SerializeField] private Button buttonQuit;
    [SerializeField] private CanvasRenderer panelGame;
    [SerializeField] private CanvasRenderer panelControls;
    [SerializeField] private CanvasRenderer panelVideo;
    [SerializeField] private CanvasRenderer panelKeybindings;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Display")]
    [SerializeField] private Button fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Scene")]
    [SerializeField] private string menuSceneName = "Menu";

    private Resolution[] availableResolutions;
    private CanvasRenderer activePanel;

    private void Start()
    {
        SetupResolutions();
        var settings = LoadSettings();

        activePanel = panelGame;
        buttonGame.onClick.AddListener(() => ShowPanel(panelGame));
        buttonControls.onClick.AddListener(() => ShowPanel(panelControls));
        buttonVideo.onClick.AddListener(() => ShowPanel(panelVideo));
        buttonKeybindings.onClick.AddListener(() => ShowPanel(panelKeybindings));
        buttonReturn.onClick.AddListener(() => OnSaveAndExit(settings));
        buttonQuit.onClick.AddListener(() => SceneManager.LoadScene("Menu"));

        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        fullscreenToggle.onClick.AddListener(SetFullscreen);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void OnSaveAndExit(GameSettings settings)
    {
        settings.Save();
        SceneManager.LoadScene(settings.LastSceneIndex);
    }

    private void ShowPanel(CanvasRenderer newActivePanel)
    {
        activePanel.gameObject.SetActive(false);
        newActivePanel.gameObject.SetActive(true);
        activePanel = newActivePanel;
    }

    private void OnDestroy()
    {
        musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.RemoveListener(SetSfxVolume);
        fullscreenToggle.onClick.RemoveListener(SetFullscreen);
        resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
    }

    private void SetupResolutions()
    {
        availableResolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        var options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            Resolution resolution = availableResolutions[i];

            string option =
                $"{resolution.width} × {resolution.height}";

            options.Add(option);

            if (resolution.width == Screen.currentResolution.width &&
                resolution.height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private GameSettings LoadSettings()
    {
        var settings = GameSettings.Load();

        musicVolumeSlider.SetValueWithoutNotify(settings.MusicVolume);
        sfxVolumeSlider.SetValueWithoutNotify(settings.SfxVolume);
        fullscreenToggle.gameObject.GetComponentInChildren<TMP_Text>().text = settings.IsFullscreen ? "on" : "off";
        resolutionDropdown.value = Array.FindIndex(availableResolutions, r =>
            r.width == settings.ResolutionWidth &&
            r.height == settings.ResolutionHeight);
        resolutionDropdown.RefreshShownValue();
        musicVolumeSlider.value = settings.MusicVolume;
        sfxVolumeSlider.value = settings.SfxVolume;
        return settings;
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        ApplyMusicVolume(value);
    }

    public void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat(SfxVolumeKey, value);
        ApplySfxVolume(value);
    }

    private void SetFullscreen()
    {
        var fullscreen = !Screen.fullScreen;
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
    }

    public void SetResolution(int resolutionIndex)
    {
        if (availableResolutions == null ||
            resolutionIndex < 0 ||
            resolutionIndex >= availableResolutions.Length)
        {
            return;
        }

        Resolution resolution = availableResolutions[resolutionIndex];

        Screen.SetResolution(
            resolution.width,
            resolution.height,
            Screen.fullScreen);

        PlayerPrefs.SetInt(ResolutionKey, resolutionIndex);
    }

    public void BackToMenu()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene(menuSceneName);
    }

    private void ApplyMusicVolume(float value)
    {
        SetMixerVolume("MusicVolume", value);
    }

    private void ApplySfxVolume(float value)
    {
        SetMixerVolume("SfxVolume", value);
    }

    private void SetMixerVolume(string parameterName, float value)
    {
        if (audioMixer == null)
        {
            return;
        }

        float decibels = value <= 0.0001f
            ? -80f
            : Mathf.Log10(value) * 20f;

        audioMixer.SetFloat(parameterName, decibels);
    }
}