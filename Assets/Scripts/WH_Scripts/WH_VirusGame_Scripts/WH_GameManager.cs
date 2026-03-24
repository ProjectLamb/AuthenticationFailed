using UnityEngine;
using System.Collections;

public class WH_GameManager : MonoBehaviour
{
    public GameObject miniGameRoot;
    public Transform p1View, p2View; // Inspector에서 P1_View, P2_View 할당 필수
    public GameObject virusP1, virusP2, clearP1, clearP2;

    public void TriggerVirusPenalty() => StartCoroutine(VirusSequence());
    public void TriggerStageClear() => StartCoroutine(ClearSequence());

    private IEnumerator VirusSequence()
    {
        SetUI(virusP1, virusP2, true);
        yield return new WaitForSeconds(2.0f);
        if (miniGameRoot != null) miniGameRoot.SetActive(false);
    }

    private IEnumerator ClearSequence()
    {
        SetUI(clearP1, clearP2, true);
        yield return new WaitForSeconds(2.0f);
        if (miniGameRoot != null) miniGameRoot.SetActive(false);
    }

    private void SetUI(GameObject p1, GameObject p2, bool state)
    {
        // 1. 결과 텍스트 활성화
        if (p1) p1.SetActive(state);
        if (p2) p2.SetActive(state);

        // 2. [디테일 작업] 해당 뷰의 다른 모든 UI 자식들을 숨깁니다.
        if (p1View != null && p1 != null) HideOthers(p1View, p1);
        if (p2View != null && p2 != null) HideOthers(p2View, p2);
    }

    private void HideOthers(Transform parent, GameObject target)
    {
        // 부모(View) 밑에 있는 모든 자식을 반복문으로 검사합니다.
        foreach (Transform child in parent)
        {
            // 결과 텍스트(target)가 아닌 다른 모든 오브젝트는 비활성화합니다.
            if (child.gameObject != target)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}