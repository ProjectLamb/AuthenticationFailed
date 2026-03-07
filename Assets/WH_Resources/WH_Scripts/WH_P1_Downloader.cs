using UnityEngine;
using UnityEngine.UI;

public class WH_P1_Downloader : MonoBehaviour
{
    public Slider downloadSlider;
    public WH_ObjectSpawner spawner;

    public float decayRate = 0.2f;
    public float boostAmount = 5.0f;

    [SerializeField]
    private float currentProgress = 0f;

    void Update()
    {
        if (currentProgress > 0)
        {
            currentProgress -= decayRate * Time.deltaTime;
        }
        currentProgress = Mathf.Clamp(currentProgress, 0, 100);

        if (downloadSlider != null)
        {
            downloadSlider.value = currentProgress / 100f;
        }
    }

    public void OnClickDownload()
    {
        currentProgress += boostAmount;
        if (spawner != null) spawner.SpawnOneObject();
    }

    // 클리어 판정용: 현재 게이지가 100(ProgressBar 1)인지 확인
    public bool IsFull()
    {
        // 부동 소수점 오차를 고려해 99.9f 이상이면 100%로 간주합니다.
        return currentProgress >= 99.9f;
    }
}