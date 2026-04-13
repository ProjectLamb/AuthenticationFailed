using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class UIManager : MonoBehaviourPunCallbacks
{
    // �̱���: �ٸ� ��ũ��Ʈ���� UI�� ���� �����ϱ� ����
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
        // ������ ���� �ؽ�Ʈ�� �� ���Ӵϴ�.
        pcStatusText.gameObject.SetActive(false);
        phoneStatusText.gameObject.SetActive(false);
        UpdateConnectionUI();
    }

    // ���� �濡 ���� ��
    public override void OnJoinedRoom()
    {
        //UpdateConnectionUI();
    }

    // �ٸ� ����� �濡 ������ ��
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //UpdateConnectionUI();
    }

    // �÷��̾� ����� Ȯ���ؼ� �ؽ�Ʈ�� ���ִ� �Լ�
    void UpdateConnectionUI()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            // ������ ó�� ���� ������� ActorNumber 1�� �ݴϴ� (PC)
            if (p.ActorNumber == 1)
            {
                pcStatusText.gameObject.SetActive(true);
                if(PhotonNetwork.LocalPlayer.ActorNumber == 1) 
                {
                    LoadingManager.Instance.LoadingPC();
                    GameManager.Instance.DesktopOn();
                }
            }
            // �� ��°�� ���� ������Դ� ActorNumber 2�� �ݴϴ� (����Ʈ��)
            else if (p.ActorNumber == 2)
            {
                phoneStatusText.gameObject.SetActive(true);
                if(PhotonNetwork.LocalPlayer.ActorNumber == 2) 
                {
                    LoadingManager.Instance.LoadingMobile();
                    GameManager.Instance.MobileOn();
                }
            }
        }
    }
}