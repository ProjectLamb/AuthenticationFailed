using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PingPongBall : MonoBehaviourPun, IPunObservable
{
    public float speed = 5f;
    public float lerpSpeed = 20f;

    private Vector2 direction;
    private bool isStopped = false;
    private PingPong_RpcManager rpcManager;
    private Renderer ballRenderer;

    private Vector3 targetLocalPos;

    void Start()
    {
        rpcManager = FindObjectOfType<PingPong_RpcManager>();
        targetLocalPos = transform.localPosition;
        ballRenderer = GetComponent<Renderer>();

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(RespawnRoutine());
    }

    void Update()
    {
        if (isStopped) return;

        if (PhotonNetwork.IsMasterClient)
        {
            // 마스터는 평소대로 공을 이동시킵니다.
            transform.localPosition += (Vector3)(direction * speed * Time.deltaTime);
        }
        else
        {
            // [수정된 부분 1: 클라이언트 예측]
            // 클라이언트도 서버가 패킷을 안 보낼 때 가만히 있는 게 아니라, 공의 방향과 속도를 바탕으로 스스로 이동합니다!
            targetLocalPos += (Vector3)(direction * speed * Time.deltaTime);

            // 실제 공은 그 예측된 목표 위치를 부드럽게 따라갑니다. (잔렉 해결의 핵심)
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, lerpSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isStopped) return;
        if (!PhotonNetwork.IsMasterClient) return;
        if (direction == Vector2.zero) return;

        if (other.CompareTag("Player"))
        {
            float contactSide = (transform.position.x < other.transform.position.x) ? -1f : 1f;
            if (Mathf.Sign(direction.x) == Mathf.Sign(contactSide)) return;

            // 1. 공이 패들의 어느 위치에 맞았는지 비율을 계산합니다.
            // (결과값 -> 패들 맨 위: 1, 정중앙: 0, 패들 맨 아래: -1)
            float yOffset = transform.position.y - other.transform.position.y;
            float hitFactor = yOffset / other.bounds.extents.y;

            // 2. 약간의 난수(랜덤값)를 더해서 매번 똑같이 튕기는 걸 방지합니다.
            float randomness = Random.Range(-0.15f, 0.15f);

            // 3. X축 방향을 뒤집고, Y축 방향은 맞은 위치(+랜덤값)에 비례하게 꺾어줍니다.
            // 곱하는 숫자(예: 1.5f)를 키우면 모서리에 맞았을 때 더 미친 듯이 예리하게 꺾입니다.
            Vector2 newDirection = new Vector2(contactSide, (hitFactor + randomness) * 1.5f);

            // 4. normalized를 해줘야 꺾이는 각도와 상관없이 공의 '스피드'가 일정하게 유지됩니다.
            direction = newDirection.normalized;

            if (rpcManager != null) rpcManager.AddCombo();
        }
        else if (other.CompareTag("Wall"))
        {
            if (other.bounds.size.y > other.bounds.size.x)
            {
                direction = Vector2.zero;
                if (rpcManager != null) rpcManager.ResetCombo();
                StartCoroutine(RespawnRoutine());
            }
            else
            {
                // 위아래 벽에 부딪힐 때도 약간의 불규칙성을 주려면 아래처럼 할 수 있습니다. (선택사항)
                direction.y *= -1f;
                // 벽에 튕길 때도 약간 각도를 틀고 싶다면 아래 주석을 푸세요!
                // direction.x += Random.Range(-0.1f, 0.1f); 
                // direction = direction.normalized;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 마스터: 위치, 방향에 이어 '현재 속도(speed)'까지 묶어서 보냅니다.
            stream.SendNext(transform.localPosition);
            stream.SendNext(direction);
            
        }
        else
        {
            // 클라이언트: 위치, 방향, '속도'를 차례대로 받습니다.
            Vector3 networkPosition = (Vector3)stream.ReceiveNext();
            direction = (Vector2)stream.ReceiveNext();
            

            // [네트워크 지연 보상] 받은 속도를 바탕으로 오차를 완벽하게 계산합니다.
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            targetLocalPos = networkPosition + (Vector3)(direction * speed * lag);
        }
    }

    // --- 아래는 기존과 동일한 리스폰 관련 함수들 ---
    IEnumerator RespawnRoutine()
    {
        photonView.RPC("RpcRespawnStart", RpcTarget.All);
        yield return new WaitForSeconds(1f);
        if (isStopped) yield break;

        float randomX = Random.Range(0, 2) == 0 ? -1f : 1f;
        float randomY = Random.Range(-0.5f, 0.5f);
        Vector2 newDir = new Vector2(randomX, randomY).normalized;

        photonView.RPC("RpcRespawnEnd", RpcTarget.All, newDir.x, newDir.y);
    }

    [PunRPC]
    void RpcRespawnStart()
    {
        direction = Vector2.zero;
        if (ballRenderer != null) ballRenderer.enabled = false;
        transform.localPosition = new Vector3(100f, 100f, 0f);
        targetLocalPos = new Vector3(100f, 100f, 0f);
    }

    [PunRPC]
    void RpcRespawnEnd(float dx, float dy)
    {
        transform.localPosition = Vector3.zero;
        targetLocalPos = Vector3.zero;
        direction = new Vector2(dx, dy);
        if (ballRenderer != null) ballRenderer.enabled = true;
    }

    public void StopBall()
    {
        isStopped = true;
        direction = Vector2.zero;
        photonView.RPC("RpcStopBall", RpcTarget.Others);
    }

    [PunRPC]
    void RpcStopBall()
    {
        isStopped = true;
        direction = Vector2.zero;
    }
}