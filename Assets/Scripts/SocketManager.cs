using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;

public class SocketManager : MonoBehaviour
{
    public static SocketIOComponent socket;

    // Start is called before the first frame update
    void Start()
    {
        if (!socket)
        {
            socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
            DontDestroyOnLoad(socket);
            socket.Connect();
        }
    }
}
