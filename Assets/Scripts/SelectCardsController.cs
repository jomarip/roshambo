using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using SimpleJSON;
using SocketIO;

public class SelectCardsController : MonoBehaviour
{
    private GameObject clicked; // The selected element
    public TextMeshProUGUI counterRocks;
    public TextMeshProUGUI counterPapers;
    public TextMeshProUGUI counterScissors;
    private readonly SocketIOComponent socket = SocketManager.socket;
    public GameObject rockCard;
    public GameObject paperCard;
    public GameObject scissorCard;
    public Button saveButton;
    public GraphicRaycaster gr;
    public GameObject[] placeholders;
    private Vector3[] placeholderInitialPositions = new Vector3[9];
    private string selectedCardPosition;
    public TextMeshProUGUI errorMessage;
    public TextMeshProUGUI successMessage;

    // Start is called before the first frame update
    void Start()
    {
        saveButton.onClick.AddListener(SaveCards);
        // Code to be place in a MonoBehaviour with a GraphicRaycaster component
        gr = GetComponent<GraphicRaycaster>();
        // Create the PointerEventData with null for the EventSystem
        for (int i = 0; i < placeholders.Length; i++)
        {
            placeholderInitialPositions[i] = placeholders[i].transform.position;
        }
        SocketEvents();
        GetMyCards();
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
            int allyCardLayer = LayerMask.NameToLayer("Ally-card");
            int placeholderLayer = LayerMask.NameToLayer("Enemy-card");

            if (Input.GetMouseButtonDown(0))
            {
                // Place a card on the placeholder
                if (clicked != null && clickedBelow.layer == placeholderLayer)
                {
                    // Place it on the active area
                    PlaceCardOnPlaceholder(clicked, clickedBelow);
                    // If you selected a placed card previously and moved it
                    if (selectedCardPosition != null)
                    {
                        UpdateBoardArray(false, clicked.name, selectedCardPosition);
                        selectedCardPosition = null;
                    }
                    UpdateBoardArray(true, clicked.name, clickedBelow.name);
                    clicked = null;
                }
                // First click, clone placed cards
                else if (!clicked && currentClicked.tag != "Placed" && currentClicked.layer == allyCardLayer)
                {
                    GameObject instance = null;

                    // Check if you have zero or less cards available for that type
                    bool shouldAllow = UpdateSelectedCardNumbers(currentClicked.name, -1);
                    if (!shouldAllow) return;
                    switch (currentClicked.name)
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
                    instance.tag = "Placed";
                    Transform clonesContainer = GameObject.Find("ClonesContainer").transform;
                    instance.transform.SetParent(clonesContainer);
                    clicked = instance;
                }
                // Click on placed card
                else if (!clicked && currentClicked.tag == "Placed" && currentClicked.layer == allyCardLayer)
                {
                    clicked = currentClicked;
                    selectedCardPosition = clickedBelow.name;
                }
                // If clicked nothing, destroy the card instance and increase the
                // proper counter on the left
                else if (clicked != null)
                {
                    UpdateSelectedCardNumbers(clicked.name, 1);
                    UpdateBoardArray(false, clicked.name, selectedCardPosition);
                    selectedCardPosition = null;
                    Destroy(clicked);
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

    private void SocketEvents()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["privateKey"] = Static.privateKey;
        socket.Emit("game:get-board", new JSONObject(data));

        socket.On("game:board", (SocketIOEvent res) =>
        {
            Debug.Log("Received board event");
            PlaceBoard(res);
        });

        socket.On("game:save-board-complete", (SocketIOEvent e) =>
        {
            Debug.Log("Saved board successfully!");
            successMessage.text = "Board saved successfully";
        });

        socket.On("issue", (SocketIOEvent e) =>
        {
            Debug.Log("Error socket " + e.data.GetField("msg").str);
            errorMessage.text = e.data.GetField("msg").str;
        });
    }

    private void CleanMessages()
    {
        errorMessage.text = "";
        successMessage.text = "";
    }

    // isAdded Whether this card is added or removed
    // cardType Which card is placed
    // locationName Where
    private void UpdateBoardArray(bool isAdded, string cardType, string locationName)
    {
        int position = 0;
        switch (locationName)
        {
            case "one":
                position = 0;
                break;
            case "two":
                position = 1;
                break;
            case "three":
                position = 2;
                break;
            case "four":
                position = 3;
                break;
            case "five":
                position = 4;
                break;
            case "six":
                position = 5;
                break;
            case "seven":
                position = 6;
                break;
            case "eight":
                position = 7;
                break;
            case "nine":
                position = 8;
                break;
        }

        if (isAdded)
        {
            cardType = cardType.Replace("(Clone)", "");
            Static.board[position] = cardType;
        } else
        {
            Array.Clear(Static.board, position, 1);
        }
    }

    private void SaveCards()
    {
        Debug.Log("Clicked on save");
        CleanMessages();
        Dictionary<string, string> data = new Dictionary<string, string>();
        // Check if it's empty or not and set the variables accordingly
        foreach(string item in Static.board)
        {
            if (!string.IsNullOrEmpty(item))
            {
                Static.isBoardEmpty = false;
                break;
            }
        }
        for(int i = 0; i < Static.board.Length; i++)
        {
            data["board"+i] = Static.board[i];
        }
        data["privateKey"] = Static.privateKey;
        socket.Emit("game:save-board", new JSONObject(data));
    }

    private void PlaceCardOnPlaceholder(GameObject card, GameObject placeholder)
    {
        switch (placeholder.name)
        {
            case "one":
                clicked.transform.position = placeholderInitialPositions[0];
                break;
            case "two":
                clicked.transform.position = placeholderInitialPositions[1];
                break;
            case "three":
                clicked.transform.position = placeholderInitialPositions[2];
                break;
            case "four":
                clicked.transform.position = placeholderInitialPositions[3];
                break;
            case "five":
                clicked.transform.position = placeholderInitialPositions[4];
                break;
            case "six":
                clicked.transform.position = placeholderInitialPositions[5];
                break;
            case "seven":
                clicked.transform.position = placeholderInitialPositions[6];
                break;
            case "eight":
                clicked.transform.position = placeholderInitialPositions[7];
                break;
            case "nine":
                clicked.transform.position = placeholderInitialPositions[8];
                break;
        }
    }

    private void GetMyCards()
    {
        counterRocks.text = Static.myRocks;
        counterPapers.text = Static.myPapers;
        counterScissors.text = Static.myScissors;
    }

    private void PlaceBoard(SocketIOEvent res)
    {
        string resText = res.data.ToString();
        JSONNode parsed = JSON.Parse(resText);
        int a = 0;
        foreach (JSONNode item in parsed["data"])
        {
            Static.board[a] = item.Value;
            if (!string.IsNullOrEmpty(Static.board[a]))
            {
                GameObject instance = null;
                switch (Static.board[a])
                {
                    case "Paper":
                        instance = Instantiate(paperCard);
                        instance.name = "Paper";
                        counterPapers.text = (int.Parse(counterPapers.text) - 1).ToString();
                        break;
                    case "Rock":
                        instance = Instantiate(rockCard);
                        instance.name = "Rock";
                        counterRocks.text = (int.Parse(counterRocks.text) - 1).ToString();
                        break;
                    case "Scissors":
                        instance = Instantiate(scissorCard);
                        instance.name = "Scissors";
                        counterScissors.text = (int.Parse(counterScissors.text) - 1).ToString();
                        break;
                }
                instance.tag = "Placed";
                Transform clonesContainer = GameObject.Find("ClonesContainer").transform;
                instance.transform.SetParent(clonesContainer);
                // Place a new instance in the right placeholder
                instance.transform.position = placeholders[a].transform.position;
            }
            a++;
        }

        //for(int i = 0; i < Static.board.Length; i++)
        //{
        //    Debug.Log("Board[" + i + "] = " + Static.board[i]);
        //}
        //Debug.Log("Board");
        //Debug.Log(Static.board);
    }

    // Returns whether you can pick the card or not (to block 0 card selections)
    // string cardType the card type name
    // int quantity whether to add that card or reduce it from the counter
    bool UpdateSelectedCardNumbers(string cardType, int quantity)
    {
        Debug.Log("CALLED " + cardType + quantity);
        bool allowMovement = true;
        int newValue = 0;
        switch (cardType)
        {
            case "Paper":
                newValue = int.Parse(counterPapers.text) + quantity;
                Debug.Log(newValue);
                if (newValue < 0)
                {
                    allowMovement = false;
                    break;
                }
                counterPapers.text = newValue.ToString();
                break;
            case "Rock":
                newValue = int.Parse(counterRocks.text) + quantity;
                Debug.Log(newValue);
                if (newValue < 0)
                {
                    allowMovement = false;
                    break;
                }
                counterRocks.text = newValue.ToString();
                break;
            case "Scissors":
                newValue = int.Parse(counterScissors.text) + quantity;
                Debug.Log(newValue);
                if (newValue < 0)
                {
                    allowMovement = false;
                    break;
                }
                counterScissors.text = newValue.ToString();
                break;
        }
        return allowMovement;
    }
}
