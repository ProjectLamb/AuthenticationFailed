using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; // 포톤 기능 사용
public class JW_FlappyBird_Pipe : MonoBehaviourPun
{

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.localPosition += new Vector3(-0.03f*Time.deltaTime,0,0);
        if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            ControlPipe();
        }
        if(this.gameObject.transform.localPosition.x < -0.0762f)
        {
            Destroy(this.gameObject);
        }
    }

    [PunRPC]
    void ControlPipe()
    {
        if(Input.GetKey(KeyCode.DownArrow))
        {
            if(this.gameObject.transform.localPosition.y > 8f)
            {
                this.gameObject.transform.localPosition += new Vector3(0,-15f*Time.deltaTime,0);
            }
        }

        else if(Input.GetKey(KeyCode.UpArrow))
        {
            if(this.gameObject.transform.localPosition.y < 44f)
            {
                this.gameObject.transform.localPosition += new Vector3(0,15f*Time.deltaTime,0);
            }
        }
    }
}
