using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{

    [SerializeField] private AudioSource clicksound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame

    public void LoadGame()
    {
        clicksound.Play();
        SceneManager.LoadSceneAsync("Game");
        
    }
    public void ExitGame()
    {
        clicksound.Play();
        Application.Quit();
        
    }
}
