using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class TitleManager : MonoBehaviour
{

    public RawImage fadePanel;
    private bool IsStart = false;

    public void CreateRoom()
    {
        
    }

    public void StartGame()
    {
        if (!IsStart)
        {
            IsStart = true;
            StartCoroutine("FadePanel");
        }
    }

    IEnumerator FadePanel()
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.DOFade(1f,1f);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Carrot");
    }
}
