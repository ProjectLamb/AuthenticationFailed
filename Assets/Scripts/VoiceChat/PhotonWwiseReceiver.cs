using UnityEngine;
using Photon.Voice.Unity;

[RequireComponent(typeof(Speaker))]
[RequireComponent(typeof(AudioSource))]
public class PhotonWwiseReceiver : MonoBehaviour
{
    [Header("Wwise Settings")]
    public AK.Wwise.Event voicePlayEvent;

    // 기존: private const int BUFFER_SIZE = 48000; 

    // 변경: 0.2초 분량의 버퍼만 유지 (네트워크 튐 현상만 방어할 최소한의 크기)
    private const int BUFFER_SIZE = 9600;
    private FloatRingBuffer ringBuffer;
    private uint playingId;
    private AudioSource unityAudioSource;

    void Awake()
    {
        ringBuffer = new FloatRingBuffer(BUFFER_SIZE);
        unityAudioSource = GetComponent<AudioSource>();

        // 믹서로 소리를 빼돌렸다면 mute는 false로 두는 것이 안전합니다.
        // unityAudioSource.mute = true; 
        unityAudioSource.loop = true;
    }

    void Start()
    {
        // 내 캐릭터면 메아리 방지를 위해 Wwise 브릿지를 끕니다.
        if (GetComponent<Photon.Pun.PhotonView>().IsMine)
        {
            this.enabled = false;
            return;
        }

        playingId = voicePlayEvent.Post(gameObject);

        // [수정됨] Wwise 최신 API에 맞게 매개변수 순서를 변경했습니다.
        // 3번째 인자: 샘플 데이터 / 4번째 인자: 포맷 데이터
        AkAudioInputManager.PostAudioInputEvent(
            voicePlayEvent.Id,
            gameObject,
            AudioSamplesDelegate,
            AudioFormatDelegate
        );
    }

    // [Unity Audio 스레드]
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (data == null || ringBuffer == null) return;

        // [핵심 해결책] 유니티가 스테레오(2채널)로 데이터를 줬다면?
        if (channels == 2)
        {
            // 배열 크기를 절반으로 줄인 모노 배열을 만듭니다.
            float[] monoData = new float[data.Length / 2];

            for (int i = 0; i < monoData.Length; i++)
            {
                // 왼쪽(i*2) 소리와 오른쪽(i*2+1) 소리를 더해서 반으로 나눔 (모노 다운믹스)
                monoData[i] = (data[i * 2] + data[i * 2 + 1]) * 0.5f;
            }

            // 압축된 모노 데이터를 Wwise 물통(링버퍼)에 넣습니다.
            ringBuffer.Write(monoData);
        }
        else
        {
            // 만약 이미 모노(1채널)로 들어오고 있다면 그대로 넣습니다.
            ringBuffer.Write(data);
        }
    }

    // [Wwise Audio 스레드] 포맷 정보 전달
    void AudioFormatDelegate(uint playingID, AkAudioFormat format)
    {
        // ✅ 유니티 엔진이 실제로 뱉어내는 샘플레이트를 동적으로 가져와서 꽂아주는 게 가장 완벽합니다.
        format.uSampleRate = (uint)AudioSettings.outputSampleRate;

        format.channelConfig.SetStandard(4); // 모노 설정 (이건 그대로 유지)
    }

    // [Wwise Audio 스레드] 버퍼 데이터 전달
    bool AudioSamplesDelegate(uint playingID, uint channels, float[] samples)
    {
        bool isUnderrun;
        ringBuffer.Read(samples, out isUnderrun);
        return true;
    }

    void OnDestroy()
    {
        if (playingId != 0)
        {
            AkSoundEngine.StopPlayingID(playingId);
        }
    }
}