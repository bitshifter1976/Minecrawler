using System;
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
    public AudioClip selectClip;
    public AudioClip startClip;
    public AudioClip mineVibesClip;
    private MineControlsGenerated controls;

    private void Awake()
    {
        controls = new MineControlsGenerated();
    }

    private void Start()
    {
        audioSource.PlayOneShot(mineVibesClip);
        buttonNew.Select();
        buttonNew.onClick.AddListener(OnNewGameClicked);
        buttonContinue.onClick.AddListener(OnContinueGameClicked);
        buttonEnd.onClick.AddListener(OnEndGameClicked);
    }

    private void OnEndGameClicked()
    {
        audioSource.PlayOneShot(startClip);
        Application.Quit();
    }

    private void OnContinueGameClicked()
    {
        audioSource.PlayOneShot(startClip);
        PlayerPrefs.SetInt("ContinueGame", 1);
        SceneManager.LoadScene("Game");
    }

    private void OnNewGameClicked()
    {
        audioSource.PlayOneShot(startClip);
        PlayerPrefs.SetInt("ContinueGame", 0);
        SceneManager.LoadScene("Game");
    }

    private void OnEnable()
    {
        controls.Menu.Select.performed += OnMove;
        controls.Menu.Enable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        audioSource.PlayOneShot(selectClip);
        var input = context.ReadValue<Vector2>();
        if (input != null) 
        {
            if (input.x > 0 || input.y > 0)
            {
                SetPreviousButtonActive();
            }
            else
            {
                SetNextButtonActive();
            }
        }
    }

    private void SetPreviousButtonActive()
    {
        if (EventSystem.current.currentSelectedGameObject == buttonNew.gameObject)
        {
            buttonEnd.Select();
        }
        else if (EventSystem.current.currentSelectedGameObject == buttonContinue.gameObject)
        {
            buttonNew.Select();
        }
        else if (EventSystem.current.currentSelectedGameObject == buttonEnd.gameObject)
        {
            buttonContinue.Select();
        }
    }

    private void SetNextButtonActive()
    {
        if (EventSystem.current.currentSelectedGameObject == buttonNew.gameObject)
        {
            buttonContinue.Select();
        }
        else if (EventSystem.current.currentSelectedGameObject == buttonContinue.gameObject)
        {
            buttonEnd.Select();
        }
        else if (EventSystem.current.currentSelectedGameObject == buttonEnd.gameObject)
        {
            buttonNew.Select();
        }
    }

    private void OnDisable()
    {
        if (controls == null)
            return;
        controls.Menu.Select.performed -= OnMove;
        controls.Menu.Disable();
        buttonNew.onClick.RemoveListener(OnNewGameClicked);
        buttonContinue.onClick.RemoveListener(OnContinueGameClicked);
        buttonEnd.onClick.RemoveListener(OnEndGameClicked);
    }

    private void OnDestroy()
    {
        controls?.Dispose();
    }
}
