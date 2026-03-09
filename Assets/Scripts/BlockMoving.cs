using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockMoving : MonoBehaviour
{
    [Header("이동 설정")]
    public float speed = 3.0f;
    public float distance = 2.0f;
    public float tolerance = 0.2f;

    private Vector3 startPosition;
    private bool isStopped = false;
    private bool isTouchingRedLine = false;

    private Tween moveTween;
    private Collider myCollider;
    private Collider redLineCollider;

    void Start()
    {
        startPosition = transform.position;
        myCollider = GetComponent<Collider>();
        StartMoving();
    }

    void StartMoving()
    {
        float duration = distance / speed;
        moveTween = transform.DOMoveY(startPosition.y + distance, duration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // ★ 타이머 끝나면 호출 - 블록 처음 상태로 리셋
    public void ResetBlock()
    {
        isStopped = false;
        isTouchingRedLine = false;
        redLineCollider = null;

        // 기존 트윈 정리
        moveTween?.Kill();

        // 원래 위치로 복귀 후 다시 움직이기
        transform.DOMove(startPosition, 0.3f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => StartMoving());
    }

    public bool CheckAndGetTargetY(out float targetY)
    {
        targetY = 0f;
        if (!isTouchingRedLine || redLineCollider == null) return false;

        float myHalfHeight = myCollider.bounds.extents.y;
        if (transform.position.y < redLineCollider.transform.position.y)
            targetY = redLineCollider.bounds.min.y - myHalfHeight;
        else
            targetY = redLineCollider.bounds.max.y + myHalfHeight;

        float distanceDiff = Mathf.Abs(transform.position.y - targetY);
        if (distanceDiff > tolerance) return false;

        return true;
    }

    public void StopAndAlignRPC(float targetY)
    {
        if (isStopped) return;
        isStopped = true;
        moveTween.Kill();
        transform.DOMoveY(targetY, 0.1f).SetEase(Ease.OutBack);
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("RedLine")) { isTouchingRedLine = true; redLineCollider = other; } }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("RedLine")) { isTouchingRedLine = false; if (redLineCollider == other) redLineCollider = null; } }
}