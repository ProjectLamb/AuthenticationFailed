using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MiniGameManager : MonoBehaviourPun
{
    public static MiniGameManager Instance;
    public string[] MiniGames;
    public string MiniGameDir;
    public bool[] IsGamePlayed;

    public int curGameIdx;
    

    private GameObject currentMiniGame;

    void Awake()
    {
        Instance = this;
        IsGamePlayed = new bool[MiniGames.Length];
    }

    // 방장만 실행할 수 있는 미니게임 시작 함수
    public void StartRandomMiniGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 1. 아직 플레이하지 않은 게임의 인덱스(번호)들을 담을 리스트
        List<int> availableIndices = new List<int>();

        for (int i = 0; i < IsGamePlayed.Length; i++)
        {
            if (IsGamePlayed[i] == false) // 아직 플레이 안 했다면
            {
                availableIndices.Add(i);
            }
        }

        // 2. 모든 게임을 다 플레이했는지 체크
        if (availableIndices.Count == 0)
        {
            Debug.Log("모든 미니게임을 플레이했습니다!");
            // 필요하다면 여기서 IsGamePlayed를 모두 false로 리셋하는 로직 추가
            return;
        }

        // 3. 사용 가능한 인덱스 중 하나를 랜덤하게 선택
        int randomIndex = Random.Range(0, availableIndices.Count);
        int selectedGameIndex = availableIndices[randomIndex];

        // 4. 선택된 게임 정보 가져오기 및 상태 변경
        string selectedGame = MiniGames[selectedGameIndex];

        // 현재 선택된 게임의 인덱스를 기록해두기
        curGameIdx = selectedGameIndex;

        Debug.Log($"선택된 게임: {selectedGame}");
        
        // 5. 게임 생성 (기존 함수 호출)
        SpawnMiniGameNetworked(selectedGame);
    }

    public void SetGameClear()
    {
        IsGamePlayed[curGameIdx] = true;
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
            Debug.LogError($"{MiniGameDir}에 {minigameName} 프리팹이 없습니다. 에러: {e.Message}");
        }
    }
}
