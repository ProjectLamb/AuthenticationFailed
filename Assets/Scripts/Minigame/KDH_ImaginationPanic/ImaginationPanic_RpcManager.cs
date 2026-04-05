using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro; // [추가] UI 텍스트 제어를 위한 네임스페이스

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

    [Header("타이머 및 UI 설정")]
    public float timeLimit = 90f;
    public TextMeshProUGUI timerText;  // [추가] 화면 상단에 띄울 "Time: N" 텍스트
    private float currentTime;
    private bool isGameActive = false;
    private int lastSyncedTime = -1;   // [추가] 1초마다 RPC를 쏘기 위한 최적화 변수

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentTime = timeLimit;
        isGameActive = true;

        if (PhotonNetwork.IsMasterClient)
        {
            world1P.SetActive(true);
            ui2P.SetActive(false);
            Debug.Log("[이매지네이션 패닉] 1P 모드로 3D 환경을 로드합니다.");

            GenerateRandomPassword();
            Debug.Log("[이매지네이션 패닉] 통과 패스워드를 생성합니다.");
        }
        else
        {
            world1P.SetActive(false);
            ui2P.SetActive(true);
            Debug.Log("[이매지네이션 패닉] 2P 모드로 AI 터미널을 로드합니다.");
        }
    }

    void Update()
    {
        // 게임이 진행 중일 때만 작동
        if (!isGameActive) return;

        // [핵심] 시간 계산은 오직 방장(1P)만 처리합니다.
        if (PhotonNetwork.IsMasterClient)
        {
            currentTime -= Time.deltaTime;

            // 남은 시간을 정수(올림)로 변환
            int currentSeconds = Mathf.CeilToInt(currentTime);

            // 1초 단위로 숫자가 바뀔 때만 전체 클라이언트에게 RPC로 UI 업데이트 지시 (트래픽 최적화)
            if (currentSeconds != lastSyncedTime)
            {
                lastSyncedTime = currentSeconds;
                photonView.RPC("RpcUpdateTimerUI", RpcTarget.All, currentSeconds);
            }

            // 시간이 0 이하로 떨어지면 게임 오버 처리
            if (currentTime <= 0f)
            {
                isGameActive = false;
                photonView.RPC("RpcTriggerGameOver", RpcTarget.All);
            }
        }
    }

    // 전체 클라이언트의 타이머 UI를 갱신하는 함수
    [PunRPC]
    void RpcUpdateTimerUI(int secondsLeft)
    {
        // 0 이하로 내려가지 않게 보정
        secondsLeft = Mathf.Max(0, secondsLeft);

        if (timerText != null)
        {
            timerText.text = $"Time: {secondsLeft}";
        }
    }

    // ==========================================
    // 3. 게임 오버 로직
    // ==========================================
    [PunRPC]
    void RpcTriggerGameOver()
    {
        isGameActive = false;

        Debug.Log("데이터가 유실되었습니다. 세션이 만료되었습니다.");

        // TODO: 에러 창(UI) 띄우기 로직 구현
        // TODO: 확인 버튼 클릭 시 로비 씬(태초 마을)으로 돌아가는 플로우 구현
    }

    // ==========================================
    // 4. 게임 성공 로직
    // ==========================================
    [PunRPC]
    void RpcTriggerGameSuccess()
    {
        isGameActive = false; // 성공했으므로 타이머 정지

        Debug.Log("세션이 다시 연결되었습니다.");

        // TODO: 성공 창(UI) 띄우기 로직 구현
        // TODO: 확인 버튼 클릭 시 다음 씬으로 넘어가거나 결과 창 띄우기
    }

    void GenerateRandomPassword()
    {
        targetPassword = "";

        while (targetPassword.Length < 6)
        {
            string randomDigit = Random.Range(0, 10).ToString();
            if (!targetPassword.Contains(randomDigit))
            {
                targetPassword += randomDigit;
            }
        }

        photonView.RPC("RpcSyncPassword", RpcTarget.All, targetPassword);
        Debug.Log($"[시스템] 이번 스테이지 비밀번호 (중복 없음!): {targetPassword}");
    }

    [PunRPC]
    void RpcSyncPassword(string pwd)
    {
        targetPassword = pwd;
    }

    public void OnPadStepped(int padNumber, int padViewID)
    {
        photonView.RPC("RpcCheckPasswordStep", RpcTarget.MasterClient, padNumber, padViewID);
    }

    [PunRPC]
    void RpcCheckPasswordStep(int padNumber, int padViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int expectedNumber = targetPassword[currentIndex] - '0';

        if (padNumber == expectedNumber)
        {
            PhotonView padView = PhotonView.Find(padViewID);
            if (padView != null)
            {
                padView.RPC("RpcUpdatePadVisual", RpcTarget.All, padViewID, true);
            }

            currentIndex++;

            if (currentIndex >= 6)
            {
                // [수정됨] 6자리를 모두 맞추면 전체 클라이언트에게 성공 RPC 발송
                photonView.RPC("RpcTriggerGameSuccess", RpcTarget.All);
            }
        }
        else
        {
            currentIndex = 0;
            photonView.RPC("RpcBroadcastReset", RpcTarget.All);
            Debug.Log("❌ 잘못된 번호입니다. 입력이 초기화됩니다.");
        }
    }

    [PunRPC]
    void RpcBroadcastReset()
    {
        PasswordPad[] allPads = FindObjectsOfType<PasswordPad>();
        foreach (var pad in allPads)
        {
            pad.ResetPadLocal();
        }
    }
}