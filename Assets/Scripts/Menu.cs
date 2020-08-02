using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject background;
    public Button menuButton;
    public Button logoutButton;
    public Canvas canvas;
    public GameObject tutorialContainer;
    public Button tutorialButton;
    private int initialCanvasSortingOrder;
    private bool isActive;
    private bool isTutorialActive;

    // Start is called before the first frame update
    void Start()
    {
        initialCanvasSortingOrder = canvas.sortingOrder;
        // Make sure the background is off by default
        ToggleActive(false);
        // When you click on the background, close the menu
        background.GetComponent<Button>().onClick.AddListener(() =>
        {
            ToggleActive(false);
            ToggleTutorial(false);
        });
        tutorialButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            ToggleTutorial(true);
        });
        tutorialContainer.GetComponent<Button>().onClick.AddListener(() =>
        {
            ToggleTutorial(false);
        });
        menuButton.onClick.AddListener(() =>
        {
            ToggleActive(!isActive);
            ToggleTutorial(false);
        });
        logoutButton.onClick.AddListener(LogOut);
    }

    void ToggleActive(bool on)
    {
        isActive = on;
        background.SetActive(on);
        if (on)
        {
            canvas.sortingOrder = 1000;
        } else
        {
            canvas.sortingOrder = initialCanvasSortingOrder;
        }
    }

    void ToggleTutorial(bool on)
    {
        tutorialContainer.SetActive(on);
    }

    void LogOut()
    {
        StartCoroutine(GetLogOut());
    }

    IEnumerator GetLogOut()
    {
        WWWForm form = new WWWForm();

        UnityWebRequest www = UnityWebRequest.Get(Static.serverUrl + "/user/logout");
        yield return www.SendWebRequest();
        SceneManager.LoadScene("Start");
    }
}
