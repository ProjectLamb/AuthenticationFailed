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

    private readonly HashSet<int> readyPlayers = new HashSet<int>();

    public void OnClickReadyButton()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log($"[Dino] 준비 버튼 클릭 / ActorNumber={actorNumber}");

        photonView.RPC(nameof(RPC_RegisterReady), RpcTarget.MasterClient, actorNumber);
    }

    [PunRPC]
    void RPC_RegisterReady(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (gameStarted)
            return;

        readyPlayers.Add(actorNumber);

        int current = readyPlayers.Count;
        int total = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount : 2;

        Debug.Log($"[Dino] 준비 인원: {current}/{total}");

        photonView.RPC(nameof(RPC_UpdateReadyCount), RpcTarget.All, current, total);

        if (current >= total && total >= 2)
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

    [PunRPC]
    void RPC_StartDinoGame()
    {
        if (gameStarted) return;

        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            Debug.LogWarning("[Dino] 플레이어 수가 부족해서 시작 취소");
            return;
        }

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

    public void ReportGoal()
    {
        if (gameEnded) return;

        photonView.RPC(nameof(RPC_SyncEndGame), RpcTarget.All, true);
    }

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
        Debug.Log($"[Dino] stopCount = {stopCount}");

        // 둘 다 부딪혔을 때만 실패
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

        if (!isSuccess)
        {
            Debug.Log("<color=red>Dino Game Failure!</color>");

            // 🔥 추가: 게임 실패 시(충돌 시) 모든 공룡의 움직임을 멈추고 
            // 아직 이미지가 안 바뀐 공룡이 있다면 데미지 상태로 강제 동기화
            WH_Dino_Controller_Multi[] allDinos = FindObjectsOfType<WH_Dino_Controller_Multi>();
            foreach (var dino in allDinos)
            {
                dino.StopDino(); // 움직임 정지
                                 // 부딪힌 공룡은 이미 이미지가 바뀌었겠지만, 확실히 하기 위해 호출
                if (dino.isMoving == false)
                {
                    dino.GetComponent<PhotonView>().RPC("RPC_ChangeToDamageSprite", RpcTarget.AllBuffered);
                }
            }

            if (gameManager != null) gameManager.OnFailure();
        }
        else if (isSuccess)
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

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (readyPlayers.Contains(otherPlayer.ActorNumber))
        {
            readyPlayers.Remove(otherPlayer.ActorNumber);

            int total = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount : 2;
            photonView.RPC(nameof(RPC_UpdateReadyCount), RpcTarget.All, readyPlayers.Count, total);
        }
    }
}