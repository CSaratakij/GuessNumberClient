using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SocketIO;

public class GameSessionUI : MonoBehaviour
{
    SocketIOComponent socket;

    [SerializeField]
    GameClient game;

    [SerializeField]
    Text txtChatHistory;

    [SerializeField]
    TMP_InputField inputMessage;

    [SerializeField]
    ScrollRect scrollRect;

    [SerializeField]
    RectTransform panelPlayer;

    [SerializeField]
    GameObject prefabButton;

    [SerializeField]
    TextMeshProUGUI lblTotalPlayer;

    void Awake()
    {
        txtChatHistory.text = string.Empty;
    }

    void Start()
    {
        socket = GameClient.socket;
    }

    public void TryToSubmitText()
    {
        string messageFormat = "{0} : {1}";

        if (string.IsNullOrEmpty(inputMessage.text)) {
            return;
        }

        int number;
        bool isCanParse = int.TryParse(inputMessage.text, out number);

        if (isCanParse)
        {
            //send number instead...
            string message = string.Format(messageFormat, GameClient.PlayerName, inputMessage.text);

            JSONObject jSONObject = new JSONObject(JSONObject.Type.OBJECT);

            jSONObject.AddField("owner", GameClient.PlayerName);
            jSONObject.AddField("number", number);

            socket.Emit("message.number", jSONObject);
            Debug.Log("send number...");
        }
        else
        {
            string message = string.Format(messageFormat, GameClient.PlayerName, inputMessage.text);

            JSONObject jSONObject = new JSONObject(JSONObject.Type.OBJECT);
            jSONObject.AddField("message", message);

            socket.Emit("message.sent", jSONObject);
            Debug.Log("send");
        }

        inputMessage.text = string.Empty;
    }

    public void AddChatHistory(string message)
    {
        string result = (txtChatHistory.text + message + Environment.NewLine);
        txtChatHistory.text = result;

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0.0f;
    }

    public void ResetData()
    {
        txtChatHistory.text = string.Empty;

        for (int i = 0; i < panelPlayer.childCount; ++i)
        {
            Destroy(panelPlayer.GetChild(i).gameObject);
        }
    }

    public void UpdatePlayerList(List<string> list)
    {
        for (int i = 0; i < panelPlayer.childCount; ++i)
        {
            Destroy(panelPlayer.GetChild(i).gameObject);
        }

        lblTotalPlayer.text = string.Format("Player: {0}", list.Count);

        foreach (var name in list)
        {
            GameObject obj = Instantiate(prefabButton, Vector3.zero, Quaternion.identity);
            obj.transform.SetParent(panelPlayer, false);

            Text lblPlayer = obj.transform.GetChild(0).GetComponent<Text>();
            lblPlayer.text = name;
        }
    }

    public void UpdateLeaderBoard(List<string> list)
    {
//TODO
    }
}
