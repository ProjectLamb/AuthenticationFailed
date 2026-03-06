using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JW_FlappyBird_BirdControl : MonoBehaviour
{

    public Canvas canvasBird;
    private Rigidbody rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        canvasBird.worldCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            rigid.velocity = new Vector3(0,0,0);
            rigid.AddForce(0,300,0);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log("GameOver");
    }
}
