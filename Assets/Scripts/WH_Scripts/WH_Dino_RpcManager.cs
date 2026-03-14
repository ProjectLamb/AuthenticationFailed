using UnityEngine;
using Photon.Pun;

public class WH_Dino_RpcManager : MonoBehaviourPun
{
    public WH_Dino_Manager gameManager;
    private int stopCount = 0;

    // 성공 보고 (누군가 부딪히면 전체 클라이언트에 성공 알림)
    public void ReportGoal()
    {
        photonView.RPC("RPC_SyncEndGame", RpcTarget.All, true);
    }

    // 장애물 충돌 보고 (방장만 카운트 체크)
    public void ReportStop()
    {
        photonView.RPC("RPC_HandleStopCount", RpcTarget.MasterClient);
    }

    [PunRPC]
    void RPC_HandleStopCount()
    {
        stopCount++;
        if (stopCount >= 2)
        {
            photonView.RPC("RPC_SyncEndGame", RpcTarget.All, false);
        }
    }

    [PunRPC]
    void RPC_SyncEndGame(bool isSuccess)
    {
        if (isSuccess) gameManager.OnSuccess();
        else gameManager.OnFailure();
    }
}