using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class WH_RegisterManager : MonoBehaviourPun
{
    private AudioSource audioSource;

    [Header("사운드")]
    public AudioClip alertSound;

    [Header("역할 구분")]
    public bool isDesktop; // P1(Desktop)=true, P2(Mobile)=false

    [Header("각 스테이지 오브젝트 모음")]
    public GameObject agreeWindow;
    public GameObject regWindow;

    [Header("인증서 단계 UI")]
    public GameObject p1CertWindow;      // ★ 반드시 인증창 루트 전체를 넣기
    public InputField p1AuthInput;       // P1 인증번호 입력창
    public GameObject p2NoticeBar;       // P2 상단 알림바
    public GameObject p2CertWindow;      // P2 메시지 상세창
    public TextMeshProUGUI p2CodeText;   // P2에서 보여줄 인증번호 텍스트

    [Header("정보, 알림창")]
    public GameObject alert;
    public GameObject howtoplay;

    private static string generatedCode = "";

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        HideAllCertUI();
    }

    void Start()
    {
        // 다른 스크립트/프리팹 초기화 순서 때문에 다시 켜지는 경우 방지
        HideAllCertUI();
    }

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

    // -------------------------
    // 기존 약관 / 등록 단계
    // -------------------------

    public void AgreeClick()
    {
        photonView.RPC(nameof(RpcAgreeClick), RpcTarget.All);
    }

    [PunRPC]
    void RpcAgreeClick()
    {
        if (!GameManager.Instance.IsCanAgree)
        {
            if (alert != null) alert.SetActive(true);
            PlayAlertSound();
        }
        else
        {
            Debug.Log("동의하기 성공");
            if (agreeWindow != null) agreeWindow.SetActive(false);
        }
    }

    public void LauncherClick()
    {
        photonView.RPC(nameof(RpcLauncherClick), RpcTarget.All);
    }

    [PunRPC]
    void RpcLauncherClick()
    {
        switch (GameManager.Instance.stageNumber)
        {
            case 0:
                if (agreeWindow != null)
                    agreeWindow.SetActive(true);
                break;

            case 1:
                if (regWindow != null)
                    regWindow.SetActive(true);
                break;

            default:
                Debug.Log("인증 대기 중...");
                break;
        }
    }

    public void HowToPlay()
    {
        if (alert != null) alert.SetActive(false);
        if (howtoplay != null) howtoplay.SetActive(true);
    }

    public void StartMiniGame()
    {
        if (howtoplay != null)
            howtoplay.SetActive(false);

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