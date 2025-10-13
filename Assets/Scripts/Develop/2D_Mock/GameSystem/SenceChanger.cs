using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SenceChanger : MonoBehaviour
{
    public void SwitchScene(string Name)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(Name);
    }

    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("GameSystem");

        if (objs.Length > 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}
