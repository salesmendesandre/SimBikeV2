using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartFreeMode()
    {
        Route.guided = false;
        SceneManager.LoadScene("Reducido");
        
    }

    public void StartGuidedMode()
    {
        Route.guided = true;
        SceneManager.LoadScene("Reducido");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
