using UnityEngine;
using TMPro;
using System.Collections;

public class WH_P2_Controller : MonoBehaviour
{
    public float moveSpeed = 600f;
    public float collisionDistance = 35f;
    public TextMeshProUGUI scoreText;

    [Header("References")]
    public WH_RpcManager rpcManager;

    [Header("Hit Settings")]
    public float hitCooldown = 0.3f;

    private RectTransform rectTransform;
    private float moveRangeX;
    private bool canBeHit = true;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        UpdateMoveRange();

        if (rpcManager == null)
        {
            Debug.LogError("[P2_Controller] rpcManager가 연결되지 않았습니다.");
        }
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 pos = rectTransform.anchoredPosition;

        pos.x += h * moveSpeed * Time.deltaTime;
        pos.y += v * moveSpeed * Time.deltaTime;

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
            if (itemRect == null) continue;

            float dist = Vector2.Distance(rectTransform.anchoredPosition, itemRect.anchoredPosition);

            if (dist < collisionDistance)
            {
                Debug.Log($"[P2_Controller] 충돌 감지: {item.name}, tag={item.tag}");

                if (rpcManager != null)
                {
                    rpcManager.ReportCollision(item.tag);
                }

                Destroy(item.gameObject);
                StartCoroutine(HitCooldownRoutine());
                return;
            }
        }
    }

    IEnumerator HitCooldownRoutine()
    {
        canBeHit = false;
        yield return new WaitForSeconds(hitCooldown);
        canBeHit = true;
    }

    private void UpdateMoveRange()
    {
        if (transform.parent != null)
        {
            RectTransform parentRect = transform.parent.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                moveRangeX = (parentRect.rect.width / 2f) - (rectTransform.rect.width / 2f);
            }
        }
    }
}