using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using SocketIO;

public class Static : MonoBehaviour
{
    public static int timeAfterAction = 1;
    public static string userId;
    public static string userAddress;
    public static string mnemonic;
    public static string privateKey;
    public static double balance;
    public static string serverUrl = "http://3.137.51.91";
    //public static string serverUrl = "http://localhost:80";

    // Game related data
    public static string roomId;
    public static string playerOne;
    public static string playerTwo;
    public static string gameName;
    public static string gameType;
    public static string rounds;
    public static string moveTimer;
    public static string globalRocks;
    public static string globalScissors;
    public static string globalPapers;
    //public static string[] board = new string[9];
    public static string[] board = {"", "", "",
                                    "","","",
                                    "", "", ""};
    public static bool isBoardEmpty = true;
    public static string[] boardOne;
    public static string[] boardTwo;

    // Private game
    public static Texture2D qrImage;
    public static string qrData;
    public static string privateGameId;
    public static string myRocks;
    public static string myPapers;
    public static string myScissors;
}