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
    public GameObject flappyGame;

    void Start()
    {
        // 마스터 클라이언트(방장)만 타이머 루프를 시작합니다.
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(TimeLimit());
        }
    }

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
        flappyGame.SetActive(false);
        GameManager.Instance.IsCanAgree = true;
    }
}
