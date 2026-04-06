using UnityEngine;
using Photon.Pun;
using TMPro;

public class PingPong_RpcManager : MonoBehaviourPun
{
    [Header("UI 연결")]
    public TextMeshPro comboText;
    public TextMeshPro timerText;

    [Header("게임 설정")]
    public int targetCombo = 30;       // 목표 콤보
    private int currentCombo = 0;      // 현재 콤보
    private float timeRemaining = 40f; // 제한 시간 40초
    private bool isGameOver = false;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (isGameOver) return;

        // 시간은 각자의 컴퓨터에서 자연스럽게 줄어들도록 합니다.
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();

            // 단, 시간이 끝났다는 '판정'은 방장(MasterClient)만 내립니다.
            if (PhotonNetwork.IsMasterClient && timeRemaining <= 0)
            {
                // 시간 초과 -> 게임 오버(false) 신호를 모두에게 전송
                photonView.RPC("RpcGameOver", RpcTarget.All, false);
            }
        }
    }

    // 🔴 공이 패들에 닿았을 때 (방장의 공 스크립트에서 호출됨)
    public void AddCombo()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        currentCombo++;
        photonView.RPC("RpcSyncCombo", RpcTarget.All, currentCombo); // 양쪽 화면 콤보 갱신

        // 30콤보 달성 체크
        if (currentCombo >= targetCombo)
        {
            // 목표 달성 -> 게임 클리어(true) 신호를 모두에게 전송
            photonView.RPC("RpcGameOver", RpcTarget.All, true);
        }
    }

    // 🔴 공을 놓쳐서 양옆 벽에 닿았을 때 (방장의 공 스크립트에서 호출됨)
    public void ResetCombo()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        currentCombo = 0;
        photonView.RPC("RpcSyncCombo", RpcTarget.All, currentCombo);
    }

    // ================= [ RPC 함수들 (네트워크 동기화) ] =================

    [PunRPC]
    void RpcSyncCombo(int syncedCombo)
    {
        currentCombo = syncedCombo;
        UpdateComboUI();
    }

    [PunRPC]
    void RpcGameOver(bool isClear)
    {
        isGameOver = true;
        timeRemaining = 0;
        UpdateTimerUI();

        if (isClear)
        {
            if (comboText != null) comboText.text = "CLEAR!!";
            Debug.Log("P1/P2 모두 게임 클리어!");
        }
        else
        {
            if (comboText != null) comboText.text = "GAME OVER";
            Debug.Log("P1/P2 모두 게임 오버!");
        }

        // 게임이 끝났으니 공을 멈추라고 명령합니다.
        PingPongBall ball = FindObjectOfType<PingPongBall>();
        if (ball != null)
        {
            ball.StopBall();
        }
    }

    // ================= [ UI 업데이트 함수들 ] =================

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = "Time : " + Mathf.CeilToInt(Mathf.Max(0, timeRemaining)).ToString();
    }

    void UpdateComboUI()
    {
        if (comboText != null && !isGameOver)
            comboText.text = "Combo : " + currentCombo;
    }

    void UpdateUI()
    {
        UpdateTimerUI();
        UpdateComboUI();
    }
}
