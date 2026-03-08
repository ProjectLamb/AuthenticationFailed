using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ImaginationPanicPlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;
    public float turnSpeed = 15f; // 캐릭터가 도는 속도

    private Rigidbody rb;
    private Vector3 movement;
    private Camera mainCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // 수정된 부분: 무조건 Camera.main을 믿지 않고, "1P_Camera"라는 이름을 가진 카메라를 직접 찾습니다.
        GameObject camObj = GameObject.Find("1P_Camera");
        if (camObj != null)
        {
            mainCamera = camObj.GetComponent<Camera>();
        }
        else
        {
            mainCamera = Camera.main; // 최후의 보루
            Debug.LogWarning("1P_Camera를 찾지 못해 기본 카메라를 사용합니다.");
        }
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 1. 카메라의 앞(Forward)과 오른쪽(Right) 벡터를 가져옵니다.
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;

        // 2. Y축(높이) 성분을 날려버려서 땅바닥 평면과 평행하게 만듭니다.
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // 3. 카메라 방향을 기준으로 입력값을 곱해 최종 이동 방향을 구합니다.
        movement = (camForward * v + camRight * h).normalized;

        // 4. 이동하는 방향으로 캐릭터가 자연스럽게 회전하도록 처리 (선택 사항)
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        // 물리적인 이동 처리
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
