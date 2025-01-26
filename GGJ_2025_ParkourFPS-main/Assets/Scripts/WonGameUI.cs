using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WonGameUI : MonoBehaviour
{
    

    public void GoToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
