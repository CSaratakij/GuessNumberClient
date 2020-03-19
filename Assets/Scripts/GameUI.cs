using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    public enum View
    {
        ConnectView = 0,
        GameSessionView = 1,
        InGameView = 2,
        LeaderBoardView = 3
    }

    [SerializeField]
    CanvasGroup[] views;

    int currentViewID = 0;
    public View CurrentView => (View)currentViewID;

    void Awake()
    {
        Show(View.ConnectView);
    }

    void HideAll()
    {
        for (int i = 0; i < views.Length; ++i)
        {
            views[i].alpha = 0;
            views[i].blocksRaycasts = false;
        }
    }

    public void Show(View view)
    {
        Show((int)view);
    }

    public void Show(int id)
    {
        HideAll();

        views[id].alpha = 1;
        views[id].blocksRaycasts = true;

        currentViewID = id;
    }
}
