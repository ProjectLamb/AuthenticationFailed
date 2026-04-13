using UnityEngine;
using System.Collections;

public class WH_ObjectSpawner : MonoBehaviour
{
    public GameObject virusPrefab;
    private RectTransform areaRect;

    [Header("Auto Spawn Settings")]
    public float spawnInterval = 2f;   // 1초마다
    public int spawnPerEdge = 2;       // 모서리당 3개

    private Coroutine spawnRoutine;

    void Awake()
    {
        areaRect = GetComponent<RectTransform>();

        if (areaRect == null)
            Debug.LogError("[Spawner] RectTransform이 없습니다.");
    }

    // -----------------------------
    // 기존 랜덤 스폰 (유지)
    // -----------------------------
    public void SpawnVirus()
    {
        int dir = Random.Range(0, 4);

        Vector2 baseDir = Vector2.zero;
        Vector2 spawnPos = Vector2.zero;

        float width = areaRect.rect.width / 2f;
        float height = areaRect.rect.height / 2f;

        switch (dir)
        {
            case 0:
                spawnPos = new Vector2(Random.Range(-width, width), height);
                baseDir = Vector2.down;
                break;

            case 1:
                spawnPos = new Vector2(Random.Range(-width, width), -height);
                baseDir = Vector2.up;
                break;

            case 2:
                spawnPos = new Vector2(-width, Random.Range(-height, height));
                baseDir = Vector2.right;
                break;

            case 3:
                spawnPos = new Vector2(width, Random.Range(-height, height));
                baseDir = Vector2.left;
                break;
        }

        float angle = Random.Range(-45f, 45f);
        Vector2 finalDir = Quaternion.Euler(0, 0, angle) * baseDir;

        CreateVirus(spawnPos, finalDir);
    }

    // -----------------------------
    // 🔥 자동 스폰 시작
    // -----------------------------
    public void StartAutoSpawn()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(AutoSpawnRoutine());
    }

    public void StopAutoSpawn()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator AutoSpawnRoutine()
    {
        while (true)
        {
            SpawnAllEdges();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // -----------------------------
    // 🔥 각 모서리당 3개씩 생성
    // -----------------------------
    private void SpawnAllEdges()
    {
        for (int edge = 0; edge < 4; edge++)
        {
            for (int i = 0; i < spawnPerEdge; i++)
            {
                SpawnFromEdge(edge);
            }
        }
    }

    private void SpawnFromEdge(int edge)
    {
        float width = areaRect.rect.width / 2f;
        float height = areaRect.rect.height / 2f;

        Vector2 spawnPos = Vector2.zero;
        Vector2 baseDir = Vector2.zero;

        switch (edge)
        {
            case 0: // 위
                spawnPos = new Vector2(Random.Range(-width, width), height);
                baseDir = Vector2.down;
                break;

            case 1: // 아래
                spawnPos = new Vector2(Random.Range(-width, width), -height);
                baseDir = Vector2.up;
                break;

            case 2: // 왼쪽
                spawnPos = new Vector2(-width, Random.Range(-height, height));
                baseDir = Vector2.right;
                break;

            case 3: // 오른쪽
                spawnPos = new Vector2(width, Random.Range(-height, height));
                baseDir = Vector2.left;
                break;
        }

        float angle = Random.Range(-45f, 45f);
        Vector2 finalDir = Quaternion.Euler(0, 0, angle) * baseDir;

        CreateVirus(spawnPos, finalDir);
    }

    // -----------------------------
    // 🔥 실제 생성 공통 함수
    // -----------------------------
    private void CreateVirus(Vector2 spawnPos, Vector2 dir)
    {
        if (virusPrefab == null)
        {
            Debug.LogError("[Spawner] virusPrefab이 없습니다.");
            return;
        }

        GameObject obj = Instantiate(virusPrefab, transform);

        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = spawnPos;
            rect.localScale = Vector3.one;
        }

        WH_FallingObject falling = obj.GetComponent<WH_FallingObject>();
        if (falling != null)
        {
            falling.Init(dir);
        }
        else
        {
            Debug.LogError("[Spawner] WH_FallingObject 없음");
        }
    }
}