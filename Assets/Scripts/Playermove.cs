using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playermove : MonoBehaviour
{
    public float speed = 5f;
    Rigidbody2D rb;

    //이동 범위 설정(Panel 기준)

    public float minX = -4.0f;
    public float maxX = 4.0f;
    public float minY = -4.5f;
    public float maxY = -3.0f;
    // Start is called before the first frame update
    void Start()
    {
        rb  = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //WASD를 입력받아 움직임
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        rb.velocity = new Vector2(x * speed, y * speed);

        // 값이 최소보다 작으면 최소를, 최대보다 크면 최대를 반환
        // 플레이어의 현재 X위치가 minX와 maxX 사이를 절대 못 벗어나게 꽉 잡는 역할
        float clampX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampY = Mathf.Clamp(transform.position.y, minY, maxY);

        // 계산된 좌표를 플레이어의 실제 포지션에 대입한다
        transform.position = new Vector2(clampX, clampY);

    }
}
