using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ImaginationPanic_RpcManager : MonoBehaviourPun
{

    [Header("1P 오브젝트")]
    public GameObject world1P;

    [Header("2P 오브젝트")]
    public GameObject ui2P;

    public static ImaginationPanic_RpcManager Instance;

    [Header("게임 상태")]
    public string targetPassword = ""; // 생성된 6자리 패스워드
    public int currentIndex = 0;       // 현재 P1이 맞춰야 할 비밀번호 자릿수 인덱스

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 내가 1P(PC 방장)라면 3D 세상을 켜고 챗봇 UI를 끈다.
            world1P.SetActive(true);
            ui2P.SetActive(false);
            Debug.Log("[이매지네이션 패닉] 1P 모드로 3D 환경을 로드합니다.");

            GenerateRandomPassword();
            Debug.Log("[이매지네이션 패닉] 통과 패스워드를 생성합니다.");
        }
        else
        {
            // 내가 2P(모바일/서브)라면 3D 세상을 끄고 챗봇 UI를 켠다.
            world1P.SetActive(false);
            ui2P.SetActive(true);
            Debug.Log("[이매지네이션 패닉] 2P 모드로 AI 터미널을 로드합니다.");
        }
    }

    void GenerateRandomPassword()
    {
        targetPassword = "";

        // 비밀번호가 6자리가 될 때까지 계속 뽑습니다.
        while (targetPassword.Length < 6)
        {
            // 0부터 9 사이의 랜덤 숫자 뽑기
            string randomDigit = Random.Range(0, 10).ToString();

            // 만약 현재 만들어진 비밀번호 안에 방금 뽑은 숫자가 없다면? -> 추가!
            if (!targetPassword.Contains(randomDigit))
            {
                targetPassword += randomDigit;
            }
        }

        // P2의 API 호출을 위해 타겟 코드를 네트워크 전체에 동기화
        photonView.RPC("RpcSyncPassword", RpcTarget.All, targetPassword);
        Debug.Log($"[시스템] 이번 스테이지 비밀번호 (중복 없음!): {targetPassword}");
    }

    [PunRPC]
    void RpcSyncPassword(string pwd)
    {
        targetPassword = pwd;
    }

    // P1이 발판(PasswordPad)을 밟았을 때 호출되는 함수
    public void OnPadStepped(int padNumber, int padViewID)
    {
        // 채점은 방장(서버)에서만 진행
        photonView.RPC("RpcCheckPasswordStep", RpcTarget.MasterClient, padNumber, padViewID);
    }

    [PunRPC]
    void RpcCheckPasswordStep(int padNumber, int padViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int expectedNumber = targetPassword[currentIndex] - '0'; // char를 int로 변환

        if (padNumber == expectedNumber)
        {
            // [핵심 수정] 1. 매니저의 무전기가 아니라, '밟힌 발판'의 무전기를 찾아서 호출합니다!
            PhotonView padView = PhotonView.Find(padViewID);
            if (padView != null)
            {
                padView.RPC("RpcUpdatePadVisual", RpcTarget.All, padViewID, true);
            }

            currentIndex++;

            if (currentIndex >= 6)
            {
                Debug.Log("🎉 비밀번호 해제 완료! 스테이지 클리어!");
                // TODO: 클리어 처리 로직 (문 열림 등)
            }
        }
        else
        {
            // 오답! 진행도 초기화
            currentIndex = 0;

            // [핵심 수정] 2. 오답 시 맵 전체의 발판을 리셋하라고 매니저 자신의 무전기로 싹 다 방송합니다.
            photonView.RPC("RpcBroadcastReset", RpcTarget.All);
            Debug.Log("❌ 잘못된 번호입니다. 입력이 초기화됩니다.");
        }
    }

    // 매니저 전용: 모든 클라이언트에게 "맵에 있는 모든 발판 리셋해!" 라고 명령하는 함수
    [PunRPC]
    void RpcBroadcastReset()
    {
        // 씬에 깔려있는 모든 PasswordPad 스크립트를 찾아서 초기화시킵니다.
        PasswordPad[] allPads = FindObjectsOfType<PasswordPad>();
        foreach (var pad in allPads)
        {
            pad.ResetPadLocal();
        }
    }
}
