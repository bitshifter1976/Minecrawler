using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Button buttonNew;
    public Button buttonContinue;
    public Button buttonEnd;

    public AudioSource audioSource;
    public AudioClip selectClip;
    public AudioClip startClip;
    public AudioClip musicClip;

    private GameObject lastSelectedObject;
    public AmbientEffectSpawner ambientEffectSpawner;

    private void Start()
    {
        ambientEffectSpawner = gameObject.AddComponent<AmbientEffectSpawner>();
        //ambientEffectSpawner.gameObject.AddComponent<FallingRock>();
        if (musicClip != null)
        {
            audioSource.clip = musicClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        buttonNew.onClick.AddListener(OnNewGameClicked);
        buttonContinue.onClick.AddListener(OnContinueGameClicked);
        buttonEnd.onClick.AddListener(OnEndGameClicked);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonNew.gameObject);

        lastSelectedObject = buttonNew.gameObject;
    }

    private void Update()
    {
        GameObject selectedObject =
            EventSystem.current.currentSelectedGameObject;

        if (selectedObject != null &&
            selectedObject != lastSelectedObject)
        {
            if (selectClip != null)
                audioSource.PlayOneShot(selectClip);

            lastSelectedObject = selectedObject;
        }

        // Die Maus kann die aktuelle Tastaturauswahl verlieren lassen.
        // Sobald wieder eine Navigationstaste gedrückt wird,
        // kümmert sich das InputSystemUIInputModule darum.
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