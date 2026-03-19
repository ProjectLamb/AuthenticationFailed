using UnityEngine;
using Photon.Voice.Unity;

[RequireComponent(typeof(Speaker))]
[RequireComponent(typeof(AudioSource))]
public class PhotonWwiseReceiver : MonoBehaviour
{
    [Header("Wwise Settings")]
    public AK.Wwise.Event voicePlayEvent;

    private const int BUFFER_SIZE = 48000;
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
        ringBuffer.Write(data);
    }

    // [Wwise Audio 스레드] 포맷 정보 전달
    void AudioFormatDelegate(uint playingID, AkAudioFormat format)
    {
        format.uSampleRate = (uint)AudioSettings.outputSampleRate;

        // [수정됨] uChannelMask 대신 최신 API인 channelConfig 사용
        // 4 = AK_SPEAKER_SETUP_MONO (모노/1채널 마이크 세팅)
        format.channelConfig.SetStandard(4);
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