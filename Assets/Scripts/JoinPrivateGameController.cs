using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocketIO;
using SimpleJSON;
using UnityEngine.SceneManagement;

public class JoinPrivateGameController : MonoBehaviour
{
    public InputField gameCode;
    public Button sendGameButton;
    private SocketIOComponent socket = SocketManager.socket;
    public Text errorMessage;

    // Start is called before the first frame update
    void Start()
    {
        sendGameButton.onClick.AddListener(JoinGame);
        SocketEvents();
    }

    private void JoinGame()
    {
        ShowError("");
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["gameId"] = gameCode.text;

        socket.Emit("game:join-private-game", new JSONObject(data));
    }

    private void SocketEvents()
    {
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

            // Reset data to not allow it to join again
            Static.qrImage = null;
            Static.qrData = null;
            Static.privateGameId = null;

            // Move them to the right scene with the Static data
            SceneManager.LoadScene("Gameplay");
        });

        socket.On("issue", (SocketIOEvent e) =>
        {
            Debug.Log("Error socket " + e.data.GetField("msg").str);
            ShowError(e.data.GetField("msg").str);
        });
    }

    private void ShowError(string msg)
    {
        errorMessage.text = msg;
    }
}
