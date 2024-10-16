using UnityEngine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Slideshow : MonoBehaviour
{
    public Image slideshowImage;
    public Sprite[] slides;  // Array of screenshots
    private int currentSlide = 0;

    void Start()
    {
        // Display the first slide
        slideshowImage.sprite = slides[currentSlide];
    }

    public void NextSlide()
    {
        if (currentSlide < slides.Length - 1)
        {
            currentSlide++;
            slideshowImage.sprite = slides[currentSlide];
        }
    }

    public void PreviousSlide()
    {
        if (currentSlide > 0)
        {
            currentSlide--;
            slideshowImage.sprite = slides[currentSlide];
        }
    }

    public void Exit()
    {
        SceneManager.LoadScene("MainMenu");
    }
}