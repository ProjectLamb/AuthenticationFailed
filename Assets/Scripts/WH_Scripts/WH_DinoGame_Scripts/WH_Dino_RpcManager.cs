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

    // �غ� �Ϸ��� �÷��̾ actorNumber�� ����
    private HashSet<int> readyPlayers = new HashSet<int>();

    // -----------------------------
    // �� �÷��̾ �غ� ��ư Ŭ��
    // -----------------------------
    public void OnClickReadyButton()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log($"[Dino] �غ� ��ư Ŭ�� / ActorNumber={actorNumber}");

        // ���忡�� �غ� ��û ����
        photonView.RPC(nameof(RPC_RegisterReady), RpcTarget.MasterClient, actorNumber);
    }

    // ������ �غ� ���� ���
    [PunRPC]
    void RPC_RegisterReady(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (gameStarted)
            return;

        readyPlayers.Add(actorNumber);

        int current = readyPlayers.Count;
        int total = 2; // ����� 2�� ����

        Debug.Log($"[Dino] �غ� �ο�: {current}/{total}");

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
            Debug.LogError("[Dino] gameManager�� ������� �ʾҽ��ϴ�.");
        }
    }

    // -----------------------------
    // ��ü ���� ����
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
            Debug.LogError("[Dino] gameManager�� ������� �ʾҽ��ϴ�.");
        }
    }

    // -----------------------------
    // ���� ����
    // -----------------------------
    public void ReportGoal()
    {
        if (gameEnded) return;

        photonView.RPC(nameof(RPC_SyncEndGame), RpcTarget.All, true);
        if (gameEnded) return;

        photonView.RPC(nameof(RPC_SyncEndGame), RpcTarget.All, true);
    }

    // -----------------------------
    // ��ֹ� �浹 ����
    // -----------------------------
    public void ReportStop()
    {
        if (gameEnded) return;

        photonView.RPC(nameof(RPC_HandleStopCount), RpcTarget.MasterClient);
        if (gameEnded) return;

        photonView.RPC(nameof(RPC_HandleStopCount), RpcTarget.MasterClient);
    }

    [PunRPC]
    void RPC_HandleStopCount()
    {
        if (gameEnded) return;

        if (gameEnded) return;

        stopCount++;


        if (stopCount >= 2)
        {
            photonView.RPC(nameof(RPC_SyncEndGame), RpcTarget.All, false);
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
                    Debug.Log("<color=cyan>���� �ܰ� ���� RPC ���� �Ϸ�</color>");
                }
                else
                {
                    Debug.LogError("������ WH_RegisterManager�� ã�� �� �����ϴ�.");
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

    // �÷��̾ ������ �غ� ī��Ʈ�� �ٽ� �ݿ�
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