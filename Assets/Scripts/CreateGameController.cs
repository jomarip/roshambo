using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SimpleJSON;
using SocketIO;

public class CreateGameController : MonoBehaviour
{
	public Button createGameButton;
	public GameObject spinner;
	public Text errorMessage;
    public Text successMessage;
    public InputField gameName;
    public Dropdown gameType;
    public InputField roundsNumber;
    public InputField moveTimer;
    public int defaultRoundNumber = 5;
    public int defaultMoveTimer = 10;
    private SocketIOComponent socket = SocketManager.socket;

    // Start is called before the first frame update
    void Start()
    {
        ToggleSpinner(false);
        createGameButton.onClick.AddListener(CreateGame);
        SocketEvents();
    }

    void CreateGame()
	{
        ShowError("");
        ShowSuccess("");

        ToggleSpinner(true);
        int selectedDropdownValue = gameType.value;
        string selectedDropdownText = gameType.options[gameType.value].text;
        string selectedRoundNumber = roundsNumber.text;
        string selectedMoveTimer = moveTimer.text;

        if (gameName.text.Length <= 0)
        {
            ShowError("The game name can't be empty");
            return;
        }
        if (selectedDropdownValue == 0)
        {
            ShowError("You must select a game type");
            return;
        }
        if (selectedRoundNumber.Length == 0)
        {
            selectedRoundNumber = defaultRoundNumber.ToString();
        }
        if (selectedMoveTimer.Length == 0)
        {
            selectedMoveTimer = defaultMoveTimer.ToString();
        }

        // Send gameName, gameType, rounds, moveTimer
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["gameName"] = gameName.text;
        data["gameType"] = selectedDropdownText;
        data["rounds"] = selectedRoundNumber;
        data["moveTimer"] = selectedMoveTimer;

        // Count how many cards you have selected
        int cardCount = 0;
        foreach (string item in Static.board)
        {
            if (!string.IsNullOrEmpty(item))
            {
                cardCount++;
            }
        }
        data["totalCardsPlayerOne"] = cardCount.ToString();

        socket.Emit("game:create", new JSONObject(data));
    }

	void ShowError(string msg)
	{
		this.errorMessage.text = msg;
        if (msg.Length > 0)
        {
            Debug.Log("Error " + msg);
        }
        ToggleSpinner(false);
    }

    void ShowSuccess(string msg)
    {
        this.successMessage.text = msg;
        ToggleSpinner(false);
    }

    void ToggleSpinner(bool isDisplayed)
	{
        createGameButton.gameObject.SetActive(!isDisplayed);
		spinner.SetActive(isDisplayed);
	}

    private void SocketEvents()
    {
        socket.On("game:create-complete", (SocketIOEvent e) =>
        {
            ShowSuccess(e.data.GetField("msg").str);
            StartCoroutine(LoadMatchmaking());
        });
        socket.On("issue", (SocketIOEvent e) =>
        {
            ShowError(e.data.GetField("msg").str);
            Debug.Log("Socket error received " + e.data);
        });
    }

    IEnumerator LoadMatchmaking()
    {
        yield return new WaitForSeconds(Static.timeAfterAction);
        SceneManager.LoadScene("Matchmaking");
    }
}
