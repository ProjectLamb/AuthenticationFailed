using UnityEngine;
using Photon.Pun;

public class PingPongMove : MonoBehaviourPun
{
    public float speed = 7f;
    public float minY = -3.5f;
    public float maxY = 3.5f;
    public float lerpSpeed = 20f; // 보간 속도 (높을수록 빠르게 따라감)

    [Header("이 패들이 P1(왼쪽)인가요?")]
    public bool isPlayer1 = true;

    private float targetY;        // 상대방에게 받은 목표 Y값
    private bool isMyPaddle =>
        (PhotonNetwork.IsMasterClient && isPlayer1) ||
        (!PhotonNetwork.IsMasterClient && !isPlayer1);

    void Start()
    {
        targetY = transform.localPosition.y;
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom) return;

        if (isMyPaddle)
        {
            // 내 패들: 직접 조작
            float input = 0f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) input = 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) input = -1f;

            if (input != 0f)
            {
                Vector3 pos = transform.localPosition;
                pos.y = Mathf.Clamp(pos.y + input * speed * Time.deltaTime, minY, maxY);
                transform.localPosition = pos;
                photonView.RPC("RpcSyncPaddle", RpcTarget.Others, pos.y);
            }
        }
        else
        {
            // 상대 패들: 목표값으로 부드럽게 보간
            Vector3 pos = transform.localPosition;
            pos.y = Mathf.Lerp(pos.y, targetY, lerpSpeed * Time.deltaTime);
            transform.localPosition = pos;
        }
    }

    [PunRPC]
    void RpcSyncPaddle(float newY)
    {
        targetY = newY; // 즉시 적용 대신 목표값만 저장
    }
}