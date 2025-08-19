using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReset : MonoBehaviour
{
    public static void RestartScene()
    {

        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex, LoadSceneMode.Single);
      
    }
}
