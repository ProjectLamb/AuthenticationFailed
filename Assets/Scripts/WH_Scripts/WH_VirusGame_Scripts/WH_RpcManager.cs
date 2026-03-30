using UnityEngine;
using Photon.Pun;
using System.Collections;

public class WH_RpcManager : MonoBehaviourPun
{
    public WH_ObjectSpawner spawner;
    public WH_P2_Controller p2Controller;
    public WH_P1_Downloader p1Downloader;
    public WH_GameManager gameManager;

    [Header("Views")]
    public GameObject p1View;
    public GameObject p2View;

    private bool isGameEnded = false;

    void OnEnable()
    {
        // 서버 동기화 시간을 위해 0.2초 대기 후 역할 설정
        StartCoroutine(DelayedSetup());
    }

    IEnumerator DelayedSetup()
    {
        yield return new WaitForSeconds(0.2f);
        bool isMaster = PhotonNetwork.IsMasterClient;
        p1View.SetActive(isMaster);
        p2View.SetActive(!isMaster);
    }

    void Update()
    {
        // 모든 판정은 마스터(1P)만 수행
        if (!PhotonNetwork.IsMasterClient || isGameEnded) return;

        // [승리 조건] P1 게이지 100% AND P2 백신 10개 이상
        if (!isGameEnded && p1Downloader.IsFull())
        {
            isGameEnded = true;
            photonView.RPC("RPC_TriggerClearUI", RpcTarget.All);
        }
    }

    // --- 스폰 및 충돌 통신 ---
    public void RequestSpawn() => photonView.RPC("RPC_MasterSpawnRequest", RpcTarget.MasterClient);

    [PunRPC]
    void RPC_MasterSpawnRequest()
    {
        photonView.RPC("RPC_SyncSpawn", RpcTarget.All);
    }


    [PunRPC]
    void RPC_SyncSpawn()
    {
        spawner.SpawnVirus();
    }

    public void ReportCollision(string tag) => photonView.RPC("RPC_HandleCollision", RpcTarget.MasterClient, tag);

    [PunRPC]
    void RPC_HandleCollision(string tag)
    {
        if (isGameEnded) return;

        if (tag == "WH_Virus")
        {
            isGameEnded = true;
            photonView.RPC("RPC_TriggerVirusUI", RpcTarget.All);
        }
    }

    //[PunRPC] void RPC_SyncScore(int score) => p2Controller.UpdateScoreUI(score);
    [PunRPC] void RPC_TriggerVirusUI() => gameManager.TriggerVirusPenalty();
    [PunRPC] void RPC_TriggerClearUI() => gameManager.TriggerStageClear();
}