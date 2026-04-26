using UnityEngine;
using UnityEngine.UI;

public class WH_P1_Downloader : MonoBehaviour
{
    public Slider downloadSlider;
    public WH_RpcManager rpcManager;
    public float decayRate = 6.0f;
    public float boostAmount = 2.0f;

    // 인스펙터에서 수정 가능하도록 RTPC 이름을 변수로 빼두면 관리가 편합니다.
    public string rtpcName = "A6_P1_Button_Progress";

    [SerializeField] private float currentProgress = 0f;

    void Update()
    {
        // 1. 프로그레스 값 계산 (시간에 따른 감소 및 0~100 제한)
        if (currentProgress > 0) currentProgress -= decayRate * Time.deltaTime;
        currentProgress = Mathf.Clamp(currentProgress, 0, 100);

        // 2. UI 슬라이더 업데이트
        if (downloadSlider != null) downloadSlider.value = currentProgress / 100f;

        // 3. Wwise RTPC 값 실시간 연동 (핵심!)
        // 글로벌(전역) 사운드에 적용할 경우 gameObject를 뺍니다.
        // 특정 오브젝트에서 나는 소리에만 적용하려면 gameObject 파라미터를 추가하세요.
        AkSoundEngine.SetRTPCValue(rtpcName, currentProgress);
    }

    public void OnClickDownload()
    {
        currentProgress += boostAmount;
        if (rpcManager != null) rpcManager.RequestSpawn();

        // Update에서 이미 처리 중이라 여기에 Wwise 코드를 추가할 필요는 없습니다.
    }

    public bool IsFull() => currentProgress >= 99.9f;
}