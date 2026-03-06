using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ImaginationPanic_RpcManager : MonoBehaviourPun
{

    [Header("1P 오브젝트")]
    public GameObject world1P;

    [Header("2P 오브젝트")]
    public GameObject ui2P;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 내가 1P(PC 방장)라면 3D 세상을 켜고 챗봇 UI를 끈다.
            world1P.SetActive(true);
            ui2P.SetActive(false);
            Debug.Log("[이매지네이션 패닉] 1P 모드로 3D 환경을 로드합니다.");
        }
        else
        {
            // 내가 2P(모바일/서브)라면 3D 세상을 끄고 챗봇 UI를 켠다.
            world1P.SetActive(false);
            ui2P.SetActive(true);
            Debug.Log("[이매지네이션 패닉] 2P 모드로 AI 터미널을 로드합니다.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
