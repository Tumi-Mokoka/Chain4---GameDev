using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
 
        SceneManager.LoadScene("GameScene");
    }

    public void StartTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }
}
