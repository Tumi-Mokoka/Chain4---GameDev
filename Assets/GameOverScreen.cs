using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class GameOverScreen : MonoBehaviour
{

    public TextMeshProUGUI gameoverText;

    public void Setup(string text)
    {
        gameObject.SetActive(true);
        gameoverText.text = text;

    }

    public void PlayAgainButton()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
