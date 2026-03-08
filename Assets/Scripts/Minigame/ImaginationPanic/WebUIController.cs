using System.Collections;
using UnityEngine;
using UnityEngine.UI;       // Button은 기존 UI 네임스페이스 사용
using UnityEngine.Networking;
using TMPro;                // [추가] TextMeshPro 네임스페이스

// 1. JSON 데이터를 담을 클래스
[System.Serializable]
public class AIResponseData
{
    public int shapeIndex;
    public int matIndex;
    public float scaleX;
    public float scaleY;
    public float scaleZ;
    public string replyMessage;
}

public class WebUIController : MonoBehaviour
{
    [Header("UI 연결 (TMP)")]
    public TMP_InputField chatInput;        // [수정] TMP용 InputField
    public Button sendButton;               // 버튼은 그대로
    public TextMeshProUGUI chatHistoryText; // [수정] TMP용 Text

    [Header("스폰 시스템 연결")]
    public AIObjectSpawner spawner;

    // 홈서버 API 주소
    private string apiUrl = "https://authfailed.neoskyclad.com/api/v1/chat";

    void Start()
    {
        sendButton.onClick.AddListener(OnSendButtonClicked);
        chatHistoryText.text = "[시스템]: 이메지네이션 패닉 터미널에 접속했습니다.\n원하는 구조물을 묘사하세요.\n";
    }

    void OnSendButtonClicked()
    {
        if (string.IsNullOrEmpty(chatInput.text)) return;

        string userMessage = chatInput.text;

        chatHistoryText.text += $"\n[2P]: {userMessage}";
        chatInput.text = ""; // 입력창 비우기

        sendButton.interactable = false;
        StartCoroutine(SendToAI(userMessage));
    }

    IEnumerator SendToAI(string message)
    {
        string jsonPayload = $"{{\"message\":\"{message}\", \"minigame_type\":\"이메지네이션 패닉\", \"target_code\":\"\"}}";

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            chatHistoryText.text += "\n[AI]: 데이터 분석 중...";

            yield return request.SendWebRequest();

            // 응답이 오면 로딩 텍스트 지우기
            chatHistoryText.text = chatHistoryText.text.Replace("\n[AI]: 데이터 분석 중...", "");

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"통신 에러: {request.error}");
                chatHistoryText.text += "\n[시스템 Error]: API 서버 응답 없음!";
            }
            else
            {
                string responseText = request.downloadHandler.text;

                // 마크다운 제거 (안전장치)
                responseText = responseText.Replace("```json", "").Replace("```", "").Trim();

                try
                {
                    AIResponseData data = JsonUtility.FromJson<AIResponseData>(responseText);

                    chatHistoryText.text += $"\n[AI]: {data.replyMessage}\n";
                    spawner.RequestSpawnFromAI(data.shapeIndex, data.matIndex, data.scaleX, data.scaleY, data.scaleZ);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"JSON 파싱 에러: {e.Message}\n원본 데이터: {responseText}");
                    chatHistoryText.text += "\n[시스템 Error]: AI 응답 규격 불일치!";
                }
            }

            sendButton.interactable = true;
        }
    }
}