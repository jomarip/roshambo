using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using SimpleJSON;
using System;

public class InitialGameInterfaceController : MonoBehaviour
{
    private readonly SocketIOComponent socket = SocketManager.socket;

    // Start is called before the first frame update
    void Start()
    {
        SocketEvents();
    }

    void SocketEvents()
    {
        GetCards();
        GetBoard();
        
        socket.On("game:board", (SocketIOEvent res) =>
        {
            Debug.Log("Received board event");
            string resText = res.data.ToString();
            JSONNode parsed = JSON.Parse(resText);
            int a = 0;
            foreach (JSONNode item in parsed["data"])
            {
                Static.board[a] = item.Value;
                if (Static.isBoardEmpty && !string.IsNullOrEmpty(Static.board[a])) {
                    Static.isBoardEmpty = false;
                }
            }
        });

        socket.On("tron:get-my-cards", (SocketIOEvent res) =>
        {
            string msg = res.data.GetField("data").ToString();
            JSONNode cards = JSON.Parse(msg);
            Static.myRocks = cards[0].Count.ToString();
            Static.myPapers = cards[1].Count.ToString();
            Static.myScissors = cards[2].Count.ToString();
        });
    }

    void GetBoard()
    {
        // Get the board data and store it into the static to decide if the user
        // has the board ready for playing games or not
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["privateKey"] = Static.privateKey;
        socket.Emit("game:get-board", new JSONObject(data));
        Debug.Log("Getting board...");
    }

    void GetCards()
    {
        // Get cards on load
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["account"] = Static.userAddress;
        data["privateKey"] = Static.privateKey;
        socket.Emit("tron:get-my-cards", new JSONObject(data));
    }
}
