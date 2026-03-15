using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

// MonoBehaviourPunCallbacks를 상속받아야 네트워크 이벤트를 감지합니다.
public class WH_Dino_Manager : MonoBehaviourPunCallbacks
{
    public GameObject miniGameRoot;
    public GameObject p1View, p2View;
    public GameObject p1Dino, p2Dino;
    public GameObject successUI_P1, successUI_P2;
    public GameObject failUI_P1, failUI_P2;

    private bool isGameOver = false;

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SetupInitialView();
        }
    }

    private void SetupInitialView()
    {
        bool isMaster = PhotonNetwork.IsMasterClient;

        // 1. 화면 분리
        p1View.SetActive(isMaster);
        p2View.SetActive(!isMaster);

        // 2. [핵심] 자기 역할에 맞는 공룡 소유권 주장
        // 방장은 P1을, 참가자는 P2의 주권을 가져옵니다.
        if (isMaster)
            p1Dino.GetComponent<PhotonView>().RequestOwnership();
        else
            p2Dino.GetComponent<PhotonView>().RequestOwnership();
    }

    // 혹시 모르니 참가자가 들어왔을 때 방장이 한 번 더 넘겨줍니다.
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            p2Dino.GetComponent<PhotonView>().TransferOwnership(newPlayer);
            Debug.Log($"<color=cyan>P2 소유권을 {newPlayer.NickName}에게 강제 전송함</color>");
        }
    }

    public void OnSuccess()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (PhotonNetwork.IsMasterClient) successUI_P1.SetActive(true);
        else successUI_P2.SetActive(true);
        StartCoroutine(DisableGameRoutine(2f));
    }

    public void OnFailure()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (PhotonNetwork.IsMasterClient) failUI_P1.SetActive(true);
        else failUI_P2.SetActive(true);
        StartCoroutine(DisableGameRoutine(2f));
    }

    private IEnumerator DisableGameRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (miniGameRoot != null) miniGameRoot.SetActive(false);
    }
}