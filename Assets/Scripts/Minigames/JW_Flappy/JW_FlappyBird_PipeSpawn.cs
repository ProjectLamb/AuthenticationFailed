using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun; // 포톤 기능 사용

public class JW_FlappyBird_PipeSpawn : MonoBehaviourPun
{
    // 프리팹을 직접 넣는 대신, Resources 폴더 안의 파일 이름을 적습니다.
    public string pipePrefabName = "Minigames/Pipes";

    public Slider pipeSlider;

    private float nowTime;
    private float makeTime = 3f;

    private static JW_FlappyBird_PipeSpawn instance = null;

    [SerializeField]

    public static JW_FlappyBird_PipeSpawn Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("No JW_FlappyBird_PipeSpawn Instance");
            }
            return instance;
        }
    }
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == 2) pipeSlider.gameObject.SetActive(true);
        nowTime = Time.time;
    }

    void Update()
    {
        // 1. 방장(Master Client)만 파이프를 생성하도록 제한합니다.
        // 이렇게 해야 파이프가 중복으로 2~3개씩 겹쳐서 생성되지 않습니다.
        if (!PhotonNetwork.IsMasterClient) return;

        if (Time.time - nowTime > makeTime)
        {
            nowTime = Time.time;
            SpawnPipe();
        }
    }

    void SpawnPipe()
    {
        // 2. 랜덤 Y값을 계산합니다.
        float randomY = Random.Range(-12f, 12f);

        // 3. 포톤 네트워크를 통해 파이프를 생성합니다. (임시로 (0,0,0)에 생성)
        GameObject newPipe = PhotonNetwork.Instantiate(pipePrefabName, Vector3.zero, Quaternion.identity);

        // 4. 방금 생성된 파이프의 고유 번호(ViewID)를 가져옵니다.
        int pipeViewID = newPipe.GetComponent<PhotonView>().ViewID;

        // 5. '이 스패너(this)'가 가진 RPC 함수를 호출하여, 모든 사람에게 파이프 설정을 명령합니다.
        photonView.RPC(nameof(RpcSetupPipe), RpcTarget.All, pipeViewID, randomY);
    }

    [PunRPC]
    void RpcSetupPipe(int viewID, float randomY)
    {
        // 6. 전달받은 번호로 생성된 파이프를 찾습니다.
        PhotonView targetPipeView = PhotonView.Find(viewID);

        if (targetPipeView != null)
        {
            GameObject pipeObj = targetPipeView.gameObject;

            // 기존 코드와 동일하게 부모, 스케일, 로컬 위치를 세팅합니다.
            pipeObj.transform.SetParent(this.transform);
            pipeObj.transform.localScale = new Vector3(0.001415335f, 1f, 1f);
            pipeObj.transform.localPosition = new Vector3(0.1364f, 24.7f + randomY, 0f);
        }
    }
}

