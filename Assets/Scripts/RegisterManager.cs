using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using Photon.Pun;

public class RegisterManager : MonoBehaviourPun
{

    private AudioSource audioSource;
    public AudioClip alertSound;

    public GameObject regWindow;
    public GameObject alert;


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void RegClick()
    {
        // 내 화면만 바꾸는 게 아니라, 모든 사람(RpcTarget.All)에게 실행하라고 명령합니다.
        photonView.RPC("RpcRegClick", RpcTarget.All);
    }
    [PunRPC]
    void RpcRegClick()
    {
        alert.SetActive(true);
        audioSource.PlayOneShot(alertSound);
    }

    public void LauncherClick()
    {
        // 창이 뜨는 것도 모든 사람에게 공유합니다.
        photonView.RPC("RpcLauncherClick", RpcTarget.All);
    }

    [PunRPC]
    void RpcLauncherClick()
    {
        regWindow.SetActive(true);
        Debug.Log("클릭은 되고있어");
    }

    public void StartMiniGame()
    {
        alert.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.MiniGame();
        }
    }
}
