using UnityEngine;
using Photon.Voice.Unity;

[RequireComponent(typeof(Recorder))]
public class PhotonWwiseSender : MonoBehaviour
{
    private Recorder voiceRecorder;

    [Header("마이크 설정")]
    [Tooltip("체크하면 V키를 누르지 않아도 마이크가 항상 켜집니다 (디스코드 방식)")]
    public bool alwaysOn = false;

    [Tooltip("눌러서 말하기(PTT)에 사용할 단축키")]
    public KeyCode pttKey = KeyCode.V;

    void Awake()
    {
        voiceRecorder = GetComponent<Recorder>();

        // 초기 마이크 상태 설정
        voiceRecorder.TransmitEnabled = alwaysOn;
    }

    void Update()
    {
        // 항상 켜져 있는 모드면 입력 처리를 무시합니다.
        if (alwaysOn) return;

        // V 키를 누르는 순간 마이크 전송 시작
        if (Input.GetKeyDown(pttKey))
        {
            voiceRecorder.TransmitEnabled = true;
            Debug.Log("🎙️ 마이크 전송 시작 (V 누름)");
        }
        // V 키를 떼는 순간 마이크 전송 종료
        else if (Input.GetKeyUp(pttKey))
        {
            voiceRecorder.TransmitEnabled = false;
            Debug.Log("🔇 마이크 전송 종료 (V 뗌)");
        }
    }
}