using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static GameManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

  

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
    }
}
