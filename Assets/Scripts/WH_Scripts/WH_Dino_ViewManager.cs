using UnityEngine;
using Photon.Pun;

public class WH_Dino_View_Manager : MonoBehaviourPun
{
    public GameObject p1ViewPanel; // RawImage_P1이 담긴 패널
    public GameObject p2ViewPanel; // RawImage_P2가 담긴 패널

    void Start()
    {
        // 멀티플레이 연결 상태 확인
        if (PhotonNetwork.IsConnected)
        {
            SetupView();
        }
    }

    void SetupView()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // [방장 - P1]
            p1ViewPanel.SetActive(true);
            p2ViewPanel.SetActive(false);

            // P1 뷰를 화면 전체에 꽉 채우도록 RectTransform 조정 (필요 시)
            SetFullScreen(p1ViewPanel.GetComponent<RectTransform>());
        }
        else
        {
            // [참가자 - P2]
            p1ViewPanel.SetActive(false);
            p2ViewPanel.SetActive(true);

            // P2 뷰를 화면 전체에 꽉 채우도록 RectTransform 조정
            SetFullScreen(p2ViewPanel.GetComponent<RectTransform>());
        }
    }

    // UI를 화면 전체(Full Screen)로 만드는 헬퍼 함수
    void SetFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}