using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FolderHell_Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private int limitTime = 30;

    void Start()
    {
        timerText.text = limitTime.ToString();
        StartCoroutine("TimeLimit");
    }

    IEnumerator TimeLimit()
    {
        while(limitTime > 0)
        {
            limitTime -= 1;
            timerText.text = limitTime.ToString();
            yield return new WaitForSeconds(1f);
        }
    }
}
