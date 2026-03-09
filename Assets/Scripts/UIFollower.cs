using UnityEngine;

public class UIFollower : MonoBehaviour
{
    [Header("따라다닐 대상")]
    public Transform target; // 플레이어 오브젝트를 여기에 넣습니다.

    [Header("위치 보정값")]
    public Vector3 offset;   // 플레이어 몸통이 아니라 머리 위에 띄우기 위한 값

    // Update 대신 LateUpdate를 쓰면 플레이어가 이동한 직후에 UI가 따라가서 덜덜 떨리는 현상(Jitter)이 없습니다.
    void LateUpdate()
    {
        if (target != null)
        {
            // 플레이어의 현재 위치에 오프셋(보정값)을 더해서 텍스트 위치를 업데이트
            transform.position = target.position + offset;
        }
    }
}