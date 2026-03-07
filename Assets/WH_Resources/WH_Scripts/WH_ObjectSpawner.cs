using UnityEngine;

public class WH_ObjectSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject vaccinePrefab;
    public GameObject virusPrefab;

    private RectTransform areaRect;

    void Awake()
    {
        // 스포너가 붙은 패널(Panel_P2_View)의 RectTransform을 가져옵니다.
        areaRect = GetComponent<RectTransform>();
    }

    // P1_Downloader에서 호출하는 함수
    public void SpawnOneObject()
    {
        // 1. 랜덤 값(0.0 ~ 1.0)을 하나 뽑습니다.
        float randomChoice = Random.value;
        GameObject selectedPrefab;

        // 2. 확률에 따라 하나의 프리팹만 선택합니다.
        // 예: 0.7(70%)보다 크면 바이러스(30% 확률), 작으면 백신(70% 확률)
        if (randomChoice > 0.5f)
        {
            selectedPrefab = virusPrefab;
        }
        else
        {
            selectedPrefab = vaccinePrefab;
        }

        // 3. 선택된 단 '하나'의 프리팹만 생성 위치를 계산합니다.
        float halfWidth = areaRect.rect.width / 2f;
        float halfHeight = areaRect.rect.height / 2f;

        float randomX = Random.Range(-halfWidth, halfWidth);
        Vector2 spawnPos = new Vector2(randomX, halfHeight);

        // 4. 최종적으로 선택된 아이템 하나만 생성합니다.
        GameObject instance = Instantiate(selectedPrefab, transform);
        RectTransform objRect = instance.GetComponent<RectTransform>();
        objRect.anchoredPosition = spawnPos;
    }
}
