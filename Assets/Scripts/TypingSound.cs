using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class TypingSound : MonoBehaviour
{

    private AudioSource audioSource;
    public AudioClip[] typingSound;
    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown && 
        !Input.GetMouseButtonDown(0) && 
        !Input.GetMouseButtonDown(1) && 
        !Input.GetMouseButtonDown(2))
        {
            int ran = Random.Range(0, typingSound.Length);
            audioSource.PlayOneShot(typingSound[ran]);
        }
    }
}
