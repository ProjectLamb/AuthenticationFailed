using UnityEngine;
using TMPro;

public class WH_P2_Controller : MonoBehaviour
{
    public float moveSpeed = 600f;
    public float collisionDistance = 35f;
    public TextMeshProUGUI scoreText;

    public WH_GameManager gameManager;
    public WH_P1_Downloader p1Downloader; // 인스펙터에서 P1 패널 연결 필수

    private int vaccineCount = 0;
    private RectTransform rectTransform;
    private float moveRangeX;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        UpdateMoveRange();
    }

    void Update()
    {
        if (moveRangeX <= 0) UpdateMoveRange();

        float h = Input.GetAxisRaw("Horizontal");
        Vector2 pos = rectTransform.anchoredPosition;
        pos.x += h * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, -moveRangeX, moveRangeX);
        rectTransform.anchoredPosition = pos;

        // 1. 실시간 클리어 조건 체크 (매 프레임 감시)
        if (vaccineCount >= 10 && p1Downloader != null && p1Downloader.IsFull())
        {
            // 이미 클리어가 실행 중인지 확인 (중복 실행 방지용 플래그가 있으면 좋음)
            if (gameManager != null)
            {
                gameManager.TriggerStageClear();
                // 중요: 한 번만 실행되도록 본인 스크립트를 비활성화하거나 플래그 처리
                this.enabled = false;
            }
        }

        CheckCollisionManual();
        
    }

    private void UpdateMoveRange()
    {
        if (transform.parent != null)
        {
            float parentWidth = transform.parent.GetComponent<RectTransform>().rect.width;
            moveRangeX = (parentWidth / 2f) - (rectTransform.rect.width / 2f);
        }
    }

    private void CheckCollisionManual()
    {
        WH_FallingObject[] items = FindObjectsOfType<WH_FallingObject>();
        foreach (var item in items)
        {
            float dist = Vector2.Distance(rectTransform.anchoredPosition, item.GetComponent<RectTransform>().anchoredPosition);
            if (dist < collisionDistance)
            {
                HandleCollision(item.gameObject);
            }
        }
    }

    private void HandleCollision(GameObject obj)
    {
        if (obj.CompareTag("WH_Virus"))
        {
            Destroy(obj);
            if (gameManager != null) gameManager.TriggerVirusPenalty();
        }
        else if (obj.CompareTag("WH_Vaccine"))
        {
            vaccineCount++;
            if (scoreText != null) scoreText.text = $"백신: {vaccineCount} / 10";
            Destroy(obj);

            
        }
    }
}