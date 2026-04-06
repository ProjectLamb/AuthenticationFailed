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

    [Header("게임 설정")]
    public int targetScore = 6;
    private int p1Score = 0;
    private int p2Score = 0;
    public bool isGameOver = false;

    [Header("페이즈")]
    public int currentPhase = 1;

    [Header("멀티 설정")]
    public DataPacket_Player p2Player;

    private float timeRemaining = 30f;

    void Awake() { instance = this; }

    void Start()
    {
        bool isMulti = PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount >= 2;
        timeRemaining = isMulti ? 60f : 30f;

        // 솔로모드면 P2 점수 UI 숨기기
        if (!isMulti && p2ScoreText != null)
            p2ScoreText.gameObject.SetActive(false);

        UpdateUI();
        if (centerText != null) centerText.text = "";
    }

    void Update()
    {
        if (isGameOver) return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();

            if (PhotonNetwork.IsMasterClient && timeRemaining <= 0)
                photonView.RPC("RpcGameOver", RpcTarget.All, false);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && p2Player != null)
            p2Player.photonView.TransferOwnership(newPlayer);
    }

    // P2 바구니에 들어왔을 때 → P1 점수
    public void AddScore(int id)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        p1Score++;
        photonView.RPC("RpcSyncScore", RpcTarget.All, p1Score);

        if (p1Score >= targetScore)
        {
            bool isMulti = PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount >= 2;

            if (isMulti)
                photonView.RPC("RpcStartPhase2", RpcTarget.All);
            else
                photonView.RPC("RpcGameOver", RpcTarget.All, true); // 솔로는 바로 클리어
        }
    }

    // P1 바구니에 들어왔을 때 → P2 점수
    public void AddScoreP2(int id)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        p2Score++;
        photonView.RPC("RpcSyncScoreP2", RpcTarget.All, p2Score);

        if (p2Score >= targetScore)
            photonView.RPC("RpcGameOver", RpcTarget.All, true);
    }

    [PunRPC]
    void RpcSyncScore(int score)
    {
        p1Score = score;
        UpdateScoreUI();
    }

    [PunRPC]
    void RpcSyncScoreP2(int score)
    {
        p2Score = score;
        UpdateScoreUI();
    }

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

    void ClearCenterText()
    {
        if (centerText != null) centerText.text = "";
    }

    [PunRPC]
    void RpcGameOver(bool isClear)
    {
        isGameOver = true;
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