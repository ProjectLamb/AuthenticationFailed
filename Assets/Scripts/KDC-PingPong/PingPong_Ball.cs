using System.Collections;
using UnityEngine;
using Photon.Pun;
using DG.Tweening; // 🚨 DOTween 필수!

public class PingPongBall : MonoBehaviourPun
{
    public float speed = 5f; // DOTween으로 바뀌면서 체감 속도가 다를 수 있으니 조절해보세요!

    private Vector2 currentDirection;
    private bool isStopped = true;
    private PingPong_RpcManager rpcManager;
    private Renderer ballRenderer;

    private Tween moveTween; // DOTween 제어용 변수

    void Start()
    {
        rpcManager = FindObjectOfType<PingPong_RpcManager>();
        ballRenderer = GetComponent<Renderer>();
    }

    // 🎯 RpcManager가 조작법 확인 후 부르는 함수
    public void ResetBall()
    {
        isStopped = false;

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    // ❌ Update 함수 완전 삭제! (DOTween이 알아서 움직여줌)
    // ❌ OnPhotonSerializeView 함수 완전 삭제! (실시간 동기화 필요 없음)

    // 🎯 마스터 클라이언트가 공의 도착 지점을 계산해서 모두에게 쏘는 함수
    private void CalculateAndShoot(Vector3 startPos, Vector2 dir)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        currentDirection = dir.normalized;
        // 현재 위치에서 방향을 따라 아주 멀리 있는 목적지 계산
        Vector3 targetPos = startPos + (Vector3)(currentDirection * 50f);
        float duration = 50f / speed; // 시간 = 거리 / 속력

        // 양쪽 모두에게 시작점, 끝점, 걸리는 시간을 보냄
        photonView.RPC("RpcMoveBall", RpcTarget.All, startPos, targetPos, duration, currentDirection.x, currentDirection.y);
    }

    [PunRPC]
    void RpcMoveBall(Vector3 startPos, Vector3 targetPos, float duration, float dirX, float dirY)
    {
        currentDirection = new Vector2(dirX, dirY);
        if (ballRenderer != null) ballRenderer.enabled = true;

        // 기존에 움직이던 트윈 정지 및 위치 강제 동기화 (오차 교정)
        moveTween?.Kill();
        transform.localPosition = startPos;

        // DOTween으로 이동 시작 (등속도 이동)
        moveTween = transform.DOLocalMove(targetPos, duration).SetEase(Ease.Linear);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isStopped) return;
        if (!PhotonNetwork.IsMasterClient) return;
        if (currentDirection == Vector2.zero) return;

        if (other.CompareTag("Player"))
        {
            float contactSide = (transform.position.x < other.transform.position.x) ? -1f : 1f;
            if (Mathf.Sign(currentDirection.x) == Mathf.Sign(contactSide)) return;

            // 형님이 짜두신 정교한 반사각 + 랜덤성 로직 그대로 유지!
            float yOffset = transform.position.y - other.transform.position.y;
            float hitFactor = yOffset / other.bounds.extents.y;
            float randomness = Random.Range(-0.15f, 0.15f);

            Vector2 newDirection = new Vector2(contactSide, (hitFactor + randomness) * 1.5f).normalized;

            if (rpcManager != null) rpcManager.AddCombo();

            // 부딪힌 현재 위치에서 새로운 방향으로 다시 쏨
            CalculateAndShoot(transform.localPosition, newDirection);
        }
        else if (other.CompareTag("Wall"))
        {
            if (other.bounds.size.y > other.bounds.size.x) // 좌우 벽 (게임 오버/점수 로직)
            {
                if (rpcManager != null) rpcManager.ResetCombo();
                StartCoroutine(RespawnRoutine());
            }
            else // 상하 벽 (일반 바운스)
            {
                Vector2 newDirection = currentDirection;
                newDirection.y *= -1f; // y축만 반전

                // 부딪힌 위치에서 튕긴 방향으로 다시 쏨
                CalculateAndShoot(transform.localPosition, newDirection);
            }
        }
    }

    IEnumerator RespawnRoutine()
    {
        photonView.RPC("RpcRespawnStart", RpcTarget.All);
        yield return new WaitForSeconds(1f);
        if (isStopped) yield break;

        float randomX = Random.Range(0, 2) == 0 ? -1f : 1f;
        float randomY = Random.Range(-0.5f, 0.5f);
        Vector2 newDir = new Vector2(randomX, randomY).normalized;

        // 원점(0,0,0)에서 새로운 방향으로 발사
        CalculateAndShoot(Vector3.zero, newDir);
    }

    [PunRPC]
    void RpcRespawnStart()
    {
        moveTween?.Kill(); // 움직임 멈춤
        currentDirection = Vector2.zero;
        if (ballRenderer != null) ballRenderer.enabled = false;
        transform.localPosition = new Vector3(100f, 100f, 0f);
    }

    public void StopBall()
    {
        isStopped = true;
        photonView.RPC("RpcStopBall", RpcTarget.All);
    }

    [PunRPC]
    void RpcStopBall()
    {
        isStopped = true;
        moveTween?.Kill();
        currentDirection = Vector2.zero;
    }
}