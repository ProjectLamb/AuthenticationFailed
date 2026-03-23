using UnityEngine;
using Photon.Pun;

public class PingPongMove : MonoBehaviourPun
{
    public float minY = -3.5f;
    public float maxY = 3.5f;
    public float lerpSpeed = 20f;

    [Header("마우스 감도 (위아래 움직임 속도)")]
    public float mouseSensitivity = 15f;

    [Header("이 패들이 P1(왼쪽)인가요?")]
    public bool isPlayer1 = true;

    private float targetY;
    private bool isMyPaddle =>
        (PhotonNetwork.IsMasterClient && isPlayer1) ||
        (!PhotonNetwork.IsMasterClient && !isPlayer1);

    void Start()
    {
        targetY = transform.localPosition.y;

        // [선택 사항] 주석을 풀면 게임 시작 시 마우스 커서가 화면 중앙에 묶이고 숨겨집니다. (진짜 오락실 게임 느낌)
        /*
        if (isMyPaddle) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        */
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom) return;

        if (isMyPaddle)
        {
            // [핵심 변경점] 마우스를 위아래로 움직인 '변화량'만 쏙 빼옵니다. (클릭 절대 필요 없음!)
            float mouseY = Input.GetAxis("Mouse Y");

            // 마우스를 아주 살짝이라도 움직였다면
            if (Mathf.Abs(mouseY) > 0.001f)
            {
                Vector3 pos = transform.localPosition;

                // 기존 위치에 마우스 움직임(변화량) * 감도를 더해줍니다.
                pos.y = Mathf.Clamp(pos.y + mouseY * mouseSensitivity * Time.deltaTime, minY, maxY);

                transform.localPosition = pos;
                photonView.RPC("RpcSyncPaddle", RpcTarget.Others, pos.y);
            }
        }
        else
        {
            // 상대 패들 동기화 (기존과 동일)
            Vector3 pos = transform.localPosition;
            pos.y = Mathf.Lerp(pos.y, targetY, lerpSpeed * Time.deltaTime);
            transform.localPosition = pos;
        }
    }

    [PunRPC]
    void RpcSyncPaddle(float newY)
    {
        targetY = newY;
    }
}