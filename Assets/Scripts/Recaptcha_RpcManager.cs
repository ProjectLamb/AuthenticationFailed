using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recaptcha_RpcManager : MonoBehaviourPun
{
    private bool is1P;
    private int sharedCurrentTargetIndex = 0;

    [Header("블록 연결")]
    public BlockMoving[] blocks;

    void Start()
    {
        is1P = PhotonNetwork.IsMasterClient;
    }

    void Update()
    {
        if (sharedCurrentTargetIndex >= blocks.Length) return;

        bool isMyTurn = (is1P && sharedCurrentTargetIndex % 2 == 0) ||
                        (!is1P && sharedCurrentTargetIndex % 2 != 0);

        if (isMyTurn && Input.GetKeyDown(KeyCode.Return))
        {
            BlockMoving currentBlock = blocks[sharedCurrentTargetIndex];
            if (currentBlock.CheckAndGetTargetY(out float targetY))
            {
                photonView.RPC("RpcReportActionToServer", RpcTarget.MasterClient,
                               sharedCurrentTargetIndex, targetY);
            }
        }
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
    }
}