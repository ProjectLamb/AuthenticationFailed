using UnityEngine;
using Photon.Pun;

public class PingPongMove : MonoBehaviourPun
{
    public float minY = -0.4f;
    public float maxY = 0.4f;
    public float lerpSpeed = 20f;

    // 🚨 감도(Sensitivity) 변수는 아예 삭제했습니다! (마우스 커서를 그대로 따라가므로 필요 없음)

    [Header("이 패들이 P1(왼쪽)인가요?")]
    public bool isPlayer1 = true;

    private float targetY;
    private bool isMyPaddle =>
        (PhotonNetwork.IsMasterClient && isPlayer1) ||
        (!PhotonNetwork.IsMasterClient && !isPlayer1);

    private float zDistanceToCamera;

    void Start()
    {
        targetY = transform.localPosition.y;

        // 카메라와 패들 사이의 거리를 계산 (마우스 좌표를 3D/2D 게임 월드 좌표로 정확히 변환하기 위해 필수)
        if (Camera.main != null)
        {
            zDistanceToCamera = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        }
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom) return;

        // 매니저가 게임 시작 신호를 주기 전이거나 게임 오버면 조작 불가
        if (PingPong_RpcManager.instance != null && (!PingPong_RpcManager.instance.isGameStarted || PingPong_RpcManager.instance.isGameOver)) return;

        if (isMyPaddle)
        {
            // 🎯 1. 현재 마우스의 화면 상 위치를 가져옴
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = zDistanceToCamera;

            // 🎯 2. 화면 좌표(픽셀)를 게임 월드 좌표로 변환
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

            // 🎯 3. 월드 좌표를 로컬 좌표로 변환 (minY, maxY가 localPosition 기준이기 때문)
            Vector3 localMousePos = transform.parent != null ? transform.parent.InverseTransformPoint(worldMousePos) : worldMousePos;

            // 🎯 4. 패들이 minY ~ maxY 범위를 벗어나지 않도록 제한
            float clampedY = Mathf.Clamp(localMousePos.y, minY, maxY);

            // 🎯 5. 위치가 변했을 때만 움직이고 RPC 쏘기 (네트워크 낭비 방지)
            if (Mathf.Abs(transform.localPosition.y - clampedY) > 0.001f)
            {
                Vector3 newPos = transform.localPosition;
                newPos.y = clampedY;
                transform.localPosition = newPos;

                photonView.RPC("RpcSyncPaddle", RpcTarget.Others, newPos.y);
            }
        }
        else
        {
            // 상대방 패들은 부드럽게 Lerp로 따라가게 유지
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