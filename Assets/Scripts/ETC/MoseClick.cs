using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MoseClick : MonoBehaviour
{

    private AudioSource audioSource;
    public AudioClip clickSound;
    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
