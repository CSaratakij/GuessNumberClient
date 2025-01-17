﻿using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SocketIO;

[RequireComponent(typeof(SocketIOComponent))]
public class GameClient : MonoBehaviour
{
    const int MAX_SCORE = 1000;
    static readonly string WindowTitleFormat = "{0}:({1})";

    #if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
    [DllImport("MyX11Plugin.so")]
    public static extern void SetWindowTitle(string name);
    #endif


    bool IsGameStart = false;

    public static SocketIOComponent socket = null;
    public static string PlayerName = "";

    [SerializeField]
    TMP_InputField playerNameField;

    [SerializeField]
    TMP_InputField ipInputField;

    [SerializeField]
    TMP_InputField portInputField;
    
    [SerializeField]
    GameUI ui;

    [SerializeField]
    GameSessionUI gameSessionUI;

    public int CurrentNumber => number;
    public int CurrentScore => score;

    public List<string> PlayerList => playerList;
    public List<Score> LeaderBoardList => leaderBoardList;

    int number = 0;
    int score = MAX_SCORE;

    List<string> playerList;
    List<Score> leaderBoardList;

    public struct Score
    {
        public string name;
        public int tryCount;

        public Score(string name, int tryCount)
        {
            this.name = name;
            this.tryCount = tryCount;
        }
    }

    void Awake()
    {
        Initialize();
        MakeSingleton();
    }

    void Start()
    {
        SubscribeEvents();
    }

    void LateUpdate()
    {
        if (Time.frameCount % 3 == 0)
        {
            GameSessionViewHandler();
        }
    }

    void Initialize()
    {
        playerList = new List<string>();
        leaderBoardList = new List<Score>();
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
            ui.Show(GameUI.View.GameSessionView);
            Debug.Log("Connnected...");
        });

        socket.On("connect", (e) => {
            ui.Show(GameUI.View.GameSessionView);
            Debug.Log("Connnected...");
            // RequestPlayersList();
            TryCreateNewPlayer();
            // RequestAddPlayer(GameClient.PlayerName);
            // RequestCurrentScore(GameClient.PlayerName);
        });

        socket.On("game.respondJoinedPlayers", (e) => {
            Debug.Log("Get player info...");
            JSONObject json = e.data;

            playerList.Clear();

            foreach (var key in json.keys)
            {
                var value = json[key];
                playerList.Add(value.str);
            }

            gameSessionUI.UpdatePlayerList(playerList);
        });

        socket.On("game.respondLeaderBoard", (e) => {
            List<JSONObject> info = e.data["info"].list;
            leaderBoardList.Clear();

            foreach (var obj in info)
            {
                leaderBoardList.Add(new Score(
                    obj["name"].str,
                    Convert.ToInt32(obj["tryCount"].n)
                ));
            }

            gameSessionUI.UpdateLeaderBoard(leaderBoardList);
            ui.Show(GameUI.View.LeaderBoardView);

            Debug.Log("Show leader board...");
        });

        socket.On("game.respondScore", (e) => {
            try
            {
                if (e.data.HasField("tryCount"))
                {
                    score = Mathf.Abs((int)e.data["tryCount"].n);
                    Debug.Log("Current Score : " + score);
                }
                else
                {
                    score = MAX_SCORE;
                }
            }
            catch (Exception ex)
            {
                score = MAX_SCORE;
                Debug.Log("What the fuck happen in score...?");
            }
        });

        socket.On("game.respondToStart", (e) => {
            Debug.Log("Game can start....");
            try
            {
                JSONObject jSON = e.data["chooseNumber"];
                string strNumber = jSON.ToString();

                number = (int)Convert.ToDecimal(strNumber);
                Debug.Log("Get number : " + number);

                ui.Show(GameUI.View.InGameView);
            }
            catch (Exception ex)
            {
                Debug.Log("Fuck...");
            }
        });

        socket.On("message.send", (e) => {
            string msg = e.data["message"].str;
            Debug.Log(msg);
            gameSessionUI.AddChatHistory(msg);
        });

        socket.On("disconnect", (e) => {
            ui.Show(GameUI.View.ConnectView);
            Debug.Log("Disconnect...");
        });

        socket.On("game.changePlayState", (e) => {
            IsGameStart = (e.data["isStart"].str == "1") ? true : false;
        });

        socket.On("game.hasWinner", (e) => {
            gameSessionUI.AddChatHistory("Winner is : " + e.data["winner"]);
        });
    }

    void GameSessionViewHandler()
    {
        bool shouldOpenGameSession = (socket.IsConnected && ui.CurrentView == GameUI.View.ConnectView);

        if (shouldOpenGameSession)
        {
            ui.Show(GameUI.View.GameSessionView);
        }
    }

    void RequestPlayersList()
    {
        // JSONObject jSONObject = new JSONObject(JSONObject.Type.OBJECT);
        socket?.Emit("game.requestJoinedPlayers");
    }

    public void RequestCurrentScore(string name)
    {
        JSONObject jSONObject = new JSONObject(JSONObject.Type.OBJECT);
        jSONObject.AddField("requestOwner", name);
        socket?.Emit("game.requestScore", jSONObject);
    }

    void RequestAddPlayer(string name)
    {
        JSONObject jSONObject = new JSONObject(JSONObject.Type.OBJECT);
        jSONObject.AddField("name", GameClient.PlayerName);
        socket?.Emit("add.player", jSONObject);
    }

    public void btnConnect_OnClick()
    {
        var webSocketURLFormat = "ws://{0}:{1}/socket.io/?EIO=4&transport=websocket";
        socket.url = string.Format(webSocketURLFormat, ipInputField.text, portInputField.text);

        UpdatePlayerName();

        gameSessionUI.ResetData();
        socket?.Connect();

        #if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
        try
        {
            string title = string.Format(WindowTitleFormat, Application.productName, PlayerName);
            SetWindowTitle(title);
        }
        catch (DllNotFoundException e)
        {
            Debug.Log(e.ToString());
        }
        #endif

        // ui.Show(GameUI.View.GameSessionView);
    }

    public void btnGameStart_OnClick()
    {
        //notify server and start a game session
        JSONObject jSONObject = new JSONObject(JSONObject.Type.OBJECT);
        jSONObject.AddField("requestOwner", GameClient.PlayerName);
        socket?.Emit("game.requestToStart", jSONObject);
    }

    public void btnLeaderBoard_OnClick()
    {
        socket?.Emit("game.requestLeaderBoard");
    }

    public void btnDisconnect_OnClick()
    {
        #if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
        try
        {
            SetWindowTitle(Application.productName);
        }
        catch (DllNotFoundException e)
        {
            Debug.Log(e.ToString());
        }
        #endif

        socket.Close();
    }

    public void UpdatePlayerName()
    {
        PlayerName = playerNameField.text;
    }

    public void TryCreateNewPlayer()
    {
        JSONObject jSONObject = new JSONObject(JSONObject.Type.OBJECT);
        jSONObject.AddField("requestOwner", PlayerName);
        socket.Emit("game.requestCreatePlayer", jSONObject);
    }

    public void UpdateScore(int tryCount)
    {
        string owner = GameClient.PlayerName;

        JSONObject jSONObject = new JSONObject(JSONObject.Type.OBJECT);

        jSONObject.AddField("requestOwner", owner);
        jSONObject.AddField("tryCount", tryCount);
        jSONObject.AddField("oldScore", score);

        socket.Emit("game.scoreSubmit", jSONObject);
        Debug.Log("send score");
    }
}
