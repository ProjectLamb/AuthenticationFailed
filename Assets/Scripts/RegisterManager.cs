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

    [Header("각 스테이지 오브젝트 모음")]
    public GameObject agreeWindow;
    public GameObject regWindow;

    [Header("정보,알림창")]
    public GameObject alert;
    public GameObject howtoplay;


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void AgreeClick()
    {
        // 내 화면만 바꾸는 게 아니라, 모든 사람(RpcTarget.All)에게 실행하라고 명령합니다.
        photonView.RPC("RpcAgreeClick", RpcTarget.All);
    }
    [PunRPC]
    void RpcAgreeClick()
    {
        if (!GameManager.Instance.IsCanAgree)
        {
            alert.SetActive(true);
            audioSource.PlayOneShot(alertSound);
        }
        else
        {
            Debug.Log("동의하기 성공");
            agreeWindow.SetActive(false);
        }
    }

    public void LauncherClick()
    {
        // 창이 뜨는 것도 모든 사람에게 공유합니다.
        photonView.RPC("RpcLauncherClick", RpcTarget.All);
    }

    [PunRPC]
    void RpcLauncherClick()
    {
        switch (GameManager.Instance.stageNumber)
        {
            case 0:
                agreeWindow.SetActive(true);
                break;
            case 1:
                regWindow.SetActive(true);
                break;
            default:
                Debug.Log("올바른 스테이지 설정이 되어있지 않습니다!");
                break;

        }
    }

    public void HowToPlay()
    {
        alert.SetActive(false);
        howtoplay.SetActive(true);
    }
    public void StartMiniGame()
    {
        howtoplay.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.MiniGame();
        }
    }
}
