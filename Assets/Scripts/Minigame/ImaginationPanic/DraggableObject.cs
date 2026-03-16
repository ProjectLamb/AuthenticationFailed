using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DraggableObject : MonoBehaviour
{
    private Rigidbody rb;

    // 드래그를 위한 변수들
    private float fixedY;
    private Vector3 offset;
    private Plane dragPlane;

    private Camera activeCamera;

    [Header("낙하 제한 설정")]
    [Tooltip("오브젝트가 멈출 최하단 Y 좌표 (바닥을 뚫지 않게 0.5로 설정)")]
    public float stopY = 0.5f;
    private bool isLanded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        GameObject camObj = GameObject.Find("1P_Camera");
        activeCamera = (camObj != null) ? camObj.GetComponent<Camera>() : Camera.main;
    }

    void OnMouseDown()
    {
        fixedY = transform.position.y;
        dragPlane = new Plane(Vector3.up, new Vector3(0, fixedY, 0));

        Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            offset = transform.position - hitPoint;
        }

        // 마우스로 잡았을 때는 물리 무시
        if (rb != null) rb.isKinematic = true;
    }

    void OnMouseDrag()
    {
        Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 newPosition = hitPoint + offset;

            // [추가 방어] 드래그 중에도 절대 stopY(0.5) 밑으로 못 내려가게 막음!
            newPosition.y = Mathf.Max(fixedY, stopY);

            transform.position = newPosition;
        }
    }

    void OnMouseUp()
    {
        // [수정됨] 이전에 있던 rb.isKinematic = false; 를 아예 삭제했습니다.
        // 만약 오브젝트가 아직 공중에 있다면(isLanded == false), 물리력을 다시 켜서 떨어지게 합니다.
        // 하지만 이미 땅에 닿은 상태(isLanded == true)라면 계속 Kinematic(고정) 상태를 유지합니다.
        if (rb != null && !isLanded)
        {
            rb.isKinematic = false;
        }
    }

    void FixedUpdate()
    {
        // 땅에 아직 안 닿았고, 현재 높이가 제한 높이(0.5)보다 낮거나 같다면
        if (!isLanded && transform.position.y <= stopY)
        {
            // 1. 위치를 정확히 0.5로 강제 고정
            transform.position = new Vector3(transform.position.x, stopY, transform.position.z);

            // 2. 바닥에 닿는 순간 물리를 완전히 꺼버려서 단단한 고정석으로 만듦
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.isKinematic = true; // 이거 하나면 아래로 떨어지지도, 밀리지도 않습니다.
            }

            isLanded = true;
        }
    }

    // LateUpdate는 이제 필요 없으므로 삭제했습니다!
}