using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class DataPacket_RpcManager : MonoBehaviourPunCallbacks
{
    public static DataPacket_RpcManager instance;

    [Header("UI 연결")]
    public TextMeshPro p1ScoreText;
    public TextMeshPro p2ScoreText;
    public TextMeshPro timerText;
    public TextMeshPro centerText;

    [Header("1단계: 시작 UI (Ready)")]
    public GameObject readyPanel;
    public GameObject readyButton;
    public GameObject waitingText;

    [Header("2단계: 조작법 UI (Control)")]
    public GameObject controlsPanel;
    public GameObject controlConfirmBtn;
    public GameObject controlWaitingText;

    [Header("게임 설정")]
    public int targetScore = 6;
    private int p1Score = 0;
    private int p2Score = 0;
    public bool isGameOver = false;
    public bool isGameStarted = false; // 🚨 실제 게임 시작 여부

    [Header("페이즈")]
    public int currentPhase = 1;

    [Header("멀티 설정")]
    public DataPacket_Player p2Player;
    private float timeRemaining = 30f;

    // 동기화용 변수
    private bool p1Ready = false;
    private bool p2Ready = false;
    private bool p1Confirm = false;
    private bool p2Confirm = false;

    void Awake() { instance = this; }

    void Start()
    {
        // 초기 UI 상태 세팅
        isGameStarted = false;
        isGameOver = false;
        transform.localPosition = Vector3.zero;
        if (readyPanel != null) readyPanel.SetActive(true);
        if (readyButton != null) readyButton.SetActive(true);
        if (waitingText != null) waitingText.SetActive(false);

        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (controlConfirmBtn != null) controlConfirmBtn.SetActive(true);
        if (controlWaitingText != null) controlWaitingText.SetActive(false);

        bool isMulti = PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount >= 2;
        timeRemaining = isMulti ? 60f : 30f;

        if (!isMulti && p2ScoreText != null)
            p2ScoreText.gameObject.SetActive(false);

        UpdateUI();
        if (centerText != null) centerText.text = "";
    }

    void Update()
    {
        // 🚨 조작법까지 확인 완료되어야 타이머가 흐릅니다.
        if (!isGameStarted || isGameOver) return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();

            if (PhotonNetwork.IsMasterClient && timeRemaining <= 0)
                photonView.RPC("RpcGameOver", RpcTarget.All, false);
        }
    }

    // =======================================================
    // [1단계] Ready 버튼 (양쪽 확인)
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
                photonView.RPC("RpcShowControls", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void RpcShowControls()
    {
        if (readyPanel != null) readyPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    // =======================================================
    // [2단계] 조작법 확인 버튼 (양쪽 확인)
    // =======================================================
    public void OnClickControlsConfirmBtn()
    {
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

        if (PhotonNetwork.IsMasterClient)
        {
            if ((p1Confirm && p2Confirm) || PhotonNetwork.CurrentRoom.PlayerCount == 1)
                photonView.RPC("RpcStartActualGame", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void RpcStartActualGame()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        isGameStarted = true; // 🚀 여기서부터 Update의 타이머가 움직입니다.
        Debug.Log("DataPacket 게임 시작!");
    }

    // =======================================================
    // [기존 로직] 점수 및 페이즈 관리
    // =======================================================
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && p2Player != null)
            p2Player.photonView.TransferOwnership(newPlayer);
    }

    public void AddScore(int id)
    {
        if (!PhotonNetwork.IsMasterClient || !isGameStarted) return;

        p1Score++;
        photonView.RPC("RpcSyncScore", RpcTarget.All, p1Score);

        if (p1Score >= targetScore)
        {
            bool isMulti = PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount >= 2;
            if (isMulti) photonView.RPC("RpcStartPhase2", RpcTarget.All);
            else photonView.RPC("RpcGameOver", RpcTarget.All, true);
        }
    }

    public void AddScoreP2(int id)
    {
        if (!PhotonNetwork.IsMasterClient || !isGameStarted) return;

        p2Score++;
        photonView.RPC("RpcSyncScoreP2", RpcTarget.All, p2Score);

        if (p2Score >= targetScore)
            photonView.RPC("RpcGameOver", RpcTarget.All, true);
    }

    [PunRPC]
    void RpcSyncScore(int score) { p1Score = score; UpdateScoreUI(); }

    [PunRPC]
    void RpcSyncScoreP2(int score) { p2Score = score; UpdateScoreUI(); }

    [PunRPC]
    void RpcStartPhase2()
    {
        currentPhase = 2;
        p2Score = 0;
        if (centerText != null) centerText.text = "PHASE 2!";
        Invoke("ClearCenterText", 2f);

        if (p2Player != null)
            p2Player.transform.localPosition = new Vector3(0.4f, p2Player.transform.localPosition.y, 0);

        UpdateUI();
    }

    void ClearCenterText() { if (centerText != null) centerText.text = ""; }

    [PunRPC]
    void RpcGameOver(bool isClear)
    {
        isGameOver = true;
        isGameStarted = false;
        timeRemaining = 0;
        UpdateTimerUI();
        if (centerText != null)
            centerText.text = isClear ? "MISSION CLEAR!!" : "GAME OVER";
    }

    void UpdateTimerUI() { if (timerText != null) timerText.text = "Time : " + Mathf.CeilToInt(Mathf.Max(0, timeRemaining)).ToString(); }
    void UpdateScoreUI()
    {
        if (p1ScoreText != null) p1ScoreText.text = "Score : " + p1Score + " / " + targetScore;
        if (p2ScoreText != null) p2ScoreText.text = "Score : " + p2Score + " / " + targetScore;
    }
    void UpdateUI() { UpdateTimerUI(); UpdateScoreUI(); }
}