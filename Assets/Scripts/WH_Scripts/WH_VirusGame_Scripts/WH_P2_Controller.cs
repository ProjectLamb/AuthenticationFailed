using UnityEngine;
using UnityEngine.UI; // Image 제어를 위해 추가
using TMPro;
using System.Collections;

public class WH_P2_Controller : MonoBehaviour
{
    public float moveSpeed = 600f;
    public float collisionDistance = 35f;
    public TextMeshProUGUI scoreText;

    [Header("References")]
    public WH_RpcManager rpcManager;
    public Image playerImage;         // P2의 Image 컴포넌트 연결
    public RectTransform viewContainer; // 흔들기 효과를 줄 부모 컨테이너 (P2_View 등)

    [Header("Sprites")]
    public Sprite normalSprite; // FM_Run01 할당
    public Sprite damageSprite; // FM_Damage03 할당

    [Header("Hit Settings")]
    public float hitCooldown = 0.5f; // 피격 지속 시간 (0.5초 요청)
    public float shakeAmount = 10f;  // 화면 흔들림 강도

    private RectTransform rectTransform;
    private bool canBeHit = true;
    private Vector2 originalViewPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (viewContainer != null) originalViewPos = viewContainer.anchoredPosition;

        if (playerImage == null) playerImage = GetComponent<Image>();

        // 초기 이미지 설정
        if (playerImage != null && normalSprite != null) playerImage.sprite = normalSprite;
    }

    void Update()
    {
        // ⌨️ WASD로만 조작 가능하도록 수정
        float h = 0;
        if (Input.GetKey(KeyCode.A)) h -= 1;
        if (Input.GetKey(KeyCode.D)) h += 1;

        float v = 0;
        if (Input.GetKey(KeyCode.W)) v += 1;
        if (Input.GetKey(KeyCode.S)) v -= 1;

        // 🔄 이동 방향에 따른 좌우 반전 (Rotation Y 사용)
        if (h < 0) rectTransform.localRotation = Quaternion.Euler(0, 180, 0); // 왼쪽
        else if (h > 0) rectTransform.localRotation = Quaternion.Euler(0, 0, 0);  // 오른쪽

        Vector2 pos = rectTransform.anchoredPosition;
        pos.x += h * moveSpeed * Time.deltaTime;
        pos.y += v * moveSpeed * Time.deltaTime;

        // 이동 범위 제한 (부모 캔버스 기준)
        RectTransform parentRect = transform.parent.GetComponent<RectTransform>();
        float maxX = (parentRect.rect.width / 2f) - (rectTransform.rect.width / 2f);
        float maxY = (parentRect.rect.height / 2f) - (rectTransform.rect.height / 2f);

        pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
        pos.y = Mathf.Clamp(pos.y, -maxY, maxY);

        rectTransform.anchoredPosition = pos;

        if (canBeHit)
        {
            CheckCollisionManual();
        }
    }

    private void CheckCollisionManual()
    {
        WH_FallingObject[] items = FindObjectsOfType<WH_FallingObject>();
        foreach (var item in items)
        {
            if (item == null) continue;
            RectTransform itemRect = item.GetComponent<RectTransform>();
            float dist = Vector2.Distance(rectTransform.anchoredPosition, itemRect.anchoredPosition);

            if (dist < collisionDistance)
            {
                if (rpcManager != null) rpcManager.ReportCollision(item.tag);

                Destroy(item.gameObject);
                // 💥 피격 효과 코루틴 실행
                StartCoroutine(HitEffectRoutine());
                return;
            }
        }
    }

    IEnumerator HitEffectRoutine()
    {
        canBeHit = false;

        // 1. 이미지 변경 (FM_Damage03)
        if (playerImage != null && damageSprite != null) playerImage.sprite = damageSprite;

        // 2. 화면 흔들림 시작
        float elapsed = 0f;
        while (elapsed < hitCooldown)
        {
            if (viewContainer != null)
            {
                float x = Random.Range(-1f, 1f) * shakeAmount;
                float y = Random.Range(-1f, 1f) * shakeAmount;
                viewContainer.anchoredPosition = originalViewPos + new Vector2(x, y);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. 복구
        if (viewContainer != null) viewContainer.anchoredPosition = originalViewPos;
        if (playerImage != null && normalSprite != null) playerImage.sprite = normalSprite;

        canBeHit = true;
    }
}