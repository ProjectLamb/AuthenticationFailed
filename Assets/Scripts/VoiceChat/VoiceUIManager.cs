using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class UIManager : MonoBehaviourPunCallbacks
{
    // 싱글톤: 다른 스크립트에서 UI에 쉽게 접근하기 위함
    public static UIManager Instance;

    public TextMeshProUGUI pcStatusText;
    public TextMeshProUGUI phoneStatusText;
    public Image pcMicIcon;
    public Image phoneMicIcon;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 시작할 때는 텍스트를 다 꺼둡니다.
        pcStatusText.gameObject.SetActive(false);
        phoneStatusText.gameObject.SetActive(false);
    }

    // 내가 방에 들어갔을 때
    public override void OnJoinedRoom()
    {
        UpdateConnectionUI();
    }

    // 다른 사람이 방에 들어왔을 때
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateConnectionUI();
    }

    // 플레이어 목록을 확인해서 텍스트를 켜주는 함수
    void UpdateConnectionUI()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            // 포톤은 처음 들어온 사람에게 ActorNumber 1을 줍니다 (PC)
            if (p.ActorNumber == 1)
            {
                pcStatusText.gameObject.SetActive(true);
            }
            // 두 번째로 들어온 사람에게는 ActorNumber 2를 줍니다 (스마트폰)
            else if (p.ActorNumber == 2)
            {
                phoneStatusText.gameObject.SetActive(true);
            }
        }
    }
}