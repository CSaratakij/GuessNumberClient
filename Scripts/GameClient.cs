using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;

[RequireComponent(typeof(SocketIOComponent))]
public class GameClient : MonoBehaviour
{
    static SocketIOComponent socket = null;

    void Awake()
    {
        MakeSingleton();
    }

    void Start()
    {
        SubscribeEvents();
    }

    void MakeSingleton()
    {
        if (socket == null)
        {
            socket = GetComponent<SocketIOComponent>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SubscribeEvents()
    {
        socket.On("open", (e) => {
            Debug.Log("Connnected...");
        });

        socket.On("message.sent", (e) => {
            string msg = e.data["message"].str;
            Debug.Log(msg);
        });

        socket.On("disconnect", (e) => {
            Debug.Log("Disconnect...");
        });
    }

    public void btnConnect_OnClick()
    {
        socket?.Connect();

        JSONObject jSONObject = new JSONObject(JSONObject.Type.OBJECT);

        jSONObject.AddField("message", "Hi..");
        socket.Emit("message.send", jSONObject);
        Debug.Log("send");
    }
}
