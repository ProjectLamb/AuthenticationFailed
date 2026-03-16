using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class BlockMoving : MonoBehaviour
{
    [Header("이동 설정")]
    public float speed = 3.0f;
    public float distance = 2.0f;
    public float tolerance = 0.2f;

    [Header("자식 오브젝트 연결")]
    public TextMeshPro turnText;   // 자식 TurnText 연결
    public GameObject outlineRoot; // 자식 Outline 오브젝트 연결

    private Renderer[] lineRenderers; // Line1~4 의 Renderer
    private Material outlineMat;
    private Tween blinkTween;
    private Tween glowTween;

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

        // TurnText 끔
        if (turnText != null) turnText.gameObject.SetActive(false);

        // Outline 초기화
        if (outlineRoot != null)
        {
            // Line1~4 Renderer 수집
            lineRenderers = outlineRoot.GetComponentsInChildren<Renderer>();

            if (lineRenderers.Length > 0)
            {
                // 공유 머티리얼 생성 (Unlit/Color)
                outlineMat = new Material(Shader.Find("Unlit/Color"));
                outlineMat.color = Color.clear;

                foreach (var r in lineRenderers)
                    r.material = outlineMat;
            }

            outlineRoot.SetActive(false);
        }

        StartMoving();
    }

    void StartMoving()
    {
        float duration = distance / speed;
        moveTween = transform.DOMoveY(startPosition.y + distance, duration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void SetTurnText(bool isActive, bool isP1 = true)
    {
        blinkTween?.Kill(); blinkTween = null;
        glowTween?.Kill(); glowTween = null;

        // 텍스트
        if (turnText != null)
        {
            turnText.gameObject.SetActive(isActive);
            if (isActive)
            {
                turnText.text = isP1 ? "P1" : "P2";
                turnText.color = isP1 ? Color.blue : Color.red;
                turnText.alpha = 1f;
                blinkTween = turnText.DOFade(0.1f, 0.4f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            else turnText.alpha = 1f;
        }

        // 테두리 글로우
        if (outlineRoot != null && outlineMat != null)
        {
            outlineRoot.SetActive(isActive);

            if (isActive)
            {
                Color c = isP1
                    ? new Color(0.2f, 0.4f, 1.0f, 1f)  // 파란색
                    : new Color(1.0f, 0.2f, 0.2f, 1f); // 빨간색

                outlineMat.color = c;

                // 깜빡임
                Color fadeOut = new Color(c.r, c.g, c.b, 0f);
                glowTween = DOTween.To(
                    () => outlineMat.color,
                    v => outlineMat.color = v,
                    fadeOut, 0.4f
                ).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                outlineMat.color = Color.clear;
            }
        }
    }

    public void ResetBlock()
    {
        isStopped = false;
        isTouchingRedLine = false;
        redLineCollider = null;

        SetTurnText(false);
        moveTween?.Kill();
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
        SetTurnText(false);
        moveTween.Kill();
        transform.DOMoveY(targetY, 0.1f).SetEase(Ease.OutBack);
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("RedLine")) { isTouchingRedLine = true; redLineCollider = other; } }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("RedLine")) { isTouchingRedLine = false; if (redLineCollider == other) redLineCollider = null; } }
}