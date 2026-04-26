using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JW_SprintConnect_RpcManager : MonoBehaviourPun
{
    [Header("게임 관련 UI")]
    public Slider sprintSlider;
    public Button sprintBtnP1;
    public Button sprintBtnP2;
    public TextMeshProUGUI timerText;

    public GameObject sprintConnectGame;
    public GameObject successAlert;
    public GameObject failAlert;

    public Canvas canvasGame;

    public string status = "P1";
    public bool IsClear = false;
    private int limitTime = 30;

    [Header("알림창,경고")]
    public Canvas canvasHtp;
    public GameObject alert;
    public GameObject htp;

    [Header("준비여부")]
    private bool IsDesktopOK = false;
    private bool IsMoblieOK = false;
    private bool IsAllOK = false;
    [Header("준비여부")]
    private bool IsDesktopReady = false;
    private bool IsMoblieReady = false;
    private bool IsAllReady = false;

    void Awake()
    {
        canvasGame.worldCamera = Camera.main;
    }
    void Update()
    {
        // 슬라이더 체크는 마스터만 하고 결과를 RPC로 쏘거나, 
        // 혹은 모든 클라이언트가 각자 체크하되 성공 처리는 한 번만 실행되게 합니다.
        if (!IsClear && sprintSlider.value >= 1f)
        {
            IsClear = true;
            photonView.RPC("RpcOpenSuccessAlert", RpcTarget.All);
        }
    }

    // [버튼 클릭 이벤트] P1 버튼을 눌렀을 때
    public void OnClickP1()
    {
        if (status == "P1")
            photonView.RPC("RpcChangeStatus", RpcTarget.All, "P2");
        else
            photonView.RPC("RpcResetSlider", RpcTarget.All);
    }

    // [버튼 클릭 이벤트] P2 버튼을 눌렀을 때
    public void OnClickP2()
    {
        if (status == "P2")
            photonView.RPC("RpcChangeStatus", RpcTarget.All, "P1");
        else
            photonView.RPC("RpcResetSlider", RpcTarget.All);
    }

    [PunRPC]
    void RpcChangeStatus(string nextStatus)
    {
        status = nextStatus;
        sprintSlider.value += 0.02f;
        UpdateUI();
    }

    [PunRPC]
    void RpcResetSlider()
    {
        sprintSlider.value = 0f;
    }

    // 모든 플레이어의 화면에서 UI를 갱신하는 함수
    void UpdateUI()
    {
        // P1 UI 업데이트 (객체가 활성화 되어 있을 때만 접근하거나, 혹은 항상 접근 가능하게 두어야 함)
        SetButtonStyle(sprintBtnP1, status == "P1");
        // P2 UI 업데이트
        SetButtonStyle(sprintBtnP2, status == "P2");
    }

    void SetButtonStyle(Button btn, bool isMyTurn)
    {
        if (btn == null) return;

        Image img = btn.GetComponent<Image>();
        // 버튼 자식에 있는 TMP를 찾거나 직접 참조 변수를 만드세요.
        TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();

        if (isMyTurn)
        {
            img.color = new Color32(0, 255, 23, 255);
            if (txt != null) txt.text = "누르세요!";
        }
        else
        {
            img.color = new Color32(255, 64, 0, 255);
            if (txt != null) txt.text = "기다려!";
        }
    }

    // --- 알림 및 타이머 로직 ---

    [PunRPC]
    void RpcSyncTimer(int networkedTime)
    {
        limitTime = networkedTime;
        timerText.text = limitTime.ToString();
        if (limitTime <= 0 && !IsClear) RpcOpenFailAlert();
    }

    [PunRPC]
    void RpcOpenSuccessAlert()
    {
        IsClear = true;
        GameManager.Instance.ClearMiniGame();
        GameManager.Instance.stageNumber += 1;
        RegisterManager.Instance.IsClear = true;
        successAlert.SetActive(true);
    }

    [PunRPC]
    public void RpcOpenFailAlert()
    {
        GameManager.Instance.stageNumber = 0;
        failAlert.SetActive(true);
    }

    public void CloseSuccessAlert()
    {
        successAlert.SetActive(false);
        photonView.RPC("CloseGame", RpcTarget.All);
    }

    public void CloseFailAlert()
    {
        failAlert.SetActive(false);
        photonView.RPC("CloseGame", RpcTarget.All);
    }

    [PunRPC]
    void CloseGame()
    {
        Destroy(this.gameObject);
    }


    IEnumerator TimeLimit()
    {
        while (limitTime > 0 && !IsClear)
        {
            yield return new WaitForSeconds(1f);
            limitTime--;
            photonView.RPC("RpcSyncTimer", RpcTarget.AllBuffered, limitTime);
        }
    }
    public void OK()
    {
        photonView.RPC("RpcSetOK", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void RpcSetOK(int actorNumber)
    {
        if (actorNumber == 1) IsDesktopOK = true;
        else if (actorNumber == 2) IsMoblieOK = true;

        Debug.Log($"Player {actorNumber} OK!");

        // 마스터 클라이언트만 시작 조건을 체크하는 것이 안전합니다.
        if (PhotonNetwork.IsMasterClient)
        {
            CheckOK();
        }
    }
    void CheckOK()
    {
        if (IsDesktopOK && IsMoblieOK && !IsAllOK)
        {
            IsAllOK = true;
            photonView.RPC("RpcHtp", RpcTarget.All);
        }
    }

    [PunRPC]
    void RpcHtp()
    {
        alert.SetActive(false);
        htp.SetActive(true);
    }

    // 버튼을 눌렀을 때 호출되는 함수
    public void Ready()
    {
        // 내 ActorNumber에 따라 상대방에게도 내 상태를 알립니다.
        // RpcTarget.All로 보내야 나를 포함한 모든 사람의 IsReady 변수가 갱신됩니다.
        photonView.RPC("RpcSetReady", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void RpcSetReady(int actorNumber)
    {
        if (actorNumber == 1) IsDesktopReady = true;
        else if (actorNumber == 2) IsMoblieReady = true;

        Debug.Log($"Player {actorNumber} Ready!");

        // 마스터 클라이언트만 시작 조건을 체크하는 것이 안전합니다.
        if (PhotonNetwork.IsMasterClient)
        {
            CheckStartCondition();
        }
    }

    void CheckStartCondition()
    {
        if (IsDesktopReady && IsMoblieReady && !IsAllReady)
        {
            IsAllReady = true;
            photonView.RPC("RpcGameStart", RpcTarget.All);
        }
    }

    [PunRPC]
    void RpcGameStart()
    {
        canvasHtp.enabled = false;
        canvasGame.enabled = true;

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(TimeLimit());
        }

        // 로컬 플레이어에 따라 버튼 감추기보다는 '상호작용 가능 여부'로 제어하는 것이 안전합니다.
        // 만약 버튼 객체 자체가 할당되지 않았다면 에러가 나므로, 인스펙터에 두 버튼 모두 연결되어 있어야 합니다.
        sprintBtnP1.gameObject.SetActive(PhotonNetwork.LocalPlayer.ActorNumber == 1);
        sprintBtnP2.gameObject.SetActive(PhotonNetwork.LocalPlayer.ActorNumber == 2);

        // 초기 UI 상태 설정 (RPC를 통해 동기화된 상태로 시작)
        UpdateUI();
    }
}