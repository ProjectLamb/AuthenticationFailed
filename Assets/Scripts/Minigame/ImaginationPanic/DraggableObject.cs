using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DraggableObject : MonoBehaviour
{
    private Rigidbody rb;

    // 드래그를 위한 변수들
    private float fixedY; // 고정시킬 높이
    private Vector3 offset; // 클릭한 위치와 오브젝트 중심의 오차
    private Plane dragPlane; // 높이를 고정할 가상의 바닥 평면

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnMouseDown()
    {
        // 1. 클릭하는 순간의 Y축 높이를 기억합니다.
        fixedY = transform.position.y;

        // 2. 그 높이를 기준으로 무한히 넓은 가상의 바닥(Plane)을 하나 만듭니다.
        dragPlane = new Plane(Vector3.up, new Vector3(0, fixedY, 0));

        // 3. 카메라에서 마우스 클릭 위치로 레이저(Ray)를 쏩니다.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 레이저가 가상의 바닥(dragPlane)에 닿았다면
        if (dragPlane.Raycast(ray, out float distance))
        {
            // 닿은 지점의 3D 좌표를 구하고, 오브젝트 중심과의 오차(Offset)를 계산합니다.
            Vector3 hitPoint = ray.GetPoint(distance);
            offset = transform.position - hitPoint;
        }

        // 드래그하는 동안 중력 때문에 떨어지지 않도록 고정
        if (rb != null) rb.isKinematic = true;
    }

    void OnMouseDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (dragPlane.Raycast(ray, out float distance))
        {
            // 마우스가 이동한 위치의 바닥 좌표를 구합니다.
            Vector3 hitPoint = ray.GetPoint(distance);

            // 오차를 더해서 새 위치를 잡고, Y축은 한 번 더 확실하게 고정합니다.
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