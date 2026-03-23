using UnityEngine;
using Photon.Pun;

public class AIObjectSpawner : MonoBehaviourPun
{
    [Header("기본 도형 프리팹 (Resources 폴더 경로)")]
    public string[] shapes = { "Minigames/Shapes/Cube", "Minigames/Shapes/Cylinder", "Minigames/Shapes/Sphere" };

    [Header("매터리얼 프리팹 (Resources/Materials 폴더)")]
    // 미리 만들어둔 나무, 철, 유리 등의 매터리얼 이름
    public string[] materialNames = { "Mat_Wood", "Mat_Iron", "Mat_Brick" };

    public Transform spawnPoint;

    // 2P의 웹 UI에서 서버 응답을 받은 직후 호출할 함수
    public void RequestSpawnFromAI(int shapeIndex, int matIndex, float scaleX, float scaleY, float scaleZ)
    {
        // 2P가 방장(1P)에게 "이 스펙대로 스폰해줘!" 라고 요청
        photonView.RPC("RpcSpawnCustomObject", RpcTarget.MasterClient, shapeIndex, matIndex, scaleX, scaleY, scaleZ);
    }

    [PunRPC]
    void RpcSpawnCustomObject(int shapeIndex, int matIndex, float scaleX, float scaleY, float scaleZ)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 1. 지정된 형태의 프리팹 스폰 (스폰 포인트 위치에서)
        string selectedShape = shapes[Mathf.Clamp(shapeIndex, 0, shapes.Length - 1)];
        GameObject spawnedObj = PhotonNetwork.Instantiate(selectedShape, spawnPoint.position, Quaternion.identity);

        // 2. 스폰된 오브젝트의 고유 ID(ViewID)를 가져와서, 모두의 화면에 스케일과 재질을 동기화하라고 명령
        int viewID = spawnedObj.GetComponent<PhotonView>().ViewID;
        photonView.RPC("RpcApplyCustomProperties", RpcTarget.All, viewID, matIndex, scaleX, scaleY, scaleZ);
    }

    [PunRPC]
    void RpcApplyCustomProperties(int viewID, int matIndex, float scaleX, float scaleY, float scaleZ)
    {
        // ViewID로 방금 스폰된 오브젝트 찾기
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView == null) return;

        GameObject targetObj = targetView.gameObject;

        // [추가된 핵심 로직] 1P, 2P 모두의 화면에서 부모를 spawnPoint로 설정합니다.
        // true를 주면 현재의 월드 좌표와 크기를 유지한 채로 부모 밑으로 들어갑니다.
        targetObj.transform.SetParent(spawnPoint, true);

        // 3. 스케일(크기) 적용
        targetObj.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

        // 4. 재질(Material) 입히기
        string matName = materialNames[Mathf.Clamp(matIndex, 0, materialNames.Length - 1)];
        Material loadedMat = Resources.Load<Material>($"Materials/{matName}");

        if (loadedMat != null)
        {
            Renderer renderer = targetObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = loadedMat;
            }
        }

        Debug.Log($"[AI Spawner] {matName} 재질의 크기({scaleX}, {scaleY}, {scaleZ}) 오브젝트가 {spawnPoint.name} 하위에 생성 완료!");
    }


    // ---------------------------------------------------
    // [디버그 전용] AI 서버 응답 시뮬레이터
    // ---------------------------------------------------

    [ContextMenu("🚀 [테스트] '나무 다리' 소환 (AI 응답 시뮬레이션)")]
    public void DEBUG_SpawnWoodenBridge()
    {
        // 상황: 2P가 "나무 재질의 다리를 큼직하게 소환해줘" 라고 입력했고, 
        // 서버가 {shape: 0(Cube), mat: 0(Wood), scale: (5, 0.5, 2)} 를 리턴했다고 가정.

        // Cube(0), Wood(0), 가로 5, 높이 0.5, 세로 2
        RequestSpawnFromAI(0, 0, 5f, 0.5f, 2f);
        Debug.Log("[DEBUG] 🪵 AI가 '나무 다리'를 생성하라고 지시했습니다!");
    }

    [ContextMenu("🚀 [테스트] '거대한 철구' 소환 (AI 응답 시뮬레이션)")]
    public void DEBUG_SpawnGiantIronBall()
    {
        // 상황: 2P가 "무거운 쇠구슬로 다 부숴줘" 라고 입력했고,
        // 서버가 {shape: 2(Sphere), mat: 1(Iron), scale: (3, 3, 3)} 을 리턴했다고 가정.

        // Sphere(2), Iron(1), 지름 3
        RequestSpawnFromAI(2, 1, 3f, 3f, 3f);
        Debug.Log("[DEBUG] ⛓️ AI가 '거대한 철구'를 생성하라고 지시했습니다!");
    }

    [ContextMenu("🚀 [테스트] '통통 튀는 기둥' 소환 (AI 응답 시뮬레이션)")]
    public void DEBUG_SpawnBouncyPillar()
    {
        // Cylinder(1), Bouncy(2), 얇고 길게 (1, 4, 1)
        RequestSpawnFromAI(1, 2, 1f, 4f, 1f);
        Debug.Log("[DEBUG] 🍄 AI가 '통통 튀는 기둥'을 생성하라고 지시했습니다!");
    }
}