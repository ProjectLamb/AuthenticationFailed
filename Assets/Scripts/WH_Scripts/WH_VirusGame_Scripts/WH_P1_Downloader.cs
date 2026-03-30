using UnityEngine;
using UnityEngine.UI;

public class WH_P1_Downloader : MonoBehaviour
{
    public Slider downloadSlider;
    public WH_RpcManager rpcManager;
    public float decayRate = 0.5f;
    public float boostAmount = 3.0f;

    [SerializeField] private float currentProgress = 0f;

    void Update()
    {
        if (currentProgress > 0) currentProgress -= decayRate * Time.deltaTime;
        currentProgress = Mathf.Clamp(currentProgress, 0, 100);
        if (downloadSlider != null) downloadSlider.value = currentProgress / 100f;
    }

    public void OnClickDownload()
    {
        currentProgress += boostAmount;
        if (rpcManager != null) rpcManager.RequestSpawn();
    }

    public bool IsFull() => currentProgress >= 99.9f;
}