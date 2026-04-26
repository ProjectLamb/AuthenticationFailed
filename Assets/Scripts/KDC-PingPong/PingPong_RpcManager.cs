using UnityEngine;
using Photon.Pun;
using TMPro;

public class PingPong_RpcManager : MonoBehaviourPun
{
    public static PingPong_RpcManager instance;

    [Header("UI 연결")]
    public TextMeshPro comboText;
    public TextMeshPro timerText;
    public TextMeshPro centerText;

    [Header("1단계: 시작 UI (Ready)")]
    public GameObject readyPanel;
    public GameObject readyButton;
    public GameObject waitingText;

    [Header("2단계: 조작법 UI (Control)")]
    public GameObject controlsPanel;
    public GameObject controlConfirmBtn;   // 새로 추가: 조작법의 '확인' 버튼
    public GameObject controlWaitingText;  // 새로 추가: 조작법의 "상대방 대기 중..." 텍스트

    [Header("게임 설정")]
    public int targetCombo = 30;
    private int currentCombo = 0;
    private float timeRemaining = 40f;
    public bool isGameOver = false;
    public bool isGameStarted = false;

    // 레디 상태 체크용
    private bool p1Ready = false;
    private bool p2Ready = false;

    // 조작법 확인 상태 체크용 (새로 추가)
    private bool p1Confirm = false;
    private bool p2Confirm = false;

    void Awake() { instance = this; }

    void Start()
    {
        isGameStarted = false;
        isGameOver = false;

        // 1단계 UI 켜고 2단계 UI 끄기
        if (readyPanel != null) readyPanel.SetActive(true);
        if (readyButton != null) readyButton.SetActive(true);
        if (waitingText != null) waitingText.SetActive(false);

        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (controlConfirmBtn != null) controlConfirmBtn.SetActive(true);
        if (controlWaitingText != null) controlWaitingText.SetActive(false);

        UpdateUI();
        if (centerText != null) centerText.text = "";
    }

    void Update()
    {
        if (!isGameStarted || isGameOver) return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();

            if (PhotonNetwork.IsMasterClient && timeRemaining <= 0)
            {
                photonView.RPC("RpcGameOver", RpcTarget.All, false);
            }
        }
    }

    // =======================================================
    // [1단계] Ready 버튼 로직
    // =======================================================
    public void OnClickReadyButton()
    {
        if (readyButton != null) readyButton.SetActive(false);
        if (waitingText != null) waitingText.SetActive(true);

        int myId = PhotonNetwork.IsMasterClient ? 1 : 2;
        photonView.RPC("RpcPlayerReady", RpcTarget.AllBuffered, myId);
    }

    [PunRPC]
    void RpcPlayerReady(int playerId)
    {
        if (playerId == 1) p1Ready = true;
        if (playerId == 2) p2Ready = true;

        if (PhotonNetwork.IsMasterClient)
        {
            if ((p1Ready && p2Ready) || PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                photonView.RPC("RpcShowControls", RpcTarget.AllBuffered);
            }
        }
    }

    [PunRPC]
    void RpcShowControls()
    {
        // Ready 창 끄고 조작법 창 켜기
        if (readyPanel != null) readyPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    // =======================================================
    // [2단계] 조작법 확인 버튼 로직 (양쪽 다 눌러야 통과)
    // =======================================================
    public void OnClickControlsConfirmBtn()
    {
        // 누르면 버튼 숨기고 "대기 중..." 텍스트 띄움
        if (controlConfirmBtn != null) controlConfirmBtn.SetActive(false);
        if (controlWaitingText != null) controlWaitingText.SetActive(true);

        int myId = PhotonNetwork.IsMasterClient ? 1 : 2;
        photonView.RPC("RpcPlayerConfirm", RpcTarget.AllBuffered, myId);
    }

    [PunRPC]
    void RpcPlayerConfirm(int playerId)
    {
        if (playerId == 1) p1Confirm = true;
        if (playerId == 2) p2Confirm = true;

        // 방장이 체크: 둘 다 조작법 확인을 눌렀는가?
        if (PhotonNetwork.IsMasterClient)
        {
            if ((p1Confirm && p2Confirm) || PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                photonView.RPC("RpcStartActualGame", RpcTarget.AllBuffered);
            }
        }
    }

    [PunRPC]
    void RpcStartActualGame()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);

        isGameStarted = true;

        PingPongBall ball = FindObjectOfType<PingPongBall>();
        if (ball != null) ball.ResetBall();

        Debug.Log("양쪽 모두 확인 완료! 게임 진짜 시작!");
    }

    // =======================================================
    // [게임 로직]
    // =======================================================
    public void AddCombo()
    {
        if (!PhotonNetwork.IsMasterClient || !isGameStarted) return;
        currentCombo++;
        photonView.RPC("RpcSyncCombo", RpcTarget.All, currentCombo);
        if (currentCombo >= targetCombo) photonView.RPC("RpcGameOver", RpcTarget.All, true);
    }

    public void ResetCombo()
    {
        if (!PhotonNetwork.IsMasterClient || !isGameStarted) return;
        currentCombo = 0;
        photonView.RPC("RpcSyncCombo", RpcTarget.All, currentCombo);
    }

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
        isGameStarted = false;
        timeRemaining = 0;
        UpdateTimerUI();

        if (isClear) { if (comboText != null) comboText.text = "CLEAR!!"; }
        else { if (comboText != null) comboText.text = "GAME OVER"; }

        PingPongBall ball = FindObjectOfType<PingPongBall>();
        if (ball != null) ball.StopBall();
    }

    void UpdateTimerUI() { if (timerText != null) timerText.text = "Time : " + Mathf.CeilToInt(Mathf.Max(0, timeRemaining)).ToString(); }
    void UpdateComboUI() { if (comboText != null && !isGameOver) comboText.text = "Combo : " + currentCombo; }
    void UpdateUI() { UpdateTimerUI(); UpdateComboUI(); }
}