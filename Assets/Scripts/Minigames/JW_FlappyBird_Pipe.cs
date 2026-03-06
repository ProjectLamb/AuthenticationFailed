using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JW_FlappyBird_Pipe : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.localPosition += new Vector3(-0.05f,0,0);

        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(this.gameObject.transform.localPosition.y > -12f)
            {
                this.gameObject.transform.localPosition += new Vector3(0,-0.02f,0);
                Debug.Log("누르고있어!");
            }
        }
    }
}
