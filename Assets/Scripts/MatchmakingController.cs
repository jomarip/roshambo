using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;
using System;
using SocketIO;
using UnityEngine.SceneManagement;

public class MatchmakingController : MonoBehaviour
{
    public RectTransform gamesContainer;
    public GameObject spinner;
    public GameObject gameItemPrefab;
    public GameObject ownerGameItemPrefab;
    public Button createGameButton;
    // A list is a variable-sized array
    private List<GameObject> instantiatedGames = new List<GameObject>();
    public Text noGamesAvailableText;
    public Text errorText;
    private SocketIOComponent socket = SocketManager.socket;

    private void Start()
    {
        noGamesAvailableText.gameObject.SetActive(false);
        createGameButton.onClick.AddListener(OnCreateGameClicked);
        SocketEvents();
    }

    private void OnCreateGameClicked()
    {
        if (Static.isBoardEmpty)
        {
            ShowError("You need to setup your cards to create a game first in the initial screen");
        }
        else
        {
            SceneManager.LoadScene("CreateGame");
        }
    }

    public void DeleteGame()
    {
        ShowError("");
        socket.Emit("game:delete");
        foreach (GameObject instance in instantiatedGames)
        {
            Destroy(instance);
        }
        EmitGetGames();
    }

    void ToggleSpinner(bool isDisplayed)
    {
        spinner.SetActive(isDisplayed);
    }

    void ShowError(string msg)
    {
        errorText.text = msg;
    }

    // Executed when a user clicks on "Join" on an active game of the list
    public void JoinGame (
        string playerOne,
        string playerTwo,
        string gameName,
        string gameType,
        string rounds,
        string moveTimer
    ) {
        // Check if board is setup otherwise show error
        if (Static.isBoardEmpty)
        {
            ShowError("You need to setup your cards to play first");
            return;
        }

        // Send a join game event
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["playerOne"] = playerOne;
        data["playerTwo"] = playerTwo;
        data["gameName"] = gameName;
        data["gameType"] = gameType;
        data["rounds"] = rounds;
        data["moveTimer"] = moveTimer;

        // Count how many cards you have selected
        int cardCount = 0;
        foreach(string item in Static.board)
        {
            if (!string.IsNullOrEmpty(item))
            {
                cardCount++;
            }
        }
        data["totalCardsPlayerTwo"] = cardCount.ToString();

        socket.Emit("game:join", new JSONObject(data));
    }

    private void EmitGetGames()
    {
        Debug.Log("Getting games...");
        socket.Emit("game:get-games");
        ToggleSpinner(true);
    }

    private void SocketEvents() {
        EmitGetGames();

        // ON Game joined, this will be received by the creator of the game
        socket.On("game:join-complete", (SocketIOEvent e) =>
        {
            string data = e.data.ToString();
            JSONNode parsed = JSON.Parse(data);

            Static.roomId = parsed["roomId"].Value;
            Static.playerOne = parsed["playerOne"].Value;
            Static.playerTwo = parsed["playerTwo"].Value;
            Static.gameName = parsed["gameName"].Value;
            Static.gameType = parsed["gameType"].Value;
            Static.rounds = parsed["rounds"].Value;
            Static.moveTimer = parsed["moveTimer"].Value;
            Static.globalRocks = parsed["leagueRocksInGame"].Value;
            Static.globalScissors = parsed["leagueScissorsInGame"].Value;
            Static.globalPapers = parsed["leaguePapersInGame"].Value;

            // Move them to the right scene with the Static data
            SceneManager.LoadScene("Gameplay");
        });

        // When a new game is added
        socket.On("game:create-complete", data =>
        {
            EmitGetGames();
        });

        socket.On("game:get-games", (SocketIOEvent e) => {
            string msg = e.data.GetField("data").ToString();
            JSONNode parsed = JSON.Parse(msg);
            InstantiateGames(parsed);
            ToggleSpinner(false);
        });

        socket.On("issue", (SocketIOEvent e) =>
        {
            Debug.Log("Error socket " + e.data.GetField("msg").str);
            ShowError(e.data.GetField("msg").str);
            ToggleSpinner(false);
        });
    }

    private void InstantiateGames(JSONNode games)
    {
        // Clean previous game messages
        foreach (GameObject instance in instantiatedGames)
        {
            Destroy(instance);
        }
        if (noGamesAvailableText)
        {
            if (games.Count <= 0)
            {
                noGamesAvailableText.gameObject.SetActive(true);
            }
            else
            {
                noGamesAvailableText.gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < games.Count; i++)
        {
            bool isOwner = false;
            GameObject gameItem;

            // If you're the owner of this game, instantiate the special design
            if (Static.userId == games[i]["playerOne"].Value)
            {
                isOwner = true;
                gameItem = Instantiate(ownerGameItemPrefab) as GameObject;
                // Execute the delete game function on click
                gameItem.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
                {
                    DeleteGame();
                });
            }
            else
            {
                gameItem = Instantiate(gameItemPrefab) as GameObject;
                gameItem.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
                {
                    string playerOne = gameItem.transform.Find("userId").GetComponent<Text>().text;
                    string playerTwo = Static.userId;
                    string gameName = gameItem.transform.Find("Name").GetComponent<Text>().text;
                    string gameType = gameItem.transform.Find("DataGameType").GetComponent<Text>().text;
                    string rounds = gameItem.transform.Find("DataRounds").GetComponent<Text>().text;
                    string moveTimer = gameItem.transform.Find("DataMoveTimer").GetComponent<Text>().text;

                    JoinGame(
                        playerOne,
                        playerTwo,
                        gameName,
                        gameType,
                        rounds,
                        moveTimer
                    );
                });
            }
            // Add it to the list of instantiated
            instantiatedGames.Add(gameItem);

            // Name
            gameItem.transform.Find("Name").GetComponent<Text>().text =
                games[i]["gameName"].Value;

            // User id
            gameItem.transform.Find("userId").GetComponent<Text>().text =
                games[i]["playerOne"].Value;

            // Invisible game type
            gameItem.transform.Find("DataGameType").GetComponent<Text>().text =
                games[i]["gameType"].Value;

            // Invisible data rounds
            gameItem.transform.Find("DataRounds").GetComponent<Text>().text =
                games[i]["rounds"].Value;

            // Invisible move timer
            gameItem.transform.Find("DataMoveTimer").GetComponent<Text>().text =
                games[i]["moveTimer"].Value;

            // Visual rounds
            if (games[i]["gameType"].Value == "Rounds")
            {
                gameItem.transform.Find("Type").GetComponent<Text>().text =
                    games[i]["rounds"].Value + " rounds";
            }
            else
            {
                gameItem.transform.Find("Type").GetComponent<Text>().text =
                    games[i]["gameType"].Value;
            }
            // Move timer
            gameItem.transform.Find("Timer").GetComponent<Text>().text =
                games[i]["moveTimer"].Value + " seconds";
            gameItem.transform.SetParent(gamesContainer.transform, false);
            // Set your geme on top of the list
            if (isOwner)
            {
                gameItem.transform.SetAsFirstSibling();
            }
        }
    }
}
