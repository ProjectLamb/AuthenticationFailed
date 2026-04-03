using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingManager : MonoBehaviour
{
    private static LoadingManager instance = null;

    public static LoadingManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("No LoadingManager Instance");
            }
            return instance;
        }
    }

    [Header("PC LOADING PANEL")]
    public Canvas PCLoadingCanvas;
    public GameObject PCLoadingPanel;
    public GameObject PCLoadingStat;
    public Image PCLoadingLogo;
    public RawImage PCLoadingBackground;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //PC 로딩화면 키기
    public void LoadingPC()
    {
        PCLoadingPanel.SetActive(true);
        StartCoroutine("LoadingPCEnd");
    }

    IEnumerator LoadingPCEnd()
    {
        yield return new WaitForSeconds(2.5f);
        PCLoadingStat.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        PCLoadingLogo.DOFade(0f, 1f);
        yield return new WaitForSeconds(1.0f);
        PCLoadingBackground.DOFade(0f,1f);
        yield return new WaitForSeconds(1.0f);
        PCLoadingCanvas.enabled = false;
    }
}
