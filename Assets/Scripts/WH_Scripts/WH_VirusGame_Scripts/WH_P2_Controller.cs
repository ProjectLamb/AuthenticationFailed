using UnityEngine;
using TMPro;

public class WH_P2_Controller : MonoBehaviour
{
    public float moveSpeed = 600f;
    public float collisionDistance = 35f;
    public TextMeshProUGUI scoreText;

    //private int vaccineCount = 0;
    private RectTransform rectTransform;
    private float moveRangeX;

    //public int GetVaccineCount() => vaccineCount;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        UpdateMoveRange();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A D
        float v = Input.GetAxisRaw("Vertical");   // W S

        Vector2 pos = rectTransform.anchoredPosition;

        pos.x += h * moveSpeed * Time.deltaTime;
        pos.y += v * moveSpeed * Time.deltaTime;

        // 영역 제한 (부모 기준)
        float maxX = (transform.parent.GetComponent<RectTransform>().rect.width / 2f) - (rectTransform.rect.width / 2f);
        float maxY = (transform.parent.GetComponent<RectTransform>().rect.height / 2f) - (rectTransform.rect.height / 2f);

        pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
        pos.y = Mathf.Clamp(pos.y, -maxY, maxY);

        rectTransform.anchoredPosition = pos;

        CheckCollisionManual();
    }

    private void CheckCollisionManual()
    {
        WH_FallingObject[] items = FindObjectsOfType<WH_FallingObject>();
        foreach (var item in items)
        {
            float dist = Vector2.Distance(rectTransform.anchoredPosition, item.GetComponent<RectTransform>().anchoredPosition);
            if (dist < collisionDistance)
            {
                WH_RpcManager rpc = transform.root.GetComponent<WH_RpcManager>();
                if (rpc != null) rpc.ReportCollision(item.tag);
                Destroy(item.gameObject);
            }
        }
    }

    /*public void AddVaccineCount()
    {
        if (vaccineCount < 10)
        {
            vaccineCount++;
        }
    }
    public void UpdateScoreUI(int score)
    {
        vaccineCount = score;
        if (scoreText != null) scoreText.text = $"백신: {vaccineCount} / 10";
    }*/

    private void UpdateMoveRange()
    {
        if (transform.parent != null)
            moveRangeX = (transform.parent.GetComponent<RectTransform>().rect.width / 2f) - (rectTransform.rect.width / 2f);
    }
}