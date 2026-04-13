using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JW_Flappy_RpcManager : MonoBehaviourPun
{
    [Header("UI")]
    public TextMeshProUGUI timerText;
    private int limitTime = 30;
    public GameObject successWindow;
    public GameObject gamePrefab;
    public GameObject minigame;

    public Canvas canvasHtp;
    public GameObject alert;
    public GameObject htp;

    [Header("준비여부")]
    private bool IsDesktopOK = false;
    private bool IsMoblieOK = false;
    private bool IsAllOK = false;
    [Header("준비여부")]
    private bool IsDesktopReady = false;
    private bool IsMoblieReady = false;
    private bool IsAllReady = false;


    IEnumerator TimeLimit()
    {
        while (limitTime > 0)
        {
            yield return new WaitForSeconds(1f);
            limitTime -= 1;

            // 2. 마스터가 계산한 남은 시간을 모든 플레이어에게 동기화합니다.
            // RpcTarget.AllBuffered를 사용하면 늦게 들어온 사람도 현재 시간을 알 수 있습니다.
            photonView.RPC("RpcSyncTimer", RpcTarget.AllBuffered, limitTime);
        }
    }

    [PunRPC]
    void RpcSyncTimer(int networkedTime)
    {
        // 3. 전달받은 시간을 화면에 표시합니다.
        limitTime = networkedTime;
        timerText.text = limitTime.ToString();

        if (limitTime <= 0)
        {
            StartCoroutine("SuccessAlert");
        }
    }

    IEnumerator SuccessAlert()
    {
        Time.timeScale = 0;
        successWindow.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 1;
        gamePrefab.SetActive(false);
        GameManager.Instance.stageNumber += 1;
        // 현재 미니게임을 플레이가능 리스트에서 제외
        GameManager.Instance.ClearMiniGame();
        RegisterManager.Instance.IsClear = true;
    }

    public void OK()
    {
        photonView.RPC("RpcSetOK", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void RpcSetOK(int actorNumber)
    {
        if (actorNumber == 1) IsDesktopOK = true;
        else if (actorNumber == 2) IsMoblieOK = true;

        Debug.Log($"Player {actorNumber} OK!");

        // 마스터 클라이언트만 시작 조건을 체크하는 것이 안전합니다.
        if (PhotonNetwork.IsMasterClient)
        {
            CheckOK();
        }
    }
    void CheckOK()
    {
        if (IsDesktopOK && IsMoblieOK && !IsAllOK)
        {
            IsAllOK = true;
            photonView.RPC("RpcHtp", RpcTarget.All);
        }
    }

    [PunRPC]
    void RpcHtp()
    {
        alert.SetActive(false);
        htp.SetActive(true);
    }

    // 버튼을 눌렀을 때 호출되는 함수
    public void Ready()
    {
        // 내 ActorNumber에 따라 상대방에게도 내 상태를 알립니다.
        // RpcTarget.All로 보내야 나를 포함한 모든 사람의 IsReady 변수가 갱신됩니다.
        photonView.RPC("RpcSetReady", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void RpcSetReady(int actorNumber)
    {
        if (actorNumber == 1) IsDesktopReady = true;
        else if (actorNumber == 2) IsMoblieReady = true;

        Debug.Log($"Player {actorNumber} Ready!");

        // 마스터 클라이언트만 시작 조건을 체크하는 것이 안전합니다.
        if (PhotonNetwork.IsMasterClient)
        {
            CheckStartCondition();
        }
    }

    void CheckStartCondition()
    {
        if (IsDesktopReady && IsMoblieReady && !IsAllReady)
        {
            IsAllReady = true;
            photonView.RPC("RpcGameStart", RpcTarget.All);
        }
    }

    [PunRPC]
    void RpcGameStart()
    {
        canvasHtp.enabled = false;
        minigame.SetActive(true);
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(TimeLimit());
        }
    }
}
