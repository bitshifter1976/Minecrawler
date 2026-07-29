using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Button buttonNew;
    public Button buttonContinue;
    public Button buttonEnd;

    public AudioSource audioSource;
    public AudioSource audioSourceAmbience;
    public AudioSource audioSourceWind;
    public AudioClip selectClip;
    public AudioClip startClip;
    public AudioClip musicClip;
    public AudioClip ambienceClip;
    public AudioClip windClip;

    public Button[] buttons;

    private GameObject lastSelectedObject;
    private AmbientEffectSpawner ambientEffectSpawner;
    private WaterDropSpawner waterDropSpawner;


    private void Start()
    {
        ambientEffectSpawner = gameObject.AddComponent<AmbientEffectSpawner>();
        waterDropSpawner = gameObject.AddComponent<WaterDropSpawner>();

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

        PlayerPrefs.SetInt("ContinueGame", 0);
        SceneManager.LoadScene("Game");
    }

    private void OnContinueGameClicked()
    {
        if (startClip != null)
            audioSource.PlayOneShot(startClip);

        PlayerPrefs.SetInt("ContinueGame", 1);
        SceneManager.LoadScene("Game");
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
        buttonEnd.onClick.RemoveListener(OnEndGameClicked);
    }
}