using UnityEngine;
using Photon.Pun;

public class WH_Dino_RpcManager : MonoBehaviourPun
{
    public WH_Dino_Manager gameManager;

    private int stopCount = 0;
    private bool gameEnded = false;

    // 성공 보고
    public void ReportGoal()
    {
        if (gameEnded) return;

        photonView.RPC(nameof(RPC_SyncEndGame), RpcTarget.All, true);
    }

    // 장애물 충돌 보고
    public void ReportStop()
    {
        if (gameEnded) return;

        photonView.RPC(nameof(RPC_HandleStopCount), RpcTarget.MasterClient);
    }

    [PunRPC]
    void RPC_HandleStopCount()
    {
        if (gameEnded) return;

        stopCount++;

        if (stopCount >= 2)
        {
            photonView.RPC(nameof(RPC_SyncEndGame), RpcTarget.All, false);
        }
    }

    [PunRPC]
    void RPC_SyncEndGame(bool isSuccess)
    {
        if (gameEnded) return;
        gameEnded = true;

        if (isSuccess)
        {
            Debug.Log("<color=green>Dino Game Success!</color>");

            if (gameManager != null)
                gameManager.OnSuccess();

            // 인증 단계 시작 신호는 방장만 1회 전송
            if (PhotonNetwork.IsMasterClient)
            {
                WH_RegisterManager[] regManagers =
                    Object.FindObjectsByType<WH_RegisterManager>(FindObjectsSortMode.None);

                if (regManagers != null && regManagers.Length > 0)
                {
                    // 데스크탑용 매니저 우선 찾기
                    WH_RegisterManager targetManager = null;

                    foreach (var reg in regManagers)
                    {
                        if (reg != null && reg.isDesktop)
                        {
                            targetManager = reg;
                            break;
                        }
                    }

                    // 데스크탑용이 없으면 첫 번째 매니저 사용
                    if (targetManager == null)
                        targetManager = regManagers[0];

                    targetManager.OnMiniGameClear();
                    Debug.Log("<color=cyan>인증 단계 시작 RPC 전송 완료</color>");
                }
                else
                {
                    Debug.LogError("씬에서 WH_RegisterManager를 찾을 수 없습니다.");
                }
            }
        }
        else
        {
            Debug.Log("<color=red>Dino Game Failure!</color>");

            if (gameManager != null)
                gameManager.OnFailure();
        }
    }
}