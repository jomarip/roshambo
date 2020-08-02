using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SimpleJSON;
using SocketIO;
using System;

public class LoginWithCryptoManager : MonoBehaviour
{
    public InputField privateKeyInput;
    public Button loginButton;
    public Text errorMessage;
    public Text successMessage;
    public GameObject spinner;
    private SocketIOComponent socket = SocketManager.socket;

    // Start is called before the first frame update
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
        if (privateKeyInput.text.Length <= 0)
        {
            ShowError("The private key can't be empty");
            return;
        }

        if (privateKeyInput.text.Length != 64)
        {
            ShowError("The private key is invalid");
            return;
        }

        LoginWithCrypto();
    }

    private void LoginWithCrypto()
    {
        ShowError("");
        ShowSuccess("");
        ToggleSpinner(true);
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["privateKey"] = privateKeyInput.text;
        Static.privateKey = privateKeyInput.text;
        socket.Emit("setup:login-with-crypto-private-key", new JSONObject(data));

        socket.On("setup:login-complete", (SocketIOEvent e) =>
        {
            string a = e.data.GetField("response").ToString();
            JSONNode res = JSON.Parse(a);
            Static.userId = res["userId"].Value;
            Static.userAddress = res["userAddress"].Value;
            Static.balance = res["balance"].AsDouble / 1e6;
            Static.privateKey = res["privateKey"].Value;

            Debug.Log("Private key " + Static.privateKey);

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
        if (isDisplayed)
        {
            loginButton.gameObject.SetActive(false);
            spinner.SetActive(true);
        }
        else
        {
            loginButton.gameObject.SetActive(true);
            spinner.SetActive(false);
        }
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(Static.timeAfterAction);
        SceneManager.LoadScene("Game");
        yield break;
    }
}
