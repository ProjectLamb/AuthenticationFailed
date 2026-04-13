using UnityEngine;
using System.Collections;
using Photon.Pun;
using TMPro;

public class WH_GameManager : MonoBehaviourPunCallbacks
{
    [Header("게임 루트")]
    public GameObject miniGameRoot;

    [Header("뷰")]
    public GameObject p1View;
    public GameObject p2View;

    [Header("시작 전 UI")]
    public GameObject closeAlertUI;
    public GameObject howToPlayUI;
    public TMP_Text readyCountText;

    [Header("결과 UI")]
    public GameObject virusP1;
    public GameObject virusP2;
    public GameObject clearP1;
    public GameObject clearP2;

    [Header("P2 Life UI")]
    public GameObject[] p2LifeIcons; // 3칸 이미지 연결

    private bool isGameStarted = false;
    private bool isGameEnded = false;

    void Start()
    {
        SetupInitialView();
        SetupInitialUI();
    }

    private void SetupInitialView()
    {
        bool isMaster = PhotonNetwork.IsMasterClient;

        if (p1View != null) p1View.SetActive(isMaster);
        if (p2View != null) p2View.SetActive(!isMaster);
    }

    private void SetupInitialUI()
    {
        isGameStarted = false;
        isGameEnded = false;

        if (miniGameRoot != null) miniGameRoot.SetActive(false);

        if (closeAlertUI != null) closeAlertUI.SetActive(true);
        if (howToPlayUI != null) howToPlayUI.SetActive(false);

        if (virusP1 != null) virusP1.SetActive(false);
        if (virusP2 != null) virusP2.SetActive(false);
        if (clearP1 != null) clearP1.SetActive(false);
        if (clearP2 != null) clearP2.SetActive(false);

        UpdateReadyCountUI(0, 2);
        UpdateLifeUI(3);
    }

    public void OpenHowToPlay()
    {
        if (closeAlertUI != null) closeAlertUI.SetActive(false);
        if (howToPlayUI != null) howToPlayUI.SetActive(true);

        Debug.Log("[VirusGame] 경고창 종료 -> 플레이방법 창 오픈");
    }

    public void UpdateReadyCountUI(int current, int total)
    {
        if (readyCountText != null)
        {
            readyCountText.text = $"{current}/{total}";
        }
    }

    public void UpdateLifeUI(int currentLife)
    {
        if (p2LifeIcons == null || p2LifeIcons.Length == 0) return;

        for (int i = 0; i < p2LifeIcons.Length; i++)
        {
            if (p2LifeIcons[i] != null)
            {
                p2LifeIcons[i].SetActive(i < currentLife);
            }
        }

        Debug.Log($"[VirusGame] P2 Life UI 갱신: {currentLife}");
    }

    public void StartGameByNetwork()
    {
        if (isGameStarted) return;

        isGameStarted = true;
        isGameEnded = false;

        if (closeAlertUI != null) closeAlertUI.SetActive(false);
        if (howToPlayUI != null) howToPlayUI.SetActive(false);
        if (miniGameRoot != null) miniGameRoot.SetActive(true);

        UpdateLifeUI(3);

        Debug.Log($"<color=yellow>[VirusGame] 미니게임 시작 / IsMaster={PhotonNetwork.IsMasterClient}</color>");
    }

    public void TriggerVirusPenalty()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        StartCoroutine(VirusSequence());
    }

    public void TriggerStageClear()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        StartCoroutine(ClearSequence());
    }

    private IEnumerator VirusSequence()
    {
        SetResultUI(virusP1, virusP2, true);

        yield return new WaitForSeconds(2.0f);

        if (miniGameRoot != null) miniGameRoot.SetActive(false);
    }

    private IEnumerator ClearSequence()
    {
        SetResultUI(clearP1, clearP2, true);

        yield return new WaitForSeconds(2.0f);

        if (miniGameRoot != null) miniGameRoot.SetActive(false);
    }

    private void SetResultUI(GameObject p1Result, GameObject p2Result, bool state)
    {
        if (p1Result != null) p1Result.SetActive(state);
        if (p2Result != null) p2Result.SetActive(state);

        if (p1View != null && p1Result != null) HideOthers(p1View.transform, p1Result);
        if (p2View != null && p2Result != null) HideOthers(p2View.transform, p2Result);
    }

    private void HideOthers(Transform parent, GameObject target)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject != target)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}