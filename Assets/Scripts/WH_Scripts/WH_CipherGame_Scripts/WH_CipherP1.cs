using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WH_CipherP1 : MonoBehaviour
{
    public InputField cipherInputField;
    public Button verifyButton;
    private WH_CipherManager manager;

    public void Init()
    {
        cipherInputField.text = "";
        cipherInputField.interactable = true;
    }

    void Awake()
    {
        manager = FindObjectOfType<WH_CipherManager>();
        cipherInputField.onValueChanged.AddListener(OnInputChanged);
        if (verifyButton != null)
            verifyButton.onClick.AddListener(OnVerifyButtonClicked);
    }

    void OnInputChanged(string input)
    {
        if (string.IsNullOrEmpty(input)) return;

        // 마지막으로 입력된(변화된) 위치의 문자를 찾아 매핑합니다.
        int caretPos = cipherInputField.caretPosition;
        if (caretPos <= 0) return;

        char lastChar = input[caretPos - 1];

        if (char.IsLetter(lastChar))
        {
            char mappedChar = manager.GetMappedChar(lastChar);

            // 입력값이 이미 매핑된 값과 같다면 중복 처리 방지
            if (lastChar == mappedChar && char.IsUpper(lastChar)) return;

            cipherInputField.onValueChanged.RemoveListener(OnInputChanged);

            // 현재 커서 위치의 문자를 변환
            System.Text.StringBuilder sb = new System.Text.StringBuilder(input);
            sb[caretPos - 1] = char.ToUpper(mappedChar);
            cipherInputField.text = sb.ToString().ToUpper();

            // 커서 위치 유지
            cipherInputField.caretPosition = caretPos;

            cipherInputField.onValueChanged.AddListener(OnInputChanged);
        }
    }

    void OnVerifyButtonClicked()
    {
        // Manager의 VerifyAnswer 호출 (P1_View의 버튼 클릭 시)
        manager.OnClickVerify();
    }
}