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
    GameUI ui;

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

    [Header("LeaderBoard")]
    [SerializeField]
    RectTransform panelLeaderBoard;

    [SerializeField]
    GameObject prefabLeaderBoardButton;

    [SerializeField]
    Button btnClose;

    enum LabelIndex
    {
        Rank,
        Name,
        TryCount
    }

    void Awake()
    {
        txtChatHistory.text = string.Empty;
        btnClose.onClick.AddListener(() => {
            ui.Show(GameUI.View.GameSessionView);
        });
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

    public void UpdateLeaderBoard(List<GameClient.Score> list)
    {
        for (int i = 0; i < panelLeaderBoard.childCount; ++i)
        {
            Destroy(panelLeaderBoard.GetChild(i).gameObject);
        }

        string rankFormat = "#{0}";
        string tryCountFormat = "try: {0}";

        for (int i = 0; i < list.Count; ++i)
        {
            var score = list[i];

            GameObject obj = Instantiate(prefabLeaderBoardButton, Vector3.zero, Quaternion.identity);
            obj.transform.SetParent(panelLeaderBoard, false);

            Text lblRank = obj.transform.GetChild((int)LabelIndex.Rank).GetComponent<Text>();
            Text lblName = obj.transform.GetChild((int)LabelIndex.Name).GetComponent<Text>();
            Text lblTryCount = obj.transform.GetChild((int)LabelIndex.TryCount).GetComponent<Text>();

            lblRank.text = string.Format(rankFormat, i + 1);
            lblName.text = score.name;
            lblTryCount.text = string.Format(tryCountFormat, score.tryCount);
        }
    } 
}