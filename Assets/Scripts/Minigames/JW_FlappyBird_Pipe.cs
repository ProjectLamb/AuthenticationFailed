using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; // 포톤 기능 사용
public class JW_FlappyBird_Pipe : MonoBehaviourPun
{

    void FixedUpdate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 좌측 이동
            this.transform.localPosition += new Vector3(-0.03f * Time.deltaTime, 0, 0);

            if (this.transform.localPosition.x < -0.0762f)
            {
                PhotonNetwork.Destroy(this.gameObject);
            }
        }

        // Actor2가 입력 보내기
        if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            SendInput();
        }
    }

    void SendInput()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            photonView.RPC("MovePipe", RpcTarget.MasterClient, 1);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            photonView.RPC("MovePipe", RpcTarget.MasterClient, -1);
        }
    }

    [PunRPC]
    void MovePipe(int dir)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        float moveY = 15f * Time.deltaTime * dir;

        if (dir > 0 && transform.localPosition.y < 44f)
        {
            transform.localPosition += new Vector3(0, moveY, 0);
        }
        else if (dir < 0 && transform.localPosition.y > 8f)
        {
            transform.localPosition += new Vector3(0, moveY, 0);
        }
    }
}
