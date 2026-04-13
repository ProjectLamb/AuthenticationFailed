using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class WH_Dino_RpcManager : MonoBehaviourPunCallbacks
{
    public WH_Dino_Manager gameManager;

    private int stopCount = 0;
    private bool gameEnded = false;
    private bool gameStarted = false;

    // 준비 완료한 플레이어를 actorNumber로 관리
    private HashSet<int> readyPlayers = new HashSet<int>();

    // -----------------------------
    // 각 플레이어가 준비 버튼 클릭
    // -----------------------------
    public void OnClickReadyButton()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log($"[Dino] 준비 버튼 클릭 / ActorNumber={actorNumber}");

        // 방장에게 준비 요청 보냄
        photonView.RPC(nameof(RPC_RegisterReady), RpcTarget.MasterClient, actorNumber);
    }

    // 방장이 준비 상태 등록
    [PunRPC]
    void RPC_RegisterReady(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (gameStarted)
            return;

        readyPlayers.Add(actorNumber);

        int current = readyPlayers.Count;
        int total = 2; // 현재는 2인 기준

        Debug.Log($"[Dino] 준비 인원: {current}/{total}");

        photonView.RPC(nameof(RPC_UpdateReadyCount), RpcTarget.All, current, total);

        if (current >= total)
        {
            photonView.RPC(nameof(RPC_StartDinoGame), RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_UpdateReadyCount(int current, int total)
    {
        if (gameManager != null)
        {
            gameManager.UpdateReadyCountUI(current, total);
        }
        else
        {
            Debug.LogError("[Dino] gameManager가 연결되지 않았습니다.");
        }
    }

    // -----------------------------
    // 전체 게임 시작
    // -----------------------------
    [PunRPC]
    void RPC_StartDinoGame()
    {
        if (gameStarted) return;

        gameStarted = true;
        gameEnded = false;
        stopCount = 0;

        if (gameManager != null)
        {
            gameManager.StartGameByNetwork();
        }
        else
        {
            Debug.LogError("[Dino] gameManager가 연결되지 않았습니다.");
        }
    }

    // -----------------------------
    // 성공 보고
    // -----------------------------
    public void ReportGoal()
    {
        if (gameEnded) return;

        photonView.RPC(nameof(RPC_SyncEndGame), RpcTarget.All, true);
    }

    // -----------------------------
    // 장애물 충돌 보고
    // -----------------------------
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

            if (PhotonNetwork.IsMasterClient)
            {
                WH_RegisterManager[] regManagers =
                    Object.FindObjectsByType<WH_RegisterManager>(FindObjectsSortMode.None);

                if (regManagers != null && regManagers.Length > 0)
                {
                    WH_RegisterManager targetManager = null;

                    foreach (var reg in regManagers)
                    {
                        if (reg != null && reg.isDesktop)
                        {
                            targetManager = reg;
                            break;
                        }
                    }

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

    // 플레이어가 나가면 준비 카운트도 다시 반영
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (readyPlayers.Contains(otherPlayer.ActorNumber))
        {
            readyPlayers.Remove(otherPlayer.ActorNumber);
            photonView.RPC(nameof(RPC_UpdateReadyCount), RpcTarget.All, readyPlayers.Count, 2);
        }
    }
}