using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class WH_CipherManager : MonoBehaviourPun
{
    [SerializeField] private bool forceP1 = true;

    [Header("Game Root & UI Groups")]
    public GameObject miniGameRoot;
    public GameObject[] gameplayUIs; // 실제 게임 진행용 UI들

    [Header("Views")]
    public GameObject p1View;
    public GameObject p2View;

    [Header("Game Settings")]
    public string[] targetWords = { "APPROVED", "REGISTER", "VALIDATE", "SECURITY", "PROTOCOL" };
    public float timeLimit = 30f;

    [Header("UI References (Standard UI)")]
    public TMP_Text targetWordDisplay;
    public InputField p1InputField;
    public TMP_Text[] timerDisplays;
    public TMP_Text[] resultDisplays;

    [Header("UI References (TMP)")]
    public TextMeshProUGUI mappingDisplay;

    private string currentTargetWord;
    private Dictionary<char, char> cipherMap = new Dictionary<char, char>();

    private bool isGameActive = false;
    private float currentTime;
    private bool isP1;

    [Header("Ready & Help UI")]
    public GameObject readyPanel;  // '준비' 버튼이 있는 경고창(Alert)
    public GameObject howToPlayUI; // '도움말' 창
    private HashSet<int> readyPlayers = new HashSet<int>();
    private bool isReady = false;

    // ----------------------------------------------------------------
    // 1. 초기화 및 시작 대기
    // ----------------------------------------------------------------

    void Start()
    {
        // 1. 뷰 설정 (P1인지 P2인지 결정)
        SetupView();

        // 2. 초기 상태 설정: 게임 대기
        isGameActive = false;
        isReady = false;
        readyPlayers.Clear();

        // 3. 게임 UI들은 모두 꺼두고 준비 패널(Alert)만 켜둡니다.
        foreach (var ui in gameplayUIs) if (ui != null) ui.SetActive(false);
        if (readyPanel != null) readyPanel.SetActive(true);

        // ★ 중요: Start()에서 InitGameLogic을 호출하던 기존 코드를 삭제했습니다.
    }

    void SetupView()
    {
        isP1 = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient : forceP1;
        if (p1View != null) p1View.SetActive(isP1);
        if (p2View != null) p2View.SetActive(!isP1);
    }

    // ----------------------------------------------------------------
    // 2. 준비 및 시작 시스템 (Logic 재구성)
    // ----------------------------------------------------------------

    public void OnClickReady()
    {
        if (isReady) return;

        isReady = true;
        Debug.Log("<color=cyan>[Cipher] 준비 완료 버튼 클릭</color>");

        if (PhotonNetwork.IsConnected)
        {
            // 마스터 클라이언트에게 내 준비 상태를 보고
            photonView.RPC(nameof(RPC_SetPlayerReady), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        else if (forceP1)
        {
            // 오프라인 테스트 시 즉시 시작
            InitGameLogic(true);
        }
    }

    [PunRPC]
    void RPC_SetPlayerReady(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        readyPlayers.Add(actorNumber);

        int required = PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log($"[Cipher] 준비 인원 체크: {readyPlayers.Count} / {required}");

        // 모두가 준비되었다면 마스터가 게임 데이터를 생성하고 뿌립니다.
        if (readyPlayers.Count >= required)
        {
            InitGameLogic(true);
        }
    }

    // 방장이 암호표를 만들어서 동기화시키는 함수
    void InitGameLogic(bool resetTimer)
    {
        int randomIndex = Random.Range(0, targetWords.Length);
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string shuffled = new string(alphabet.ToCharArray().OrderBy(x => System.Guid.NewGuid()).ToArray());

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

        if (resetTimer) currentTime = timeLimit;

        // 실제 게임 UI 활성화 및 준비창 닫기
        StartGameUI();
    }

    void StartGameUI()
    {
        isGameActive = true;

        // 준비 패널과 도움말 창을 모두 닫습니다.
        if (readyPanel != null) readyPanel.SetActive(false);
        if (howToPlayUI != null) howToPlayUI.SetActive(false);

        // 게임용 UI 켜기
        foreach (var ui in gameplayUIs) if (ui != null) ui.SetActive(true);
        foreach (var res in resultDisplays) if (res != null) res.text = "";

        if (isP1)
        {
            if (targetWordDisplay != null) targetWordDisplay.text = $"해독 대상: {currentTargetWord}";
            if (p1InputField != null) p1InputField.text = "";
        }
        else
        {
            // P2 해독표 업데이트
            var p2Script = GetComponentInChildren<WH_CipherP2>();
            if (p2Script != null) p2Script.UpdateMappingDisplay(cipherMap);
            else if (mappingDisplay != null) UpdateMappingTextFallback();
        }
    }

    // ----------------------------------------------------------------
    // 3. 도움말 및 기타 기능
    // ----------------------------------------------------------------

    public void OnClickOpenHowToPlay() { if (howToPlayUI != null) howToPlayUI.SetActive(true); }
    public void OnClickCloseHowToPlay() { if (howToPlayUI != null) howToPlayUI.SetActive(false); }

    public char GetMappedChar(char input)
    {
        char upperInput = char.ToUpper(input);
        if (cipherMap != null && cipherMap.ContainsKey(upperInput)) return cipherMap[upperInput];
        return input;
    }

    private void UpdateMappingTextFallback()
    {
        if (mappingDisplay == null) return;
        string mapText = "<b>[ DECODE TABLE ]</b>\n";
        foreach (var pair in cipherMap) mapText += $"{pair.Key} → {pair.Value}  ";
        mappingDisplay.text = mapText;
    }

    // ----------------------------------------------------------------
    // 4. 업데이트 및 판정 로직 (기존 유지)
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

    public void OnClickVerify() { if (p1InputField != null) VerifyAnswer(p1InputField.text); }

    public void VerifyAnswer(string typedWord)
    {
        if (!isGameActive) return;

        if (typedWord.ToUpper() == currentTargetWord.ToUpper())
            photonView.RPC(nameof(RPC_EndGame), RpcTarget.All, true, "ACCESS GRANTED");
        else
            photonView.RPC(nameof(RPC_HandleFailure), RpcTarget.All);
    }

    [PunRPC]
    void RPC_HandleFailure()
    {
        foreach (var res in resultDisplays) if (res != null) res.text = "WRONG PASSWORD - RESETTING...";
        StartCoroutine(ResetSequence());
    }

    IEnumerator ResetSequence()
    {
        yield return new WaitForSeconds(1f);
        if (isP1 && p1InputField != null) { p1InputField.text = ""; p1InputField.ActivateInputField(); }
        if (PhotonNetwork.IsMasterClient) InitGameLogic(false);
    }

    [PunRPC]
    void RPC_EndGame(bool success, string message) { StartCoroutine(EndGameRoutine(success, message)); }

    IEnumerator EndGameRoutine(bool success, string message)
    {
        isGameActive = false;
        foreach (var res in resultDisplays)
        {
            if (res != null)
            {
                res.text = message;
                res.color = success ? Color.cyan : Color.red;
            }
        }
        yield return new WaitForSeconds(2f);
        if (miniGameRoot != null) miniGameRoot.SetActive(false);
    }
}