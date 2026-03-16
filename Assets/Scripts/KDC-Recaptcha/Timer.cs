using Photon.Pun;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Timer : MonoBehaviourPun
{
    [Header("연결")]
    public TextMeshPro timerText;     // Quad 자식 TextMeshPro 연결
    public BlockMoving[] blocks;      // 블록 배열 연결

    [Header("설정")]
    public float totalTime = 60f;     

    private float currentTime;
    private bool isRunning = false;

    void Start()
    {
        currentTime = totalTime;
        UpdateTimerText();

        // 방장만 타이머 시작
        if (PhotonNetwork.IsMasterClient)
            StartTimer();
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    void Update()
    {
        if (!isRunning) return;
        if (!PhotonNetwork.IsMasterClient) return; // 방장만 카운트

        currentTime -= Time.deltaTime;

        // 모든 클라이언트 타이머 동기화 (0.1초마다)
        photonView.RPC("RpcSyncTimer", RpcTarget.All, currentTime);

        if (currentTime <= 0f)
        {
            isRunning = false;
            photonView.RPC("RpcResetBlocks", RpcTarget.All);
        }
    }

    [PunRPC]
    void RpcSyncTimer(float time)
    {
        currentTime = time;
        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);

        // 10초 이하면 빨간색 + 흔들림
        if (currentTime <= 10f)
        {
            timerText.color = Color.red;
            
        }
        else
        {
            timerText.color = Color.green;
        }
    }

    [PunRPC]
    void RpcResetBlocks()
    {
        // 타이머 초기화
        currentTime = totalTime;
        UpdateTimerText();

        // 블록 초기화
        foreach (var block in blocks)
        {
            if (block != null)
                block.ResetBlock();
        }

        // 방장이면 타이머 재시작
        if (PhotonNetwork.IsMasterClient)
            isRunning = true;
    }
}