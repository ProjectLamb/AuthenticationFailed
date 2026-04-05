using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

// --- [추가] 서버로 보낼 요청(Request) DTO 클래스 ---
[System.Serializable]
public class ChatRequestData
{
    public string message;
    public string minigame_type;
    public string target_code;
}

[System.Serializable]
public class AIResponseData
{
    public bool isSpawning; // [핵심] 스폰 요청인지, 힌트인지 구분
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
    public TMP_InputField chatInput;
    public Button sendButton;
    public TextMeshProUGUI chatHistoryText;

    [Header("스폰 시스템 연결")]
    public AIObjectSpawner spawner;

    [Header("스크롤 UI 연결")]
    public ScrollRect chatScrollRect;

    private string apiUrl = "https://authfailed.neoskyclad.com/api/v1/chat";

    void Start()
    {
        sendButton.onClick.AddListener(OnSendButtonClicked);
        chatHistoryText.text = "[시스템]: 이메지네이션 패닉 터미널에 접속했습니다.\n비밀번호에 대한 정보를 요구하거나, 원하는 구조물을 묘사하세요.\n";
    }

    void OnSendButtonClicked()
    {
        if (string.IsNullOrEmpty(chatInput.text)) return;

        string userMessage = chatInput.text;
        chatHistoryText.text += $"\n[2P]: {userMessage}";
        chatInput.text = "";

        sendButton.interactable = false;
        StartCoroutine(SendToAI(userMessage));
    }

    IEnumerator SendToAI(string message)
    {
        // 1. 매니저에서 타겟 패스워드 가져오기
        string currentPassword = ImaginationPanic_RpcManager.Instance.targetPassword;
        string contextHistory = chatHistoryText.text;

        // 2. DTO 생성 및 직렬화
        ChatRequestData reqData = new ChatRequestData
        {
            message = contextHistory,
            minigame_type = "이메지네이션 패닉",
            target_code = currentPassword
        };
        string jsonPayload = JsonUtility.ToJson(reqData);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 로딩 텍스트 띄우기
            chatHistoryText.text += "\n[AI]: 데이터 분석 중...";

            // [동기적 대기] 서버 응답이 완전히 끝날 때까지 여기서 멈춰서 기다립니다.
            yield return request.SendWebRequest();

            // 응답이 오면 로딩 텍스트 깔끔하게 제거
            chatHistoryText.text = chatHistoryText.text.Replace("\n[AI]: 데이터 분석 중...", "");

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"통신 에러: {request.error} / {request.downloadHandler.text}");
                chatHistoryText.text += "\n[시스템 Error]: API 서버 응답 없음!";
            }
            else
            {
                string responseText = request.downloadHandler.text;

                // 🔍 [디버그 1] FastAPI 서버가 준 순수 원본 텍스트를 콘솔에 노란색으로 출력!
                Debug.Log($"<color=yellow>[API 응답 원본]</color>\n{responseText}");

                responseText = responseText.Replace("```json", "").Replace("```", "").Trim();

                try
                {
                    AIResponseData data = JsonUtility.FromJson<AIResponseData>(responseText);

                    // 🔍 [디버그 2] 파싱이 끝난 C# 객체의 데이터를 콘솔에 초록색으로 출력!
                    Debug.Log($"<color=green>[JSON 파싱 성공]</color> 스폰여부: {data.isSpawning}, 메시지: '{data.replyMessage}'");

                    // 🚨 [방어 코드] 만약 서버가 메시지를 비워서 보냈다면?
                    if (string.IsNullOrEmpty(data.replyMessage))
                    {
                        Debug.LogWarning("⚠️ 서버 응답은 왔지만 replyMessage가 비어있습니다. (프롬프트 오류 의심)");
                        data.replyMessage = "(시스템: AI가 침묵을 선택했습니다.)";
                    }

                    // UI 텍스트에 적용
                    chatHistoryText.text += $"\n[AI]: {data.replyMessage}\n";

                    // UI 즉시 새로고침 및 스크롤 내리기 보장
                    Canvas.ForceUpdateCanvases();
                    if (chatScrollRect != null)
                    {
                        chatScrollRect.verticalNormalizedPosition = 0f;
                    }

                    // 스폰 로직 실행
                    if (data.isSpawning)
                    {
                        spawner.RequestSpawnFromAI(data.shapeIndex, data.matIndex, data.scaleX, data.scaleY, data.scaleZ);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"JSON 파싱 에러: {e.Message}\n원본 데이터: {responseText}");
                    chatHistoryText.text += "\n[시스템 Error]: AI 응답 규격 불일치!";
                }
            }

            // 모든 처리가 끝난 후 버튼 다시 활성화
            sendButton.interactable = true;
        }
    }

    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame(); // UI 텍스트가 렌더링될 때까지 딱 1프레임 대기

        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f; // 0이 맨 아래, 1이 맨 위
        }
    }
}