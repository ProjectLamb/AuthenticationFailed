using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Test_Game_RpcManager : MonoBehaviourPun
{
    [Header("UI 연결")]
    public TextMeshProUGUI sharedScoreText; // 양쪽 화면에 똑같이 보여야 할 점수판

    // [원칙 2] 각자 갖는 로컬 변수 (동기화 안 함)
    private int myClickCount = 0;

    // [원칙 1] 공통 변수 (오직 MasterClient만 값을 변경하고 관리함)
    private int sharedTotalScore = 0;

    void Start()
    {
        // 초기 텍스트 세팅
        sharedScoreText.text = "Score: 0";
    }

    // -----------------------------------------------------------
    // [액션 1] 유저가 미니게임 조작(버튼 클릭 등)을 했을 때 실행되는 함수
    // -----------------------------------------------------------
    [ContextMenu("점수 올리기")]
    public void OnClickGameButton()
    {
        // 내 로컬 변수는 여기서 바로 처리 (나만 아는 정보)
        myClickCount++;
        Debug.Log($"내가 클릭한 횟수: {myClickCount}");

        // [원칙 3] 서버(방장)에게 "나 이거 눌렀어! 점수 올려줘!" 라고 행동을 보고함
        // RpcTarget.MasterClient: 오직 방장(PC)에게만 이 메시지를 쏨
        photonView.RPC("RpcReportActionToServer", RpcTarget.MasterClient, 10); // 10점 올려달라고 요청
    }

    // -----------------------------------------------------------
    // [서버 로직] 방장(1P)만 실행하는 판사 역할의 함수
    // -----------------------------------------------------------
    [PunRPC]
    void RpcReportActionToServer(int scoreToAdd)
    {
        // 방장이 아니라면(혹시 모를 버그 방지) 튕겨냄
        if (!PhotonNetwork.IsMasterClient) return;

        // 방장만이 '공통 변수'를 조작할 권한이 있음
        sharedTotalScore += scoreToAdd;

        // 판정이 끝났으니, 이제 방장이 "모든 사람(RpcTarget.All)"에게 
        // 바뀐 최종 점수를 화면에 그리라고 명령을 내림
        photonView.RPC("RpcSyncDisplay", RpcTarget.All, sharedTotalScore);
    }

    // -----------------------------------------------------------
    // [동기화 로직] 방장의 명령을 받아 양쪽 클라이언트가 화면을 똑같이 맞추는 함수
    // -----------------------------------------------------------
    [PunRPC]
    void RpcSyncDisplay(int syncedScore)
    {
        // 1P, 2P 모두 이 함수가 실행되며 동일한 점수를 UI에 표시하게 됨
        sharedScoreText.text = $"Score: {syncedScore}";
    }
}
