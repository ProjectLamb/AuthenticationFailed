using UnityEngine;

public class WH_FallingObject : MonoBehaviour
{
    public float speed = 200f;

    public float rotationSpeed = 180f;   // 회전 속도
    public float driftAmount = 30f;      // 흔들림 크기
    public float driftSpeed = 2f;        // 흔들림 속도

    private RectTransform rect;
    private Vector2 moveDir;

    private float noiseSeed;

    public void Init(Vector2 dir)
    {
        moveDir = dir.normalized;

        // 각 개체마다 랜덤 패턴
        noiseSeed = Random.Range(0f, 100f);
        rotationSpeed += Random.Range(-60f, 60f);
    }

    void Start()
    {
        rect = GetComponent<RectTransform>();
        Destroy(gameObject, 5f); //5초 후에 바이러스 프리팹 삭제
    }

    void Update()
    {
        float t = Time.time + noiseSeed;

        // 1️⃣ 기본 이동
        Vector2 move = moveDir;

        // 2️⃣ 좌우 흔들림 (노이즈 느낌)
        Vector2 perp = new Vector2(-moveDir.y, moveDir.x);
        float drift = Mathf.Sin(t * driftSpeed) * driftAmount;

        move += perp * (drift / 100f);

        // 3️⃣ 이동 적용
        rect.anchoredPosition += move.normalized * speed * Time.deltaTime;

        // 4️⃣ 회전
        rect.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        // 5️⃣ 화면 밖 제거
        if (Mathf.Abs(rect.anchoredPosition.x) > 900 ||
            Mathf.Abs(rect.anchoredPosition.y) > 900)
        {
            Destroy(gameObject);
        }
    }
}