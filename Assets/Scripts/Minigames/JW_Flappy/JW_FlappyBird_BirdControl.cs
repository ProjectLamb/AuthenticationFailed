using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class JW_FlappyBird_BirdControl : MonoBehaviourPun
{

    public Canvas canvasBird;
    public GameObject flappyGame;
    public Image failImg;
    private Rigidbody rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>(); 
        canvasBird.worldCamera = Camera.main;
    }

    void Update()
    {
        // 1. 내 소유의 오브젝트가 아니면 조작하지 않습니다.
        if (!photonView.IsMine) return;

        // 2. PC 플레이어(ActorNumber 1)만 클릭할 수 있게 제한합니다.
        // (만약 폰 사용자가 1번이 될 수도 있다면 기기 환경에 따른 체크가 더 정확합니다.)
        if (PhotonNetwork.LocalPlayer.ActorNumber != 1) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            Jump();
        }
    }

    void Jump()
    {
        rigid.velocity = Vector3.zero;
        rigid.AddForce(0, 2000, 0);
    }

    void OnCollisionEnter(Collision col)
    {
        // 3. 충돌 판정도 소유권을 가진 사람(PC) 화면에서만 처리하여 
        // 네트워크 지연으로 인한 오작동을 방지합니다.
        if (col.gameObject.tag == "JW_PIPE")
        {
            Debug.Log("앙 충돌띠");
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                Debug.Log("GameOver");
                // 게임오버 시에도 모든 사람의 화면에서 flappyGame이 꺼지게 하려면 RPC를 씁니다.
                photonView.RPC("RpcGameOver", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void RpcGameOver()
    {
        StartCoroutine("FailAlert");
    }
    IEnumerator FailAlert()
    {
        Time.timeScale = 0;
        failImg.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 1;
        Destroy(flappyGame.gameObject);
    }

}
