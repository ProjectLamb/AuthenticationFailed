using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Recaptcha_RpcManager : MonoBehaviourPun
{
    private bool is1P;
    private int sharedCurrentTargetIndex = 0;

    [Header("블록 연결")]
    public BlockMoving[] blocks;

    [Header("횟수 UI 연결")]
    public TextMeshPro attemptText;

    private int attemptsLeft = 3;
    private const int MAX_ATTEMPTS = 3;

    void Start()
    {
        is1P = PhotonNetwork.IsMasterClient;
        UpdateAttemptText();
        StartCoroutine(InitVisuals());
    }

    IEnumerator InitVisuals()
    {
        yield return new WaitForSeconds(0.1f);
        UpdateTurnText();
    }

    void Update()
    {
        if (sharedCurrentTargetIndex >= blocks.Length) return;

        bool isMyTurn = (is1P && sharedCurrentTargetIndex % 2 == 0) ||
                        (!is1P && sharedCurrentTargetIndex % 2 != 0);

        if (isMyTurn && Input.GetKeyDown(KeyCode.Return))
        {
            if (attemptsLeft <= 0) return;

            BlockMoving currentBlock = blocks[sharedCurrentTargetIndex];
            if (currentBlock.CheckAndGetTargetY(out float targetY))
            {
                photonView.RPC("RpcReportActionToServer", RpcTarget.MasterClient,
                               sharedCurrentTargetIndex, targetY);
            }
            else
            {
                attemptsLeft--;
                UpdateAttemptText();
                Debug.Log($"실패! 남은 횟수: {attemptsLeft}");

                if (attemptsLeft <= 0)
                    photonView.RPC("RpcResetGame", RpcTarget.All);
            }
        }
    }

    // 현재 턴 블록에만 텍스트 표시
    void UpdateTurnText()
    {
        for (int i = 0; i < blocks.Length; i++)
        {
            if (i == sharedCurrentTargetIndex)
            {
                bool isP1Turn = (i % 2 == 0);
                blocks[i].SetTurnText(true, isP1Turn);
            }
            else
            {
                blocks[i].SetTurnText(false);
            }
        }
    }

    void UpdateAttemptText()
    {
        if (attemptText == null) return;
        attemptText.text = $"{attemptsLeft}/{MAX_ATTEMPTS}";
        attemptText.color = (attemptsLeft <= 0) ? Color.red : Color.gray;
    }

    [PunRPC]
    void RpcReportActionToServer(int blockIndex, float targetY)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (blockIndex == sharedCurrentTargetIndex)
        {
            sharedCurrentTargetIndex++;
            photonView.RPC("RpcSyncDisplay", RpcTarget.All,
                           blockIndex, targetY, sharedCurrentTargetIndex);
        }
    }

    [PunRPC]
    void RpcSyncDisplay(int syncedBlockIndex, float syncedTargetY, int nextIndex)
    {
        blocks[syncedBlockIndex].StopAndAlignRPC(syncedTargetY);
        sharedCurrentTargetIndex = nextIndex;

        attemptsLeft = MAX_ATTEMPTS;
        UpdateAttemptText();
        UpdateTurnText();

        Debug.Log($"{syncedBlockIndex}번 블록 정렬 완료! 다음: {sharedCurrentTargetIndex}번");
    }

    [PunRPC]
    void RpcResetGame()
    {
        sharedCurrentTargetIndex = 0;
        attemptsLeft = MAX_ATTEMPTS;
        UpdateAttemptText();

        foreach (var block in blocks)
            if (block != null) block.ResetBlock();

        UpdateTurnText();
        Debug.Log("게임 초기화!");
    }

    public void ResetGame()
    {
        photonView.RPC("RpcResetGame", RpcTarget.All);
    }
}