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

            direction.x = contactSide;
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
                direction.y *= -1f;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 마스터: 위치와 '방향(direction)'을 같이 보냅니다.
            stream.SendNext(transform.localPosition);
            stream.SendNext(direction);
        }
        else
        {
            // 클라이언트: 위치와 방향을 받습니다.
            Vector3 networkPosition = (Vector3)stream.ReceiveNext();
            direction = (Vector2)stream.ReceiveNext();

            // [수정된 부분 2: 네트워크 지연(Ping) 보상]
            // 패킷이 날아오는 찰나의 시간 동안 공이 이미 이동했을 거리까지 계산해서 더해줍니다.
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