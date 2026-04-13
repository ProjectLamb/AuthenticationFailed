using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class WH_RpcManager : MonoBehaviourPunCallbacks
{
    public WH_ObjectSpawner spawner;
    public WH_P2_Controller p2Controller;
    public WH_P1_Downloader p1Downloader;
    public WH_GameManager gameManager;

    [Header("Views")]
    public GameObject p1View;
    public GameObject p2View;

    [Header("Auto Spawn")]
    public float spawnInterval = 1f;
    public int spawnPerEdge = 3;

    [Header("P2 Life Settings")]
    public int maxLife = 3;
    private int currentLife;

    private bool isGameEnded = false;
    private bool gameStarted = false;
    private Coroutine autoSpawnRoutine;

    private HashSet<int> readyPlayers = new HashSet<int>();

    void Start()
    {
        currentLife = maxLife;
        StartCoroutine(DelayedSetup());
    }

    IEnumerator DelayedSetup()
    {
        yield return new WaitForSeconds(0.2f);

        bool isMaster = PhotonNetwork.IsMasterClient;

        if (p1View != null) p1View.SetActive(isMaster);
        if (p2View != null) p2View.SetActive(!isMaster);
    }

    void Update()
    {
        if (!gameStarted) return;
        if (isGameEnded) return;

        if (PhotonNetwork.OfflineMode)
        {
            if (p1Downloader != null && p1Downloader.IsFull())
            {
                isGameEnded = true;
                StopAutoSpawn();
                gameManager.TriggerStageClear();
            }
            return;
        }

        if (!PhotonNetwork.IsMasterClient) return;

        if (p1Downloader != null && p1Downloader.IsFull())
        {
            isGameEnded = true;
            StopAutoSpawn();
            photonView.RPC(nameof(RPC_TriggerClearUI), RpcTarget.All);
        }
    }
    public void RequestSpawn()
    {
        if (spawner == null)
        {
            Debug.LogError("[RpcManager] spawner�� ������� �ʾҽ��ϴ�.");
            return;
        }

        if (PhotonNetwork.OfflineMode)
        {
            spawner.SpawnVirus();
        }
        else
        {
            photonView.RPC(nameof(RPC_SpawnOneVirus), RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_SpawnOneVirus()
    {
        if (spawner != null)
            spawner.SpawnVirus();
    }
    public void OnClickReadyButton()
    {
        if (!PhotonNetwork.IsConnected) return;

        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        photonView.RPC(nameof(RPC_RegisterReady), RpcTarget.MasterClient, actorNumber);
    }

    [PunRPC]
    void RPC_RegisterReady(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (gameStarted) return;

        readyPlayers.Add(actorNumber);

        int current = readyPlayers.Count;
        int total = PhotonNetwork.CurrentRoom.PlayerCount; // 🔥 핵심 변경

        photonView.RPC(nameof(RPC_UpdateReadyCount), RpcTarget.All, current, total);

        Debug.Log($"[READY] current={current}, total={total}");

        // 🔥 진짜 핵심 조건
        if (current >= total && total >= 2)
        {
            photonView.RPC(nameof(RPC_StartMiniGame), RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_UpdateReadyCount(int current, int total)
    {
        if (gameManager != null)
            gameManager.UpdateReadyCountUI(current, total);
    }

    [PunRPC]
    void RPC_StartMiniGame()
    {
        if (gameStarted) return;

        // 🔥 이거 없으면 또 뚫림
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            Debug.LogWarning("플레이어 부족으로 시작 차단됨");
            return;
        }

        gameStarted = true;
        isGameEnded = false;
        currentLife = maxLife;

        if (gameManager != null)
        {
            gameManager.StartGameByNetwork();
            gameManager.UpdateLifeUI(currentLife);
        }

        StartCoroutine(StartSpawnWithDelay());
    }

    IEnumerator StartSpawnWithDelay()
    {
        yield return new WaitForSeconds(2f);

        if (isGameEnded) yield break;

        if (spawner != null)
            spawner.StartAutoSpawn();
    }

    private void StopAutoSpawn()
    {
        if (spawner != null)
            spawner.StopAutoSpawn();
    }

    public void ReportCollision(string tag)
    {
        Debug.Log($"[RpcManager] ReportCollision ȣ���: {tag}");

        if (!gameStarted || isGameEnded) return;

        if (PhotonNetwork.OfflineMode)
        {
            if (tag == "WH_Virus")
            {
                HandleVirusHitOffline();
            }
        }
        else
        {
            photonView.RPC(nameof(RPC_HandleCollision), RpcTarget.MasterClient, tag);
        }
    }

    void HandleVirusHitOffline()
    {
        if (isGameEnded) return;

        currentLife--;
        currentLife = Mathf.Max(currentLife, 0);

        if (gameManager != null)
            gameManager.UpdateLifeUI(currentLife);

        Debug.Log($"[VirusGame] �������� �ǰ�, ���� ������: {currentLife}");

        if (currentLife <= 0)
        {
            isGameEnded = true;
            StopAutoSpawn();
            gameManager.TriggerVirusPenalty();
        }
    }

    [PunRPC]
    void RPC_HandleCollision(string tag)
    {
        if (isGameEnded) return;

        if (tag == "WH_Virus")
        {
            currentLife--;
            currentLife = Mathf.Max(currentLife, 0);

            Debug.Log($"[VirusGame] �ǰ�! ���� ������: {currentLife}");

            photonView.RPC(nameof(RPC_UpdateLifeUI), RpcTarget.All, currentLife);

            if (currentLife <= 0)
            {
                isGameEnded = true;
                StopAutoSpawn();
                photonView.RPC(nameof(RPC_TriggerVirusUI), RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void RPC_UpdateLifeUI(int life)
    {
        if (gameManager != null)
            gameManager.UpdateLifeUI(life);
    }

    [PunRPC]
    void RPC_TriggerVirusUI()
    {
        if (gameManager != null)
            gameManager.TriggerVirusPenalty();
    }

    [PunRPC]
    void RPC_TriggerClearUI()
    {
        StopAutoSpawn();

        if (gameManager != null)
            gameManager.TriggerStageClear();

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
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (readyPlayers.Contains(otherPlayer.ActorNumber))
        {
            readyPlayers.Remove(otherPlayer.ActorNumber);
            photonView.RPC(nameof(RPC_UpdateReadyCount), RpcTarget.All, readyPlayers.Count, 2);
        }
    }
}