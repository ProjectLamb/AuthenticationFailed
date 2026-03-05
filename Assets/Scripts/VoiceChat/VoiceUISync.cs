using UnityEngine;
using Photon.Pun;
using Photon.Voice.PUN;

public class VoiceUISync : MonoBehaviourPun
{
    private PhotonVoiceView voiceView;

    void Start()
    {
        // 내 프리팹에 붙어있는 보이스뷰 컴포넌트 가져오기
        voiceView = GetComponent<PhotonVoiceView>();
    }

    void Update()
    {
        // UIManager가 없으면 작동 안 함 (에러 방지)
        if (UIManager.Instance == null) return;

        // 마법의 속성: 지금 이 캐릭터가 말을 하고 있는가? (송출/수신 모두 포함)
        bool isSpeaking = voiceView.IsSpeaking;

        // 내가 1번 플레이어(PC)라면 파란 마이크 깜빡임
        if (photonView.Owner.ActorNumber == 1)
        {
            // 말하면 알파값 255(진하게), 안 하면 50(흐리게)
            UIManager.Instance.pcMicIcon.color = isSpeaking ?
                new Color(0, 0, 1, 1f) : new Color(0, 0, 1, 0.2f);
        }
        // 내가 2번 플레이어(스마트폰)라면 빨간 마이크 깜빡임
        else if (photonView.Owner.ActorNumber == 2)
        {
            UIManager.Instance.phoneMicIcon.color = isSpeaking ?
                new Color(1, 0, 0, 1f) : new Color(1, 0, 0, 0.2f);
        }
    }
}