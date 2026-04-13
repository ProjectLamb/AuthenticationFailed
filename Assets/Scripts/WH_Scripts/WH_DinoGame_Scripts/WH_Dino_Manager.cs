using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

public class WH_Dino_Manager : MonoBehaviourPunCallbacks
{
    [Header("게임 루트")]
    public GameObject miniGameRoot;

    [Header("뷰")]
    public GameObject p1View;
    public GameObject p2View;

    [Header("플레이어 공룡")]
    public GameObject p1Dino;
    public GameObject p2Dino;

    [Header("시작 전 UI")]
    public GameObject closeAlertUI;
    public GameObject howToPlayUI;
    public TMP_Text readyCountText;   // 0/2, 1/2, 2/2 표시용

    [Header("결과 UI")]
    public GameObject successUI_P1;
    public GameObject successUI_P2;
    public GameObject failUI_P1;
    public GameObject failUI_P2;

    private bool isGameOver = false;
    private bool isGameStarted = false;

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SetupInitialView();
            SetupInitialUI();
        }
    }

    private void SetupInitialView()
    {
        bool isMaster = PhotonNetwork.IsMasterClient;

        if (p1View != null) p1View.SetActive(isMaster);
        if (p2View != null) p2View.SetActive(!isMaster);

        if (isMaster)
        {
            if (p1Dino != null)
                p1Dino.GetComponent<PhotonView>()?.RequestOwnership();
        }
        else
        {
            if (p2Dino != null)
                p2Dino.GetComponent<PhotonView>()?.RequestOwnership();
        }
    }

    private void SetupInitialUI()
    {
        isGameOver = false;
        isGameStarted = false;

        if (miniGameRoot != null) miniGameRoot.SetActive(false);

        if (successUI_P1 != null) successUI_P1.SetActive(false);
        if (successUI_P2 != null) successUI_P2.SetActive(false);
        if (failUI_P1 != null) failUI_P1.SetActive(false);
        if (failUI_P2 != null) failUI_P2.SetActive(false);

        // 이제 둘 다 경고창을 봄
        if (closeAlertUI != null) closeAlertUI.SetActive(true);

        // 플레이 방법 창은 처음엔 끔
        if (howToPlayUI != null) howToPlayUI.SetActive(false);

        UpdateReadyCountUI(0, 2);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && p2Dino != null)
        {
            PhotonView pv = p2Dino.GetComponent<PhotonView>();
            if (pv != null)
            {
                pv.TransferOwnership(newPlayer);
                Debug.Log($"<color=cyan>P2 소유권을 {newPlayer.NickName}에게 강제 전송함</color>");
            }
        }
    }

    // 경고창 확인 -> 플레이방법 창 열기
    public void OpenHowToPlay()
    {
        if (closeAlertUI != null) closeAlertUI.SetActive(false);
        if (howToPlayUI != null) howToPlayUI.SetActive(true);

        Debug.Log("[Dino] 경고창 종료 -> 플레이 방법 창 오픈");
    }

    public void UpdateReadyCountUI(int current, int total)
    {
        if (readyCountText != null)
        {
            readyCountText.text = $"{current}/{total}";
        }
    }

    // RPC로 전체 클라이언트에서 호출될 실제 시작 처리
    public void StartGameByNetwork()
    {
        if (isGameStarted) return;

        isGameStarted = true;
        isGameOver = false;

        if (closeAlertUI != null) closeAlertUI.SetActive(false);
        if (howToPlayUI != null) howToPlayUI.SetActive(false);
        if (miniGameRoot != null) miniGameRoot.SetActive(true);

        Debug.Log($"<color=yellow>Dino Game Started / IsMaster={PhotonNetwork.IsMasterClient}</color>");
    }

    public void OnSuccess()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (PhotonNetwork.IsMasterClient)
        {
            if (successUI_P1 != null) successUI_P1.SetActive(true);
        }
        else
        {
            if (successUI_P2 != null) successUI_P2.SetActive(true);
        }

        StartCoroutine(DisableGameRoutine(2f));
    }

    public void OnFailure()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (PhotonNetwork.IsMasterClient)
        {
            if (failUI_P1 != null) failUI_P1.SetActive(true);
        }
        else
        {
            if (failUI_P2 != null) failUI_P2.SetActive(true);
        }

        StartCoroutine(DisableGameRoutine(2f));
    }

    private IEnumerator DisableGameRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (miniGameRoot != null)
            miniGameRoot.SetActive(false);
    }
}