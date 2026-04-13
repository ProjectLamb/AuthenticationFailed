using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPun
{
    private static GameManager instance = null;

    [SerializeField]
    private MiniGameManager miniGameManager = null;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("No GameManagerInstance");
            }
            return instance;
        }
    }

    public GameObject canvasDesktop;
    public GameObject canvasMobile;


    public bool IsCanAgree = false;
    public bool IsPCLoading = false;
    public int stageNumber = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DesktopOn()
    {
        canvasDesktop.SetActive(true);
        canvasMobile.SetActive(false);
    }

    public void MobileOn()
    {
        canvasDesktop.SetActive(false);
        canvasMobile.SetActive(true);
    }

    public void MiniGame()
    {
        miniGameManager.StartRandomMiniGame();
    }

    public void ClearMiniGame()
    {
        miniGameManager.SetGameClear();
    }
}
