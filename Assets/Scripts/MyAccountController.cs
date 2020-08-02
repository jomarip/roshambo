using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SocketIO;
using UnityEngine.UI;
using SimpleJSON;

public class MyAccountController : MonoBehaviour
{
    private SocketIOComponent socket = SocketManager.socket;
    public TextMeshProUGUI rocks;
    public TextMeshProUGUI papers;
    public TextMeshProUGUI scissors;
    public TextMeshProUGUI errorMessage;
    public TextMeshProUGUI successMessage;
    public InputField buyCardsQuantity;
    public Button buyCardsButton;
    public GameObject spinner;

    // Start is called before the first frame update
    void Start()
    {
        ToggleSpinner(false);
        SocketEvents();
        buyCardsButton.onClick.AddListener(BuyCards);
    }

    void BuyCards()
    {
        ToggleSpinner(true);
        ShowError("");
        ShowSuccess("");
        string cardsToBuy = buyCardsQuantity.text;
        if (cardsToBuy.Length == 0 || cardsToBuy == "0")
        {
            ShowError("You need to specify the quantity");
            return;
        }
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["cardsToBuy"] = cardsToBuy;
        data["account"] = Static.userAddress;
        data["privateKey"] = Static.privateKey;
        socket.Emit("tron:buy-cards", new JSONObject(data));
    }

    void GetCards()
    {
        // Get cards on load
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["account"] = Static.userAddress;
        data["privateKey"] = Static.privateKey;
        socket.Emit("tron:get-my-cards", new JSONObject(data));
    }

    void SocketEvents()
    {
        GetCards();
        socket.On("issue", (SocketIOEvent e) =>
        {
            Debug.Log("Error socket " + e.data.GetField("msg").str);
            ShowError(e.data.GetField("msg").str);
            ToggleSpinner(false);
        });
        socket.On("tron:buy-cards-complete", (SocketIOEvent e) =>
        {
            ShowSuccess("Cards purchased successfully!");
            ToggleSpinner(false);
            buyCardsQuantity.text = "";
            StartCoroutine(GetCardsAfterSeconds());
        });
        socket.On("tron:get-my-cards", (SocketIOEvent res) =>
        {
            string msg = res.data.GetField("data").ToString();
            JSONNode parsed = JSON.Parse(msg);
            DisplayCards(parsed);
        });
    }

    IEnumerator GetCardsAfterSeconds()
    {
        yield return new WaitForSeconds(2);
        GetCards();
        yield return null;
    }

    void DisplayCards(JSONNode cards)
    {
        rocks.text = cards[0].Count.ToString();
        papers.text = cards[1].Count.ToString();
        scissors.text = cards[2].Count.ToString();

        Static.myRocks = cards[0].Count.ToString();
        Static.myPapers = cards[1].Count.ToString();
        Static.myScissors = cards[2].Count.ToString();
    }

    void ShowError(string msg)
    {
        errorMessage.text = msg;
    }

    void ShowSuccess(string msg)
    {
        successMessage.text = msg;
    }

    void ToggleSpinner(bool isDisplayed)
    {
        spinner.SetActive(isDisplayed);
    }
}
