using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class DataPacket_Player : MonoBehaviourPun, IPunObservable
{
    [Header("플레이어 설정")]
    public bool isPlayer1_Setting = true;

    [Header("바구니(이동) 설정")]
    public float moveSpeed = 5f;
    public float minX;
    public float maxX;

    [Header("발사 설정")]
    public Transform firePoint;
    public Slider powerGauge;
    public float maxPower = 15f;
    public float gaugeSpeed = 20f;
    public float shootCooldown = 1f;
    private float lastShootTime = -1f;
    public LineRenderer trajectoryLine;
    private Vector3 startPosition;
    private float currentPower = 0f;
    private bool isCharging = false;
    private Vector3 networkPosition;

    bool IsMulti => PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount >= 2;
    bool IsP1 => PhotonNetwork.IsMasterClient;
    int Phase => DataPacket_RpcManager.instance != null ? DataPacket_RpcManager.instance.currentPhase : 1;

    bool IsShooter => (Phase == 1 && isPlayer1_Setting) || (Phase == 2 && !isPlayer1_Setting);
    bool IsMover => (Phase == 1 && !isPlayer1_Setting) || (Phase == 2 && isPlayer1_Setting);

    void Start()
    {
        networkPosition = transform.localPosition;
        startPosition = transform.localPosition;
        

        if (trajectoryLine != null) trajectoryLine.positionCount = 0;
        if (powerGauge != null) powerGauge.gameObject.SetActive(false);

        if (!isPlayer1_Setting)
        {
            if (!PhotonNetwork.IsMasterClient)
                photonView.RequestOwnership();

            transform.localScale = IsMulti
                ? new Vector3(0.07f, 0.09f, 1f)
                : new Vector3(0.13f, 0.09f, 1f);
        }
    }

    void Update()
    {
        if (DataPacket_RpcManager.instance != null && !DataPacket_RpcManager.instance.isGameStarted)
        {
            return; 
        }

        
        if (DataPacket_RpcManager.instance != null && DataPacket_RpcManager.instance.isGameOver)
        {
            if (isCharging) ResetCharging();
            return;
        }

        if (IsMulti)
            HandleMulti();
        else
            HandleSolo();
    }

    // ── 솔로 ──────────────────────────────────────
    void HandleSolo()
    {
        if (isPlayer1_Setting)
            HandleShooting();
        else
            HandleAIMovement();

    }

    // ── 멀티 ──────────────────────────────────────
    void HandleMulti()
    {
        if (!photonView.IsMine)
        {
            SyncPosition();
            return;
        }

        if (IsShooter)
        {
            if ((isPlayer1_Setting && IsP1) || (!isPlayer1_Setting && !IsP1))
                HandleShooting();
        }

        if (IsMover)
        {
            if ((isPlayer1_Setting && IsP1) || (!isPlayer1_Setting && !IsP1))
                HandleMovement();
        }
    }

    void HandleMovement()
    {
        float h = 0f;
        if (Input.GetKey(KeyCode.A)) h = -1f;
        if (Input.GetKey(KeyCode.D)) h = 1f;

        float newX = Mathf.Clamp(
            transform.localPosition.x + h * moveSpeed * Time.deltaTime,
            minX, maxX
        );
        transform.localPosition = new Vector3(newX, transform.localPosition.y, 0);
    }

    void SyncPosition()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            networkPosition,
            Time.deltaTime * 15f
        );
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext(transform.localPosition);
        else
            networkPosition = (Vector3)stream.ReceiveNext();
    }

    // ── AI (솔로 전용) ────────────────────────────
    void HandleAIMovement()
    {
        float movement = Mathf.PingPong(Time.time * moveSpeed, maxX - minX);
        transform.localPosition = new Vector3(minX + movement, transform.localPosition.y, 0);
    }

    // ── 발사 ──────────────────────────────────────
    void HandleShooting()
    {
        if (Time.time < lastShootTime + shootCooldown) return;

        bool keyDown = Input.GetKeyDown(KeyCode.Space);
        bool keyHold = Input.GetKey(KeyCode.Space);
        bool keyUp = Input.GetKeyUp(KeyCode.Space);

        if (keyDown)
        {
            isCharging = true;
            if (powerGauge != null) { powerGauge.gameObject.SetActive(true); powerGauge.value = 0f; }
        }

        if (isCharging && keyHold)
        {
            currentPower += gaugeSpeed * Time.deltaTime;
            currentPower = Mathf.Clamp(currentPower, 0f, maxPower);
            if (powerGauge != null) powerGauge.value = currentPower;
            DrawTrajectory();
        }

        if (isCharging && keyUp)
        {
            ShootCapsule();
            lastShootTime = Time.time;
            ResetCharging();
        }
    }

    void ResetCharging()
    {
        isCharging = false;
        currentPower = 0f;
        if (powerGauge != null) powerGauge.gameObject.SetActive(false);
        if (trajectoryLine != null) trajectoryLine.positionCount = 0;
    }

    void ShootCapsule()
    {
        // 항상 MasterClient가 생성하도록 RPC로 요청
        photonView.RPC("RpcShoot", RpcTarget.MasterClient,
            (Vector2)firePoint.position, currentPower);
    }
    [PunRPC]
    void RpcShoot(Vector2 spawnPos, float power)
    {
        GameObject capsule = PhotonNetwork.Instantiate(
            "Minigames/PaperPlane",
            spawnPos,
            Quaternion.identity
        );

        DataPacket_PaperPlane pp = capsule.GetComponent<DataPacket_PaperPlane>();
        if (pp != null)
        {
            Vector2 dir = isPlayer1_Setting
                ? new Vector2(1.2f, 1f).normalized
                : new Vector2(-1.2f, 1f).normalized;
            pp.Launch(dir * power);
        }
    }

    void DrawTrajectory()
    {
        if (trajectoryLine == null) return;

        int maxPoints = 25;
        trajectoryLine.positionCount = maxPoints;

        Vector2 dir = isPlayer1_Setting
            ? new Vector2(1.2f, 1f).normalized
            : new Vector2(-1.2f, 1f).normalized;

        Vector2 startVel = dir * currentPower;
        Vector2 previousPos = firePoint.position;
        trajectoryLine.SetPosition(0, previousPos);

        // 자기 자신의 콜라이더 가져오기
        Collider2D myCollider = GetComponent<Collider2D>();

        for (int i = 1; i < maxPoints; i++)
        {
            float t = i * 0.1f;
            Vector2 nextPos = (Vector2)firePoint.position + (startVel * t) + 0.5f * Physics2D.gravity * (t * t);

            RaycastHit2D hit = Physics2D.Linecast(previousPos, nextPos);

            // 자기 자신에 맞았으면 무시
            if (hit.collider != null && hit.collider != myCollider)
            {
                trajectoryLine.positionCount = i + 1;
                trajectoryLine.SetPosition(i, hit.point);
                break;
            }

            trajectoryLine.SetPosition(i, nextPos);
            previousPos = nextPos;
        }
    }
}
