using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartFreeMode()
    {
        PlayerPrefs.SetString("MODE", "free");
        SceneManager.LoadScene("Reducido");
        
    }

    public void StartGuidedMode()
    {
        PlayerPrefs.SetString("MODE", "guided");
        SceneManager.LoadScene("Reducido");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
