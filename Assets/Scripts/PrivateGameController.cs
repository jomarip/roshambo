using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SocketIO;
using SimpleJSON;
using UnityEngine.SceneManagement;

public class PrivateGameController : MonoBehaviour
{
    public RawImage qrContainer;
    public TextMeshProUGUI gameCode;
    public GameObject initialMessage;
    public GameObject privateGameContainer;
    public Button deleteGameButton;
    public Text errorMessage;
    private SocketIOComponent socket = SocketManager.socket;

    // Start is called before the first frame update
    void Start()
    {
        // Display the created QR code if any
        if (Static.qrImage)
        {
            ToggleData(true);
        }
        SocketEvents();
    }

    public void DeleteGame()
    {
        socket.Emit("game:delete");
        ToggleData(false);
        Static.qrImage = null;
        Static.qrData = null;
    }

    public void ToggleData(bool on)
    {
        ShowError("");
        privateGameContainer.SetActive(on);
        initialMessage.SetActive(!on);
        qrContainer.texture = Static.qrImage;
        gameCode.text = Static.privateGameId;
        deleteGameButton.onClick.AddListener(DeleteGame);
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
            ToggleData(false);
        });
    }

    private void ShowError(string msg)
    {
        errorMessage.text = msg;
    }
}
