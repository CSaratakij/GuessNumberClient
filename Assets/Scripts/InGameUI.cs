using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameUI : MonoBehaviour
{
    [SerializeField]
    GameClient game;

    [SerializeField]
    GameUI ui;

    [SerializeField]
    TextMeshProUGUI lblStatus;

    [SerializeField]
    Button[] btnNumpads;

    [SerializeField]
    Button btnClear;

    [SerializeField]
    Button btnSubmit;

    bool submitFlag = false;
    bool isGameStart = false;
    int tryCount = 0;

    void Awake()
    {
        SubscribeEvents();
    }

    void OnEnable()
    {
        ResetViewData();
        isGameStart = true;
    }

    void OnDisable()
    {
        isGameStart = false;
    }

    void SubscribeEvents()
    {
        for (int i = 0; i < btnNumpads.Length; ++i)
        {
            var number = (i + 1);

            if (number == 10)
            {
                number = 0;
            }

            btnNumpads[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>()?.SetText(number + "");

            btnNumpads[i].onClick.AddListener(() => {
                if (submitFlag)
                {
                    submitFlag = false;
                    ClearStatus();
                }

                AddNumber(number);
            });
        }

        btnClear.onClick.AddListener(() => {
            ClearStatus();
        });

        btnSubmit.onClick.AddListener(() => {
            if (!isGameStart || submitFlag)
            {
                return;
            }

            submitFlag = true;
            GameHandler();
        });
    }

    void GameHandler()
    {
        int tryNumber;
        bool canParse = int.TryParse(lblStatus.text, out tryNumber);

        if (!canParse)
        {
            Debug.Log("Number parse error..");
            return;
        }

        tryCount += 1;
        int delta = (tryNumber - game.CurrentNumber);

        if (delta > 0)
        {
            string currentTypeNumber = lblStatus.text;
            lblStatus.text = currentTypeNumber + " is High";
        }
        else if (delta < 0)
        {
            string currentTypeNumber = lblStatus.text;
            lblStatus.text = currentTypeNumber + " is Low";
        }
        else
        {
            string currentTypeNumber = lblStatus.text;
            lblStatus.text = "You win~\nThe number is : " + currentTypeNumber;

            StartCoroutine(OnGameFinish_Callback(3.0f));
            isGameStart = false;
        }
    }

    void AddNumber(int number)
    {
        lblStatus.text += (number + "");
    }

    void ClearStatus()
    {
        lblStatus.text = "";
        submitFlag = false;
    }

    IEnumerator OnGameFinish_Callback(float time)
    {
        yield return new WaitForSeconds(time);

        game.UpdateScore(tryCount);
        ResetViewData();

        ui.Show(GameUI.View.GameSessionView);
    }

    public void ResetViewData()
    {
        lblStatus.text = "";
        tryCount = 0;
        submitFlag = false;
    }

    public void GameStart()
    {
        lblStatus.text = "guess number~";
        isGameStart = true;
        submitFlag = true;

        game.RequestCurrentScore(GameClient.PlayerName);
    }
}
