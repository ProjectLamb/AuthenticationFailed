using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WH_CipherP1 : MonoBehaviour
{
    public InputField cipherInputField;
    public Button verifyButton;
    private WH_CipherManager manager;

    // 🔥 이전 텍스트 길이를 추적하기 위한 변수 추가
    private string lastProcessedText = "";

    public void Init()
    {
        lastProcessedText = ""; // 초기화
        cipherInputField.text = "";
        cipherInputField.interactable = true;
    }

    void Awake()
    {
        manager = FindObjectOfType<WH_CipherManager>();

        // 초기화 시 현재 텍스트 저장
        lastProcessedText = cipherInputField.text;

        cipherInputField.onValueChanged.AddListener(OnInputChanged);
        if (verifyButton != null)
            verifyButton.onClick.AddListener(OnVerifyButtonClicked);
    }

    void OnInputChanged(string input)
    {
        // 1. 🔥 백스페이스(삭제) 감지 로직
        // 글자 수가 줄어들었다면 아무것도 하지 않고 현재 상태만 저장하고 종료합니다.
        if (input.Length < lastProcessedText.Length)
        {
            lastProcessedText = input;
            return;
        }

        if (string.IsNullOrEmpty(input))
        {
            lastProcessedText = "";
            return;
        }

        // 2. 커서 위치 기반 마지막 입력 문자 확인
        int caretPos = cipherInputField.caretPosition;
        if (caretPos <= 0)
        {
            lastProcessedText = input;
            return;
        }

        char lastChar = input[caretPos - 1];

        // 3. 문자일 경우에만 매핑 로직 수행
        if (char.IsLetter(lastChar))
        {
            char mappedChar = manager.GetMappedChar(lastChar);

            // 무한 루프 방지를 위해 리스너 일시 제거
            cipherInputField.onValueChanged.RemoveListener(OnInputChanged);

            // 문자열 변환 및 대문자화
            System.Text.StringBuilder sb = new System.Text.StringBuilder(input);
            sb[caretPos - 1] = char.ToUpper(mappedChar);

            string finalResult = sb.ToString().ToUpper();
            cipherInputField.text = finalResult;

            // 커서 위치 유지
            cipherInputField.caretPosition = caretPos;

            // 🔥 최종 처리된 텍스트 저장
            lastProcessedText = finalResult;

            cipherInputField.onValueChanged.AddListener(OnInputChanged);
        }
        else
        {
            // 문자가 아닌 경우에도 현재 상태 저장
            lastProcessedText = input;
        }
    }

    void OnVerifyButtonClicked()
    {
        manager.OnClickVerify();
    }
}