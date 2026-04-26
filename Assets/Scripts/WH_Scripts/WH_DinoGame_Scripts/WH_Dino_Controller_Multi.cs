using UnityEngine;
using Photon.Pun;

public class WH_Dino_Controller_Multi : MonoBehaviourPun
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public bool isP2 = false;

    [Header("Visuals")]
    public Sprite[] runSprites;     // FM_Run01 ~ 07 이미지를 여기에 할당
    public Sprite damageSprite;    // FM_Damage03 이미지를 여기에 할당
    public float animationSpeed = 0.1f; // 프레임 전환 속도

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    [HideInInspector] public bool isMoving = true;

    private int currentFrame;
    private float animationTimer;

    private bool isInsideBoostZone = false;

    private bool hasReportedStop = false;
    private bool hasReportedGoal = false;
    private WH_Dino_RpcManager rpcManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer 참조
        rpcManager = FindObjectOfType<WH_Dino_RpcManager>();

        Debug.Log($"{gameObject.name} 소유권 여부: {photonView.IsMine}");
    }

    void Update()
    {
        // 내 캐릭터가 아니더라도 애니메이션은 보여야 하므로 애니메이션 로직은 상단에 배치
        UpdateAnimation();

        if (!photonView.IsMine || !isMoving) return;

        if (PhotonNetwork.IsMasterClient && isP2) return;
        if (!PhotonNetwork.IsMasterClient && !isP2) return;

        float direction = isP2 ? -1f : 1f;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            // 발판 영역 안에 있으면 jumpForce의 3배 적용
            float actualJumpForce = isInsideBoostZone ? jumpForce * 1.5f : jumpForce;

            rb.AddForce(Vector2.up * actualJumpForce, ForceMode2D.Impulse);
            isGrounded = false;

            if (isInsideBoostZone) Debug.Log("<color=yellow>슈퍼 점프 발동!</color>");
            else Debug.Log($"{gameObject.name} 일반 점프!");
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("WH_Dino_Boost"))
        {
            isInsideBoostZone = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("WH_Dino_Boost"))
        {
            isInsideBoostZone = false;
        }
    }
    // 달리기 애니메이션 처리 루틴
    private void UpdateAnimation()
    {
        if (!isMoving) return; // 멈춰있을(충돌 등) 때는 애니메이션 중지

        if (runSprites == null || runSprites.Length == 0) return;

        animationTimer += Time.deltaTime;
        if (animationTimer >= animationSpeed)
        {
            animationTimer = 0f;
            currentFrame = (currentFrame + 1) % runSprites.Length; // 0~6 순환
            spriteRenderer.sprite = runSprites[currentFrame];
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("WH_Ground"))
        {
            isGrounded = true;
            return;
        }

        if (!photonView.IsMine) return;

        if (collision.gameObject.CompareTag("WH_Obstacle"))
        {
            if (hasReportedStop) return;
            HandleObstacleCollision(collision.transform);
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (hasReportedGoal) return;

            hasReportedGoal = true;
            StopDino();
            rpcManager?.ReportGoal();
        }
    }

    private void HandleObstacleCollision(Transform obstacleTrans)
    {
        if (hasReportedStop) return; // 중복 실행 방지
        hasReportedStop = true;

        isMoving = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        // 🔥 핵심: 모든 플레이어에게 내 이미지를 데미지 버전으로 바꾸라고 명령 (Buffered 추가)
        photonView.RPC(nameof(RPC_ChangeToDamageSprite), RpcTarget.AllBuffered);

        transform.rotation = Quaternion.Euler(0, 0, isP2 ? -90f : 90f);

        string pointName = isP2 ? "P2_Point" : "P1_Point";
        Transform snapPoint = obstacleTrans.Find(pointName);
        if (snapPoint != null)
            transform.position = snapPoint.position;

        // RpcManager에 정지 보고
        rpcManager?.ReportStop();
    }

    [PunRPC]
    public void RPC_ChangeToDamageSprite()
    {
        // 애니메이션을 멈추고 데미지 스프라이트로 고정
        isMoving = false;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (damageSprite != null)
        {
            spriteRenderer.sprite = damageSprite;
        }
        Debug.Log($"{gameObject.name}의 이미지가 Damage_03으로 변경되었습니다.");
    }

    public void StopDino()
    {
        isMoving = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
    }
}