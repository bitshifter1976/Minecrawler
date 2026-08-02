using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Button buttonNew;
    public Button buttonContinue;
    public Button buttonOptions;
    public Button buttonEnd;

    public AudioMixer audioMixer;
    public AudioSource audioSource;
    public AudioSource audioSourceAmbience;
    public AudioSource audioSourceWind;
    public AudioClip selectClip;
    public AudioClip startClip;
    public AudioClip musicClip;
    public AudioClip ambienceClip;
    public AudioClip windClip;

    public TMP_Text versionText;

    public Button[] buttons;

    private GameObject lastSelectedObject;
    private GameSettings settings;

    private void Start()
    {
        settings = GameSettings.Load();
        settings.UseSettings(audioMixer);
        settings.LastSceneIndex = 0;

        versionText.text = MineGameManager.Version;

        gameObject.AddComponent<FallingRockSpawner>();
        gameObject.AddComponent<WaterDropSpawner>();
        gameObject.AddComponent<FireFlySpawner>();

        audioMixer.SetFloat("MusicVolume", Mathf.Log10(settings.MusicVolume) * 20);
        audioMixer.SetFloat("SfxVolume", Mathf.Log10(settings.SfxVolume) * 20);

        if (musicClip != null)
        {
            audioSource.clip = musicClip;
            audioSource.volume = 0.5f;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (ambienceClip != null)
        {
            audioSourceAmbience.clip = ambienceClip;
            audioSourceAmbience.volume = 1f;
            audioSourceAmbience.loop = true;
            audioSourceAmbience.Play();
        }

        if (windClip != null)
        {
            audioSourceWind.clip = windClip;
            audioSourceWind.volume = 0.5f;
            audioSourceWind.loop = true;
            audioSourceWind.Play();
        }

        buttonNew.onClick.AddListener(OnNewGameClicked);
        buttonContinue.onClick.AddListener(OnContinueGameClicked);
        buttonOptions.onClick.AddListener(OnOptionsClicked);   
        buttonEnd.onClick.AddListener(OnEndGameClicked);        

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonNew.gameObject);

        lastSelectedObject = buttonNew.gameObject;

        foreach (Button button in buttons)
        {
            AddHoverSelection(button);
        }
    }

    private void AddHoverSelection(Button button)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((_) =>
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        });

        trigger.triggers.Add(entry);
    }

    private void Update()
{
        var selectedObject = EventSystem.current.currentSelectedGameObject;
        if (selectedObject != null && selectedObject != lastSelectedObject)
        {
            if (selectClip != null)
                audioSource.PlayOneShot(selectClip);

            lastSelectedObject = selectedObject;
        }
    }

    private void OnNewGameClicked()
    {
        if (startClip != null)
            audioSource.PlayOneShot(startClip);

        settings.ResetForNewGame();
        SceneManager.LoadScene("Game");
    }

    private void OnContinueGameClicked()
    {
        if (startClip != null)
            audioSource.PlayOneShot(startClip);

        PlayerPrefs.SetInt("ContinueGame", 1);
        SceneManager.LoadScene("Game");
    }

    private void OnOptionsClicked()
    {
        if (startClip != null)
            audioSource.PlayOneShot(startClip);

        GameSettings settings = GameSettings.Load();
        settings.LastSceneIndex = 0;

        SceneManager.LoadScene("Options");
    }

    private void OnEndGameClicked()
    {
        if (startClip != null)
            audioSource.PlayOneShot(startClip);

        Application.Quit();
    }

    private void OnDestroy()
    {
        buttonNew.onClick.RemoveListener(OnNewGameClicked);
        buttonContinue.onClick.RemoveListener(OnContinueGameClicked);
        buttonOptions.onClick.RemoveListener(OnOptionsClicked);
        buttonEnd.onClick.RemoveListener(OnEndGameClicked);
    }
}