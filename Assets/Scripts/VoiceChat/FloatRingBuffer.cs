using System;

// Unity 오디오 스레드와 Wwise 오디오 스레드 사이에서 안전하게 데이터를 전달합니다.
public class FloatRingBuffer
{
    private readonly float[] buffer;
    private readonly int capacity;
    private volatile int writePos = 0;
    private volatile int readPos = 0;

    public FloatRingBuffer(int size)
    {
        capacity = size;
        buffer = new float[capacity];
    }

    // [Unity 스레드] Photon에서 들어온 오디오 데이터를 밀어 넣습니다.
    public void Write(float[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            buffer[writePos] = data[i];
            writePos = (writePos + 1) % capacity;
        }
    }

    // [Wwise 스레드] Wwise가 요구할 때 버퍼에서 데이터를 빼서 줍니다.
    public void Read(float[] output, out bool isUnderrun)
    {
        isUnderrun = false;
        for (int i = 0; i < output.Length; i++)
        {
            if (readPos == writePos)
            {
                // 버퍼가 비어있다면 0(묵음)을 채워 팝핑 노이즈를 방지합니다.
                output[i] = 0f;
                isUnderrun = true;
            }
            else
            {
                output[i] = buffer[readPos];
                readPos = (readPos + 1) % capacity;
            }
        }
    }
}
