using UnityEngine;
using System.Collections;

public class WH_GameManager : MonoBehaviour
{
    public GameObject miniGameRoot;
    public Transform p1View, p2View;
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
        if (p1) p1.SetActive(state);
        if (p2) p2.SetActive(state);
        HideOthers(p1View, p1);
        HideOthers(p2View, p2);
    }

    private void HideOthers(Transform parent, GameObject target)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject != target) child.gameObject.SetActive(false);
        }
    }
}