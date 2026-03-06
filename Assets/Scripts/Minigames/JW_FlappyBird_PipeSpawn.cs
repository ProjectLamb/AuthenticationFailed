using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JW_FlappyBird_PipeSpawn : MonoBehaviour
{
    public GameObject pipes;
    private float nowTime;
    private float makeTime = 2f;
    // Start is called before the first frame update
    void Start()
    {
        nowTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - nowTime > makeTime)
        {
            nowTime = Time.time;
            GameObject newPipe = Instantiate(pipes);
            newPipe.transform.parent = gameObject.transform;

            float randomY = Random.Range(-12f,12f);
            newPipe.transform.localPosition = new Vector3(46.0668335f,randomY,-30f);
        }
    }
}
