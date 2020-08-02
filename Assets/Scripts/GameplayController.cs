using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocketIO;
using TMPro;
using UnityEngine.SceneManagement;
using SimpleJSON;
using System;
using UnityEngine.EventSystems;

public class GameplayController : MonoBehaviour
{
    private GameObject clicked;
    public GameObject cardPlacement;
    private Vector2 clickedInitialPosition = new Vector2(0, 0);
    private readonly SocketIOComponent socket = SocketManager.socket;
    public TextMeshProUGUI gameName;
    public TextMeshProUGUI playerOne;
    public TextMeshProUGUI playerTwo;
    public TextMeshProUGUI gameType;
    public TextMeshProUGUI currentRoundVisual;
    public TextMeshProUGUI notification;
    public TextMeshProUGUI moveTimerText;
    public TextMeshProUGUI errorText;
    private int currentRound = 1;
    public GameObject[] allyStars;
    public GameObject[] enemyStars;
    private GameObject placedCard; // The card in the active section
    public GameObject playerOneCardsContainer;
    public GameObject playerTwoCardsContainer;
    private bool isPlayerOne;
    public TextMeshProUGUI globalRocks;
    public TextMeshProUGUI globalScissors;
    public TextMeshProUGUI globalPapers;

    public GameObject rockCard;
    public GameObject paperCard;
    public GameObject scissorCard;
    public GameObject[] placeholders;
    public GameObject placementCard; // Where your card is placed
    private string myCardTag = "MyCard";
    private Vector2 selectedCardInitialPosition;
    // Used to update the board when your placed card is used and the round is over
    private string lastCardInBoardUsed;

    private void Start()
    {
        SetupInitialData();
        SocketEvents();
        if (Static.userId == Static.playerOne) isPlayerOne = true;
        DisplayCards();
    }

    private void DisplayCards() {
        for (int i = 0; i < Static.board.Length; i++)
        {
            if (!string.IsNullOrEmpty(Static.board[i]))
            {
                GameObject instance = null;
                switch (Static.board[i])
                {
                    case "Paper":
                        instance = Instantiate(paperCard);
                        instance.name = "Paper";
                        break;
                    case "Rock":
                        instance = Instantiate(rockCard);
                        instance.name = "Rock";
                        break;
                    case "Scissors":
                        instance = Instantiate(scissorCard);
                        instance.name = "Scissors";
                        break;
                }
                instance.tag = myCardTag;
                Transform clonesContainer = GameObject.Find("ClonesContainer").transform;
                instance.transform.SetParent(clonesContainer);
                // Place a new instance in the right placeholder
                instance.transform.position = placeholders[i].transform.position;
            }
        }
        //foreach (Transform child in playerOneCardsContainer.transform)
        //{
        //    child.Find("Open").gameObject.SetActive(isPlayerOne);
        //}
        //foreach (Transform child in playerTwoCardsContainer.transform)
        //{
        //    child.Find("Open").gameObject.SetActive(!isPlayerOne);
        //}
    }

    private void SetupInitialData()
    {
        gameName.text = Static.gameName;
        if (Static.gameType == "Rounds")
        {
            gameType.text = Static.rounds + " " + Static.gameType;
        }
        else
        {
            gameType.text = Static.gameType;
        }
        playerOne.text = Static.playerOne;
        playerTwo.text = Static.playerTwo;
        moveTimerText.text = Static.moveTimer + " seconds";
        globalRocks.text = Static.globalRocks;
        globalScissors.text = Static.globalScissors;
        globalPapers.text = Static.globalPapers;
    }

    // Update is called once per frame
    void Update()
    {
        // Constantly emit raycasts
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        Vector3 mousePos = Input.mousePosition;
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        if (results.Count > 1)
        {
            GameObject currentClicked = results[0].gameObject;
            GameObject clickedBelow = results[1].gameObject;

            if (Input.GetMouseButtonDown(0))
            {
                // Place a card on the placeholder
                if (clicked && clickedBelow.name == placementCard.name)
                {
                    // Place it on the active area
                    clicked.transform.position = placementCard.transform.position;
                    EmitPlaced(clicked.name);
                    placedCard = clicked;
                    clicked = null;
                }
                // First click
                else if (!clicked && currentClicked.tag == myCardTag)
                {
                    currentClicked.tag = "Placed";
                    selectedCardInitialPosition = currentClicked.transform.position;
                    clicked = currentClicked;

                    lastCardInBoardUsed = currentClicked.name;
                }
                // Drop the selected card
                else if (clicked)
                {
                    clicked.transform.position = selectedCardInitialPosition;
                    clicked.tag = myCardTag;
                    clicked = null;
                }
            }
        }
        // Keep card on your mouse
        if (clicked != null)
        {
            clicked.transform.position = mousePos2D;
        }
    }

    // Update your local board and the server board with the used cards
    private void SyncBoardUsedCards()
    {
        // Find the card and remove it locally
        for (int i = 0; i < Static.board.Length; i++)
        {
            if (Static.board[i] == lastCardInBoardUsed)
            {
                Static.board[i] = "";
                break;
            }
        }

        // Update the server board
        Dictionary<string, string> data = new Dictionary<string, string>();
        for (int i = 0; i < Static.board.Length; i++)
        {
            data["board" + i] = Static.board[i];
        }
        data["privateKey"] = Static.privateKey;
        socket.Emit("game:save-board", new JSONObject(data));
    }

    // When a card is placed
    private void EmitPlaced(string cardType)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["roomId"] = Static.roomId;
        data["cardType"] = cardType;
        data["sender"] = Static.userAddress;
        data["privateKey"] = Static.privateKey;
        socket.Emit("game:card-placed", new JSONObject(data));
    }

    private void SocketEvents()
    {
        socket.On("game:round:draw", (SocketIOEvent res) =>
        {
            Debug.Log("Received draw event");
            currentRound++;
            currentRoundVisual.text = currentRound.ToString();
            SyncBoardUsedCards();
            string resText = res.data.ToString();
            JSONNode parsed = JSON.Parse(resText);
            DestroyEnemyUsedCard("Draw", placedCard, parsed);
            UpdateVisualGlobalCardsCounter(parsed);
        });

        socket.On("game:round:winner-one", (SocketIOEvent res) =>
        {
            Debug.Log("Received winner one");
            currentRound++;
            currentRoundVisual.text = currentRound.ToString();
            SyncBoardUsedCards();
            MoveStarsRound(isPlayerOne);
            string resText = res.data.ToString();
            JSONNode parsed = JSON.Parse(resText);
            DestroyEnemyUsedCard("Round winner player one", placedCard, parsed);
            UpdateVisualGlobalCardsCounter(parsed);
        });

        socket.On("game:round:winner-two", (SocketIOEvent res) =>
        {
            Debug.Log("Received winner two");
            currentRound++;
            currentRoundVisual.text = currentRound.ToString();
            SyncBoardUsedCards();
            MoveStarsRound(!isPlayerOne);
            string resText = res.data.ToString();
            JSONNode parsed = JSON.Parse(resText);
            DestroyEnemyUsedCard("Round winner player two", placedCard, parsed);
            UpdateVisualGlobalCardsCounter(parsed);
        });

        socket.On("game:finish:winner-player-two", (SocketIOEvent e) =>
        {
            SyncBoardUsedCards();
            if (Static.userId == Static.playerOne) {
                SceneManager.LoadScene("Loss");
            }
            else
            {
                SceneManager.LoadScene("Win");
            }
        });

        socket.On("game:finish:winner-player-one", (SocketIOEvent e) =>
        {
            SyncBoardUsedCards();
            if (Static.userId == Static.playerOne)
            {
                SceneManager.LoadScene("Win");
            }
            else
            {
                SceneManager.LoadScene("Loss");
            }
        });

        socket.On("game:finish:draw", (SocketIOEvent e) =>
        {
            SyncBoardUsedCards();
            SceneManager.LoadScene("Draw");
        });

        socket.On("issue", (SocketIOEvent e) =>
        {
            Debug.Log("Error socket " + e.data.GetField("msg").str);
            errorText.text = e.data.GetField("msg").str;
        });
    }

    private void DestroyEnemyUsedCard(string notif, GameObject myCard, JSONNode parsed)
    {
        // Start the counter at the start of each round (not counting the first)
        StopCoroutine("Timer");
        StartCoroutine("Timer");

        string selectedEnemyCard = parsed["playerTwoActive"].Value;
        GameObject enemyCardsContainer = GameObject.Find("EnemyCards");
        foreach (Transform child in enemyCardsContainer.transform)
        {
            // If selectedEnemyCard == GameObject.Find("EnemyCards")
            if (child.name == selectedEnemyCard)
            {
                child.gameObject.SetActive(true);
                StartCoroutine(MoveObject(notif, myCard, child.transform, GameObject.Find("EnemyPlace").transform.position, .3f));
                break;
            }
        }
    }

    private void MoveStarsRound(bool isPlayerOneWinner)
    {
        if (isPlayerOneWinner)
        {
            foreach (GameObject star in allyStars)
            {
                if (!star.activeSelf)
                {
                    star.SetActive(true);
                    break;
                }
            }
            foreach (GameObject star in enemyStars)
            {
                if (star.activeSelf)
                {
                    star.SetActive(false);
                    break;
                }
            }
        }
        else
        {
            foreach (GameObject star in allyStars)
            {
                if (star.activeSelf)
                {
                    star.SetActive(false);
                    break;
                }
            }
            foreach (GameObject star in enemyStars)
            {
                if (!star.activeSelf)
                {
                    star.SetActive(true);
                    break;
                }
            }
        }
    }

    // notif, the notification message
    // myCard, the card I placed
    // item, the enemy card to move
    // target, the position to place the card
    // overTime, the time to animate
    IEnumerator MoveObject(string notif, GameObject myCard, Transform item, Vector3 target, float overTime)
    {
        float startTime = Time.time;
        // Attention, the start position must be separated to avoid the lerp from
        // going too fast. Don't do item.position = Vector3.Lerp(item.position...)
        // since the start will accelerate too fast, keep an external variable
        // for the first parameter like Vector3 position and use that
        Vector3 startPosition = item.position;
        startPosition.z = 0;
        target.z = 0;
        while (Time.time < startTime + overTime)
        {
            float pos = (Time.time - startTime) / overTime;
            item.position = Vector3.Lerp(startPosition, target, pos);
            yield return null;
        }
        notification.text = notif;
        item.position = target;
        yield return new WaitForSeconds(1);
        Destroy(item.gameObject);
        Destroy(myCard);
        notification.text = "";
    }

    IEnumerator Timer()
    {
        // Increate the timer text to get attention
        moveTimerText.fontSize = 40;
        float initialTime = Time.time;
        float timeSinceMove = Time.time - initialTime;
        while (int.Parse(Static.moveTimer) > (int)timeSinceMove)
        {
            timeSinceMove = Time.time - initialTime;
            int remainingTime = int.Parse(Static.moveTimer) - (int)timeSinceMove;
            Debug.Log("Remaining time " + remainingTime);
            moveTimerText.text = remainingTime + " seconds";

            yield return new WaitForSeconds(1);
        }
        moveTimerText.fontSize = 30;
    }

    void UpdateVisualGlobalCardsCounter(JSONNode parsed)
    {
        string rocks = parsed["rocks"].Value;
        string papers = parsed["papers"].Value;
        string scissors = parsed["scissors"].Value;

        Static.globalRocks = rocks;
        Static.globalScissors = scissors;
        Static.globalPapers = papers;
        globalRocks.text = rocks;
        globalPapers.text = papers;
        globalScissors.text = scissors;
    }
}
