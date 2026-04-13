using UnityEngine;
using Photon.Pun;

public class WH_Dino_Controller_Multi : MonoBehaviourPun
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public bool isP2 = false;

    private Rigidbody2D rb;
    private bool isGrounded;
    [HideInInspector] public bool isMoving = true;

    private bool hasReportedStop = false;
    private bool hasReportedGoal = false;
    private WH_Dino_RpcManager rpcManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rpcManager = FindObjectOfType<WH_Dino_RpcManager>();

        Debug.Log($"{gameObject.name} 소유권 여부: {photonView.IsMine}");
    }

    void Update()
    {
        if (!photonView.IsMine || !isMoving) return;

        if (PhotonNetwork.IsMasterClient && isP2) return;
        if (!PhotonNetwork.IsMasterClient && !isP2) return;

        float direction = isP2 ? -1f : 1f;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            Debug.Log($"{gameObject.name} 점프 실행!");
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
        hasReportedStop = true;

        isMoving = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        transform.rotation = Quaternion.Euler(0, 0, isP2 ? -90f : 90f);

        string pointName = isP2 ? "P2_Point" : "P1_Point";
        Transform snapPoint = obstacleTrans.Find(pointName);
        if (snapPoint != null)
            transform.position = snapPoint.position;

        rpcManager?.ReportStop();
    }

    public void StopDino()
    {
        isMoving = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
    }
}