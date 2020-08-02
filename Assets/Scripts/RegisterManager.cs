using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using SimpleJSON;
using SocketIO;

public class RegisterManager : MonoBehaviour
{
    public InputField email;
    public InputField password;
    public InputField passwordRepeat;
    public InputField username;
    public Button registerButton;
    public Text errorMessage;
    public Text successMessage;
    public GameObject spinner;
    private SocketIOComponent socket = SocketManager.socket;

    // Start is called before the first frame update
    void Start()
    {
        registerButton.onClick.AddListener(RegisterUser);
        ToggleSpinner(false); // Make sure the spinner is hidden when starting the scene
    }

    void RegisterUser()
    {
        // Reset the error message
        ShowError("");
        ShowSuccess("");
        string regexEmail =
            @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
            + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
              + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
            + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";
        if (email.text.Length <= 0)
        {
            ShowError("The email can't be empty");
            return;
        }
        if (password.text.Length <= 0 || passwordRepeat.text.Length <= 0)
        {
            ShowError("The password can't be empty");
            return;
        }
        if (username.text.Length <= 0)
        {
            ShowError("The username can't be empty");
            return;
        }
        if (!Regex.IsMatch(email.text, regexEmail))
        {
            ShowError("The email isn't valid");
            return;
        }
        if (password.text != passwordRepeat.text)
        {
            ShowError("The passwords don't match");
            return;
        }

        ToggleSpinner(true);
        SendRegisterUser();
    }

    private void SendRegisterUser()
    {
        ShowError("");
        ShowSuccess("");
        ToggleSpinner(true);
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["email"] = email.text;
        data["password"] = password.text;
        data["username"] = username.text;

        socket.Emit("setup:register", new JSONObject(data));

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
        registerButton.gameObject.SetActive(!isDisplayed);
        spinner.SetActive(isDisplayed);
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(Static.timeAfterAction);
        SceneManager.LoadScene("Game");
        yield break;
    }
}
