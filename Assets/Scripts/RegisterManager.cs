using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class RegisterManager : MonoBehaviour
{

    private AudioSource audioSource;
    public AudioClip alertSound;

    public GameObject regWindow;
    public GameObject alert;
    

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void RegClick()
    {
        alert.SetActive(true);
        audioSource.PlayOneShot(alertSound);
        StartCoroutine("AlertOff");
    }

    public void LauncherClick()
    {
        regWindow.SetActive(true);
        Debug.Log("클릭은 되고있어");
    }

    IEnumerator AlertOff()
    {
        yield return new WaitForSeconds(2f);
        alert.SetActive(false);
        GameManager.Instance.MiniGame();
    }
}
