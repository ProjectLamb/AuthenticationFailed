using UnityEngine;

public class WH_FallingObject : MonoBehaviour
{
    public float fallSpeed = 300f;
    // 이제 isVirus 변수 대신 유니티 태그를 사용합니다.

    private RectTransform rectTransform;
    private float floorY;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (transform.parent != null)
        {
            floorY = -(transform.parent.GetComponent<RectTransform>().rect.height / 2f);
        }

        // 중요: P2_Controller가 이 물체를 찾을 수 있도록 리스트에 등록하는 로직을 추가할 수도 있지만,
        // 여기서는 FindObjectsOfType 방식을 사용하여 간단하게 구현하겠습니다.
    }

    void Update()
    {
        if (rectTransform.anchoredPosition.y > floorY)
        {
            rectTransform.anchoredPosition += Vector2.down * fallSpeed * Time.deltaTime;
        }
        else
        {
            // 바닥에 닿으면 1초 뒤 자동 소멸
            Destroy(gameObject);
        }
    }
}