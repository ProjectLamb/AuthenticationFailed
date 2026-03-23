using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // (필요시) Slider 컴포넌트 제어를 위해 추가
using UnityEngine.EventSystems;
using Photon.Pun; 

public class JW_FlappyBird_Pipe : MonoBehaviourPun
{
    private float minY = 8f;
    private float maxY = 44f;

    public float sliderValue = 0;
    public bool IsControl = false;

    void Start()
    {
        IsControl = false;
        
        // 로컬 플레이어가 조종자인 경우에만 슬라이더 UI 동기화
        if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            JW_FlappyBird_PipeSpawn.Instance.pipeSlider.value = PipePosToSliderValue();
        }
        
        // 코루틴 실행
        StartCoroutine(SetControl());
    }

    void FixedUpdate()
    {
        // 1. 마스터 클라이언트: 왼쪽으로 계속 이동 및 소멸 로직
        if (PhotonNetwork.IsMasterClient)
        {
            this.transform.localPosition += new Vector3(-0.03f * Time.deltaTime, 0, 0);

            if (this.transform.localPosition.x < -0.0762f)
            {
                PhotonNetwork.Destroy(this.gameObject);
            }
        }

        // 2. Actor 2 (조종자): 슬라이더 값을 마스터에게 전송
        if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            sliderValue = JW_FlappyBird_PipeSpawn.Instance.pipeSlider.value;
            if (IsControl) SendInput();
        }
    }

    void SendInput()
    {
        photonView.RPC("SyncPipePosition", RpcTarget.MasterClient, sliderValue);
    }

    [PunRPC]
    void SyncPipePosition(float ratio)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        float targetY = Mathf.Lerp(minY, maxY, ratio);
        Vector3 currentPos = transform.localPosition;
        transform.localPosition = new Vector3(currentPos.x, targetY, currentPos.z);
    }

    public float PipePosToSliderValue()
    {
        float currentY = transform.localPosition.y;
        return Mathf.InverseLerp(minY, maxY, currentY);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "JW_FLAPPY_MID")
        {
            IsControl = false;
        }
    }

    // ⭐ 핵심 변경 부분
    IEnumerator SetControl()
    {
        // 1. 파이프 생성 직후 슬라이더 조작 비활성화
        if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            JW_FlappyBird_PipeSpawn.Instance.pipeSlider.interactable = false;
        }

        // 2. 기본 딜레이 0.5초 대기
        yield return new WaitForSeconds(0.5f);

        // 3. ⭐ 핵심: 마우스 왼쪽 버튼(또는 모바일 터치)을 뗄 때까지 코루틴 일시 정지
        // Input.GetMouseButton(0)이 false가 될 때까지 기다립니다.
        yield return new WaitUntil(() => !Input.GetMouseButton(0));

        // 4. 마우스를 완전히 뗐음이 확인되면 그제서야 조작 권한 부여
        IsControl = true;
        
        if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            JW_FlappyBird_PipeSpawn.Instance.pipeSlider.interactable = true;
        }
    }
}