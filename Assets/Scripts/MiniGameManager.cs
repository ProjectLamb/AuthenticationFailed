using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MiniGameManager : MonoBehaviourPun
{
    public static MiniGameManager Instance;
    public string[] MiniGames;
    public string MiniGameDir;

    private GameObject currentMiniGame;

    void Awake()
    {
        Instance = this;
    }

    // 방장만 실행할 수 있는 미니게임 시작 함수
    public void StartRandomMiniGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int ran = Random.Range(0, MiniGames.Length);
        string selectedGame = MiniGames[ran];

        SpawnMiniGameNetworked(selectedGame);
    }

    // TODO: 테스트하고 배포 시 지울 것
    [Header("디버그 전용 세팅")]
    [Tooltip("테스트하고 싶은 미니게임 프리팹 이름을 적으세요")]
    public string debugGameName = "Test_Game";

    [ContextMenu("지정한 미니게임 강제 실행 (디버그)")]
    public void DEBUG_StartMiniGame()
    {
        // 1. 방에 제대로 들어와 있는지 체크 (안 그러면 RPC 쏠 때 에러 남)
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("포톤 방에 입장한 상태가 아닙니다! 방 접속부터 해주세요.");
            return;
        }

        // 2. 방장인지 체크
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("미니게임 시작 권한은 방장(PC)에게만 있습니다.");
            return;
        }

        // 3. 인스펙터에 적어둔 게임 이름으로 RPC 강제 발송
        SpawnMiniGameNetworked(debugGameName);
        Debug.Log($"[DEBUG] '{debugGameName}' 미니게임 강제 스폰 명령을 내렸습니다!");
    }

    private void SpawnMiniGameNetworked(string minigameName)
    {
        // 1. 기존에 켜져 있던 미니게임이 있다면 삭제
        if (currentMiniGame != null)
        {
            PhotonNetwork.Destroy(currentMiniGame);
        }

        // 2. Resources 폴더에서 프리팹을 불러와서 Container 아래에 생성
        try
        {
            Vector3 spawnPos = new Vector3(-49.4420013f, 18.2560005f, 0f);
            string resourcePath = $"{MiniGameDir}/{minigameName}";

            // 생성 (프리팹, 위치, 회전값)
            // Quaternion.identity는 '회전 없음'을 의미합니다.
            currentMiniGame = PhotonNetwork.Instantiate(resourcePath, spawnPos, Quaternion.identity);
            Debug.Log($"[{minigameName}] 미니게임 스폰 완료!");

        } catch (System.Exception e)
        {
            Debug.LogError($"{MiniGameDir}에 {minigameName} 프리팹이 없습니다. 에러: {{e.Message}}\"");
        }
    }
}
