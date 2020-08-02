using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using SimpleJSON;
using SocketIO;
using System.Collections.Generic;

public class LoginManager : MonoBehaviour
{
    public InputField email;
    public InputField password;
    public Button loginButton;
    public Text errorMessage;
    public Text successMessage;
    public GameObject spinner;
    private SocketIOComponent socket = SocketManager.socket;

    // Use this for initialization
    void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
    }

    private void OnLoginClicked()
    {
        // Reset the error message
        ShowError("");
        ShowSuccess("");
        // Check that the email and passwords aren't empty
        if (email.text.Length <= 0)
        {
            ShowError("The email can't be empty");
            return;
        }
        if (password.text.Length <= 0)
        {
            ShowError("The password can't be empty");
            return;
        }

        ToggleSpinner(true);
        Login();
    }

    private void Login()
    {
        ShowError("");
        ShowSuccess("");
        ToggleSpinner(true);
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["email"] = email.text;
        data["password"] = password.text;
        socket.Emit("setup:login", new JSONObject(data));

        socket.On("setup:login-complete", (SocketIOEvent e) =>
        {
            string a = e.data.GetField("response").ToString();
            JSONNode res = JSON.Parse(a);
            Static.userId = res["userId"].Value;
            Static.userAddress = res["userAddress"].Value;
            Static.balance = res["balance"].AsDouble / 1e6;
            Static.privateKey = res["privateKey"].Value;
            ShowSuccess(res["msg"].Value);
            ToggleSpinner(false);
            StartCoroutine(LoadScene());
        });

        socket.On("issue", (SocketIOEvent e) =>
        {
            ShowError(e.data.GetField("msg").str);
            ToggleSpinner(false);
        });
    }

    void ShowError(string msg)
    {
        this.errorMessage.text = msg;
    }

    void ShowSuccess(string msg)
    {
        this.successMessage.text = msg;
    }

    void ToggleSpinner(bool isDisplayed)
    {
        loginButton.gameObject.SetActive(!isDisplayed);
        spinner.SetActive(isDisplayed);
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(Static.timeAfterAction);
        SceneManager.LoadScene("Game");
        yield break;
    }
}
