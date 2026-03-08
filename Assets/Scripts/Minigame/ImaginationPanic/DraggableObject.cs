using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DraggableObject : MonoBehaviour
{
    private Rigidbody rb;

    // 드래그를 위한 변수들
    private float fixedY; // 고정시킬 높이
    private Vector3 offset; // 클릭한 위치와 오브젝트 중심의 오차
    private Plane dragPlane; // 높이를 고정할 가상의 바닥 평면

    private Camera activeCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 1P_Camera를 직접 찾아서 연결
        GameObject camObj = GameObject.Find("1P_Camera");
        activeCamera = (camObj != null) ? camObj.GetComponent<Camera>() : Camera.main;
    }

    void OnMouseDown()
    {
        fixedY = transform.position.y;
        dragPlane = new Plane(Vector3.up, new Vector3(0, fixedY, 0));

        // Camera.main 대신 activeCamera 사용
        Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            offset = transform.position - hitPoint;
        }

        if (rb != null) rb.isKinematic = true;
    }

    void OnMouseDrag()
    {
        // Camera.main 대신 activeCamera 사용
        Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 newPosition = hitPoint + offset;
            newPosition.y = fixedY;

            transform.position = newPosition;
        }
    }

    void OnMouseUp()
    {
        // 마우스를 놓으면 다시 물리(중력 등) 적용
        if (rb != null) rb.isKinematic = false;
    }

    [Header("낙하 제한 설정")]
    public float stopY = 0f; // 징검다리가 멈출 Y 좌표
    private bool isLanded = false;

    void FixedUpdate()
    {
        // 땅에 아직 안 닿았고, 현재 높이가 제한 높이보다 낮거나 같다면
        if (!isLanded && transform.position.y <= stopY)
        {
            // 1. 위치를 정확히 제한 높이로 고정
            transform.position = new Vector3(transform.position.x, stopY, transform.position.z);

            // 2. Y축 이동만 영구적으로 잠가버림 (X, Z축 이동은 드래그로 가능)
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.constraints |= RigidbodyConstraints.FreezePositionY;
            }

            isLanded = true;
        }
    }
}