using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;

public class TitleManager : MonoBehaviourPunCallbacks
{

    public RawImage fadePanel;
    public GameObject inputField;
    public InputField roomInputField;
    private bool IsStart = false;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }
    public void CreateCustomRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("아직 서버 연결 중입니다.");
            return;
        }

        int ran = Random.Range(0, 100000);
        ran = 500;
        string roomName = ran.ToString();

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;

        // 방 생성 시도
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinCustomRoom()
    {
        string roomName = roomInputField.text;

        if (!string.IsNullOrEmpty(roomName))
        {
            // 입력한 이름의 방으로 입장을 시도
            PhotonNetwork.JoinRoom(roomName);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장 성공! 방 이름: " + PhotonNetwork.CurrentRoom.Name);

        // 방장(MasterClient)이 씬을 로드하면 나머지 인원도 자동으로 따라가게 함
        // (단, Start 등에서 PhotonNetwork.AutomaticallySyncScene = true 설정이 되어 있어야 함)
        if (PhotonNetwork.IsMasterClient)
        {
            // "cococo"는 이동할 씬의 이름입니다.
            PhotonNetwork.LoadLevel("carrot");
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("방 생성 실패 (중복 가능성): " + message);

        // 다시 방 만들기 함수를 호출해서 새로운 번호를 뽑게 합니다.
        CreateCustomRoom();
    }

    // 방 입장에 실패했을 때 호출 (방 번호가 틀렸을 경우 등)
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("방 입장에 실패했습니다: " + message);
        // 에러 메시지 UI 띄우기 등의 처리
    }

    public void CreateRoom()
    {
        CreateCustomRoom();
    }

    public void JoinRoom()
    {
        inputField.SetActive(true);
    }

    public void StartGame()
    {
        if (!IsStart)
        {
            IsStart = true;
            StartCoroutine("FadePanel");
        }
    }

    IEnumerator FadePanel()
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.DOFade(1f, 1f);
        yield return new WaitForSeconds(1f);
        CreateRoom();
    }
}
