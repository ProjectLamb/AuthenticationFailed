using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("No GameManagerInstance");
            }
            return instance;
        }
    }

    public GameObject[] miniGames;

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

    public void MiniGame()
    {
        int ran = Random.Range(0,miniGames.Length);
        Vector3 spawnPos = new Vector3(-8.84240913f,3.17331696f,111.747986f);

        // 생성 (프리팹, 위치, 회전값)
        // Quaternion.identity는 '회전 없음'을 의미합니다.
        Instantiate(miniGames[ran], spawnPos, Quaternion.identity);
    }
}
