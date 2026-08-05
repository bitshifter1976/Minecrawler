using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
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
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource audioSourceFx;
    [SerializeField] private AudioClip selectClip;
    [SerializeField] private AudioClip startClip;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Display")]
    [SerializeField] private Button fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Scene")]
    [SerializeField] private string menuSceneName = "Menu";

    private Resolution[] availableResolutions;
    private CanvasRenderer activePanel;
    private GameObject lastSelectedObject;

    private void Start()
    {
        SetupResolutions();
        var settings = LoadSettings();
        audioSource.clip = Resources.Load<AudioClip>("Audio/ambience");
        audioSource.loop = true;
        audioSource.Play();

        activePanel = panelGame;
        ConfigureNavigation();
        buttonGame.onClick.AddListener(() => ShowPanel(panelGame));
        buttonControls.onClick.AddListener(() => ShowPanel(panelControls));
        buttonVideo.onClick.AddListener(() => ShowPanel(panelVideo));
        buttonKeybindings.onClick.AddListener(() => ShowPanel(panelKeybindings));
        buttonReturn.onClick.AddListener(() => OnSaveAndExit(settings));
        buttonQuit.onClick.AddListener(() =>
        {
            PlayStartSound();
            SceneManager.LoadScene("Menu");
        });

        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        fullscreenToggle.onClick.AddListener(SetFullscreen);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);

        AddHoverSelection(buttonGame);
        AddHoverSelection(buttonControls);
        AddHoverSelection(buttonVideo);
        AddHoverSelection(buttonKeybindings);
        AddHoverSelection(buttonReturn);
        AddHoverSelection(buttonQuit);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(buttonGame.gameObject);
            lastSelectedObject = buttonGame.gameObject;
        }
    }


    private void Update()
    {
        if (EventSystem.current == null)
            return;

        GameObject selected =
            EventSystem.current.currentSelectedGameObject;

        if (selected == null ||
            selected == lastSelectedObject)
        {
            return;
        }

        PlaySelectSound();
        lastSelectedObject = selected;
    }

    private void AddHoverSelection(Button button)
    {
        if (button == null)
            return;

        EventTrigger trigger =
            button.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry =
            new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };

        entry.callback.AddListener(_ =>
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(button.gameObject);
        });

        trigger.triggers.Add(entry);
    }

    private void ConfigureNavigation()
    {
        SetNavigation(buttonGame, buttonQuit, buttonControls);
        SetNavigation(buttonControls, buttonGame, buttonVideo);
        SetNavigation(buttonVideo, buttonControls, buttonKeybindings);
        SetNavigation(buttonKeybindings, buttonVideo, buttonReturn);
        SetNavigation(buttonReturn, buttonKeybindings, buttonQuit);
        SetNavigation(buttonQuit, buttonReturn, buttonGame);
    }

    private static void SetNavigation(
        Button button,
        Selectable up,
        Selectable down)
    {
        if (button == null)
            return;

        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnUp = up;
        navigation.selectOnDown = down;
        button.navigation = navigation;
    }

    private void PlaySelectSound()
    {
        if (audioSourceFx != null &&
            selectClip != null)
        {
            audioSourceFx.PlayOneShot(selectClip);
        }
    }

    private void PlayStartSound()
    {
        if (audioSourceFx != null &&
            startClip != null)
        {
            audioSourceFx.PlayOneShot(startClip);
        }
    }

    private void OnSaveAndExit(GameSettings settings)
    {
        PlayStartSound();
        settings.Save();
        SceneManager.LoadScene(settings.LastSceneIndex);
    }

    private void ShowPanel(CanvasRenderer newActivePanel)
    {
        PlayStartSound();
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