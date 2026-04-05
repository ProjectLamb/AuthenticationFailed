using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DeadZone : MonoBehaviour
{
    [Header("부활 위치 (태초마을)")]
    [Tooltip("플레이어가 부활할 위치를 담은 빈 오브젝트를 넣으세요")]
    public Transform spawnPoint;

    // 누군가 데드존 영역(Trigger)에 들어왔을 때 발동
    private void OnTriggerEnter(Collider other)
    {
        // 들어온 오브젝트의 태그가 "Player"인지 확인
        if (other.CompareTag("Player"))
        {
            // 1. 플레이어를 스폰 포인트 위치로 순간이동
            other.transform.position = spawnPoint.position;

            // 2. [핵심] Rigidbody의 물리적 가속도를 초기화 (안 그러면 부활하자마자 또 바닥을 뚫고 추락함)
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;         // 이동 가속도 0
                rb.angularVelocity = Vector3.zero;  // 회전 가속도 0
            }

            Debug.Log("플레이어가 추락하여 태초마을로 강제 송환되었습니다.");
        }
    }
}