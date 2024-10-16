using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu2 : MonoBehaviour
{


 
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("GameScene"); 
    }

    public void CloseGame()
    {
        // If you are in the editor, stop play mode
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit(); // Quit the application
        #endif
    }

    public void PlayAi()
    {
        SceneManager.LoadSceneAsync("GameSceneAI");
    }

    public void StartTutorial()
    {
        SceneManager.LoadSceneAsync("Tutorial");
    }

}
