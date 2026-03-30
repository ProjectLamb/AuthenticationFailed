using UnityEngine;

public class WH_ObjectSpawner : MonoBehaviour
{
    public GameObject virusPrefab;
    private RectTransform areaRect;

    void Awake() => areaRect = GetComponent<RectTransform>();

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

        // 🔥 방향 퍼뜨리기 (핵심)
        float angle = Random.Range(-45f, 45f);
        Vector2 finalDir = Quaternion.Euler(0, 0, angle) * baseDir;

        GameObject obj = Instantiate(virusPrefab, transform);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = spawnPos;

        obj.GetComponent<WH_FallingObject>().Init(finalDir);
    }
}