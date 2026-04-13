using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using Photon.Pun;
using TMPro;

public class RegisterManager : MonoBehaviourPun
{

    private static RegisterManager instance = null;

    public static RegisterManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("No RegisterManagerInstance");
            }
            return instance;
        }
    }

    private AudioSource audioSource;
    public AudioClip alertSound;

    [Header("각 스테이지 오브젝트 모음")]
    public GameObject agreeWindow;
    public GameObject regWindow;

    [Header("정보,알림창")]
    public GameObject alert;
    public GameObject howtoplay;

    [Header("역할 구분")]
    public bool isDesktop;

    [Header("인증서 단계 UI")]
    public GameObject p1CertWindow;      // ★ 반드시 인증창 루트 전체를 넣기
    public InputField p1AuthInput;       // P1 인증번호 입력창
    public GameObject p2NoticeBar;       // P2 상단 알림바
    public GameObject p2CertWindow;      // P2 메시지 상세창
    public TextMeshProUGUI p2CodeText;   // P2에서 보여줄 인증번호 텍스트

    public bool IsClear = false;

    private static string generatedCode = "";

    void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
        HideAllCertUI();
    }

    public void AgreeClick()
    {
        // 내 화면만 바꾸는 게 아니라, 모든 사람(RpcTarget.All)에게 실행하라고 명령합니다.
        photonView.RPC("RpcAgreeClick", RpcTarget.All);
    }
    [PunRPC]
    void RpcAgreeClick()
    {
        if (!IsClear)
        {
            StartMiniGame();
        }
        else
        {
            Debug.Log("동의하기 성공");
            agreeWindow.SetActive(false);
            IsClear = false;
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
    public void StartMiniGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.MiniGame();
        }
    }
    // -------------------------
    // 미니게임 클리어 후 인증 단계 시작
    // -------------------------

    public void OnMiniGameClear()
    {
        photonView.RPC(nameof(RpcStartCertPhase), RpcTarget.All);
    }

    [PunRPC]
    void RpcStartCertPhase()
    {
        agreeWindow.SetActive(false);
        StartCoroutine(ImmediateCertRoutine());
    }

    IEnumerator ImmediateCertRoutine()
    {
        // 미니게임 종료 직후 UI 정리 시간
        yield return new WaitForSeconds(0.1f);

        HideAllCertUI();

        if (isDesktop)
        {
            if (p1CertWindow != null)
            {
                p1CertWindow.SetActive(true);
                Debug.Log("<color=cyan>P1: 미니게임 클리어 -> 인증창 활성화</color>");
            }
            else
            {
                Debug.LogWarning("P1 Cert Window가 연결되지 않았습니다.");
            }
        }
    }

    // -------------------------
    // P1 -> 인증 요청
    // -------------------------

    public void P1_RequestAuth()
    {
        if (!isDesktop)
            return;

        string newCode = Random.Range(100000, 999999).ToString();
        photonView.RPC(nameof(RpcDeliverCode), RpcTarget.All, newCode);

        Debug.Log($"<color=yellow>P1: 인증번호 생성 {newCode}</color>");
    }

    [PunRPC]
    void RpcDeliverCode(string code)
    {
        generatedCode = code;

        if (!isDesktop)
        {
            if (p2NoticeBar != null)
                p2NoticeBar.SetActive(true);

            if (p2CodeText != null)
                p2CodeText.text = generatedCode;

            Debug.Log($"<color=orange>P2: 인증번호 수신 {generatedCode}</color>");
        }
    }

    // -------------------------
    // P2 -> 메시지 열기
    // -------------------------

    public void P2_OpenMessage()
    {
        if (isDesktop)
            return;

        if (p2NoticeBar != null)
            p2NoticeBar.SetActive(false);

        if (p2CertWindow != null)
            p2CertWindow.SetActive(true);
    }

    // -------------------------
    // P1 -> 최종 인증
    // -------------------------
    private void HideAllCertUI()
    {
        if (p1CertWindow != null)
            p1CertWindow.SetActive(false);

        if (p2NoticeBar != null)
            p2NoticeBar.SetActive(false);

        if (p2CertWindow != null)
            p2CertWindow.SetActive(false);
    }
    private void PlayAlertSound()
    {
        if (audioSource != null && alertSound != null)
        {
            audioSource.PlayOneShot(alertSound);
        }
    }
    public void P1_FinalVerify()
    {
        if (!isDesktop)
            return;

        if (p1AuthInput == null)
        {
            Debug.LogWarning("P1 Auth Input이 연결되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(generatedCode))
        {
            Debug.LogWarning("아직 생성된 인증번호가 없습니다.");
            if (alert != null) alert.SetActive(true);
            PlayAlertSound();
            return;
        }

        if (p1AuthInput.text == generatedCode)
        {
            photonView.RPC(nameof(RpcAuthSuccess), RpcTarget.All);
        }
        else
        {
            Debug.LogWarning($"인증 실패 - 입력값: {p1AuthInput.text}, 실제코드: {generatedCode}");

            if (alert != null) alert.SetActive(true);
            PlayAlertSound();
        }
    }

    [PunRPC]
    void RpcAuthSuccess()
    {
        HideAllCertUI();
        Debug.Log("<color=green>인증 완료!</color>");
    }
}
