using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI; // 일반 InputField와 Text를 사용하기 위해 필요
using TMPro;        // 해독표 등 TMP_Text를 사용하는 곳이 있다면 유지
using Photon.Pun;

public class WH_CipherManager : MonoBehaviourPun
{
    [SerializeField] private bool forceP1 = true;

    [Header("Game Root & UI Groups")]
    public GameObject miniGameRoot;
    public GameObject[] gameplayUIs;

    [Header("Views")]
    public GameObject p1View;
    public GameObject p2View;

    [Header("Game Settings")]
    public string[] targetWords = { "APPROVED", "REGISTER", "VALIDATE", "SECURITY", "PROTOCOL" };
    public float timeLimit = 30f;

    [Header("UI References (Standard UI)")]
    public TMP_Text targetWordDisplay;      // 일반 Text 사용 시 (TMP라면 TextMeshProUGUI로 변경)
    public InputField p1InputField;    // ★ 일반 InputField로 변경
    public TMP_Text[] timerDisplays;       // 일반 Text 배열
    public TMP_Text[] resultDisplays;      // 일반 Text 배열

    [Header("UI References (TMP - 필요시)")]
    public TextMeshProUGUI mappingDisplay; // P2 해독표는 TMP가 가독성이 좋아 유지 (일반 Text로 변경 가능)

    private string currentTargetWord;
    private Dictionary<char, char> cipherMap = new Dictionary<char, char>();

    private bool isGameActive = false;
    private float currentTime;
    private bool isP1;

    [Header("Intro UI")]
    public GameObject closeAlertUI;
    public GameObject howToPlayUI;


    private bool p1Ready = false;
    private bool p2Ready = false;
    private bool gameStarted = false;

    [Header("User Count UI")]
    public TMP_Text userCountText;

    public GameObject GameCanvas;
    // ----------------------------------------------------------------
    // 1. 초기화 및 뷰 설정
    // ----------------------------------------------------------------

    void Start()
    {
        SetupView();

        if (closeAlertUI != null) closeAlertUI.SetActive(true);
        if (howToPlayUI != null) howToPlayUI.SetActive(false);

        UpdateUserCount();
    }

    void UpdateUserCount()
    {
        if (userCountText == null) return;

        int current = PhotonNetwork.CurrentRoom != null
            ? PhotonNetwork.CurrentRoom.PlayerCount
            : 1;

        userCountText.text = $"{current} / 2";
    }

    public void OnClickOpenHowToPlay()
    {
        if (closeAlertUI != null) closeAlertUI.SetActive(false);
        if (howToPlayUI != null) howToPlayUI.SetActive(true);
    }
    
   
    public void OnClickHowToPlayReady()
    {
        photonView.RPC(nameof(RPC_SetReady), RpcTarget.MasterClient);
    }

    [PunRPC]
    void RPC_SetReady(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (info.Sender.IsMasterClient)
            p1Ready = true;
        else
            p2Ready = true;

        Debug.Log($"Ready 상태 → P1:{p1Ready}, P2:{p2Ready}");

        if (p1Ready && p2Ready && !gameStarted)
        {
            gameStarted = true;

            photonView.RPC(nameof(RPC_StartGame), RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_StartGame()
    {
        // 설명 UI 닫기
        if (howToPlayUI != null) howToPlayUI.SetActive(false);

        // 기존 게임 시작 로직 그대로 사용

        InitGameLogic(true);
        GameCanvas.SetActive(true);
    }
    void SetupView()
    {
        isP1 = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient : forceP1;

        if (p1View != null) p1View.SetActive(isP1);
        if (p2View != null) p2View.SetActive(!isP1);
    }

    // ----------------------------------------------------------------
    // 2. 게임 로직 동기화 (방장 실행)
    // ----------------------------------------------------------------

    void InitGameLogic(bool resetTimer)
    {
        int randomIndex = Random.Range(0, targetWords.Length);
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string shuffled = new string(alphabet.ToCharArray().OrderBy(x => System.Guid.NewGuid()).ToArray());

        // RPC로 넘겨줄 때 resetTimer 값도 함께 보냅/니다.
        photonView.RPC(nameof(RPC_SyncGameStart), RpcTarget.All, randomIndex, shuffled, resetTimer);
    }

    [PunRPC]
    void RPC_SyncGameStart(int wordIndex, string shuffledAlphabet, bool resetTimer)
    {
        currentTargetWord = targetWords[wordIndex];

        cipherMap.Clear();
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < alphabet.Length; i++)
        {
            cipherMap.Add(alphabet[i], shuffledAlphabet[i]);
        }

        // 시간 리셋 여부에 따라 처리를 다르게 합니다.
        if (resetTimer)
        {
            currentTime = timeLimit;
        }

        StartGameUI(resetTimer);
    }

    // StartGameUI에서도 시간을 건드리지 않도록 수정
    void StartGameUI(bool isInitialStart)
    {
        isGameActive = true;

        // 만약 최초 시작(isInitialStart == true)일 때만 시간을 세팅하고 싶다면 여기서 처리해도 됩니다.
        // 하지만 이미 RPC 함수에서 currentTime을 건드렸으므로 여기서는 UI 활성화만 담당합니다.

        foreach (var ui in gameplayUIs) if (ui != null) ui.SetActive(true);
        foreach (var res in resultDisplays) if (res != null) res.text = "";

        if (isP1)
        {
            if (targetWordDisplay != null) targetWordDisplay.text = $"해독 대상: {currentTargetWord}";
            if (p1InputField != null) p1InputField.text = "";
        }
        else
        {
            var p2Script = GetComponentInChildren<WH_CipherP2>();
            if (p2Script != null) p2Script.UpdateMappingDisplay(cipherMap);
        }
    }

    // ----------------------------------------------------------------
    // 3. ★ P1 스크립트 에러 해결을 위한 핵심 함수 ★
    // ----------------------------------------------------------------

    public char GetMappedChar(char input)
    {
        char upperInput = char.ToUpper(input);

        // 생성된 암호표에 있으면 변환값을, 없으면 원래 글자 반환
        if (cipherMap != null && cipherMap.ContainsKey(upperInput))
        {
            return cipherMap[upperInput];
        }
        return input;
    }

    // ----------------------------------------------------------------
    // 4. 게임 진행 및 종료 로직
    // ----------------------------------------------------------------

    void Update()
    {
        if (!isGameActive) return;

        currentTime -= Time.deltaTime;
        string timerString = $"TIME: {Mathf.CeilToInt(currentTime)}s";

        foreach (var timer in timerDisplays)
        {
            if (timer != null && timer.gameObject.activeInHierarchy)
                timer.text = timerString;
        }

        if (currentTime <= 0 && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_EndGame), RpcTarget.All, false, "TIMEOUT!");
        }
    }

    // -------------------- VERIFY & RESET LOGIC --------------------

    public void VerifyAnswer(string typedWord)
    {
        if (!isGameActive) return;

        // 대소문자 구분 없이 비교
        if (typedWord.ToUpper() == currentTargetWord.ToUpper())
        {
            // [성공] 모든 플레이어에게 성공 알림 및 게임 종료
            photonView.RPC(nameof(RPC_EndGame), RpcTarget.All, true, "ACCESS GRANTED");
        }
        else
        {
            // [실패] 태초마을로 보내기 (방장이 주도해서 다시 섞음)
            Debug.Log("<color=red>보안 인증 실패: 데이터를 초기화합니다.</color>");
            photonView.RPC(nameof(RPC_HandleFailure), RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_HandleFailure()
    {
        // 1. 결과창에 실패 메시지 잠시 띄우기
        foreach (var res in resultDisplays)
        {
            if (res != null) res.text = "WRONG PASSWORD - RESETTING...";
        }

        // 2. 사운드나 연출을 위해 잠시 대기 후 초기화 실행
        StartCoroutine(ResetSequence());
    }

    IEnumerator ResetSequence()
    {
        yield return new WaitForSeconds(1f);

        if (isP1 && p1InputField != null)
        {
            p1InputField.text = "";
            p1InputField.ActivateInputField();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // [핵심] 실패해서 다시 섞을 때는 false를 전달하여 시간을 유지합니다.
            InitGameLogic(false);
        }
    }

    // 인스펙터 버튼용
    public void OnClickVerify()
    {
        if (p1InputField != null) VerifyAnswer(p1InputField.text);
    }

    [PunRPC]
    void RPC_EndGame(bool success, string message)
    {
        StartCoroutine(EndGameRoutine(success, message));
    }

    IEnumerator EndGameRoutine(bool success, string message)
    {
        isGameActive = false;

        foreach (var res in resultDisplays)
        {
            if (res != null && res.gameObject.activeInHierarchy)
            {
                res.text = message;
                res.color = success ? Color.cyan : Color.red;
            }
        }

        yield return new WaitForSeconds(2f);

        if (miniGameRoot != null) miniGameRoot.SetActive(false);
    }


}