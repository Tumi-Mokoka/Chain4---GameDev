using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Token : MonoBehaviour
{
    // Start is called before the first frame update
    public string content;
    public string letter;
    public string tokenType;
    public string rarity;
    public int points;
    public int row;
    public int col;
    public TextMeshPro textMeshPro;
    public bool mouseDown;
    public bool isSelected;
    public bool isHolding;

    public int freezeLevel = 0;

    private GameManager gameManager;

    //effects and sounds
    public ParticleSystem explosionFX;
    public ParticleSystem freezeExplodeFX;
    public ParticleSystem isFrozenFx;

    public AudioClip explosionSound;
    public AudioClip freezeSound;
    
    



    private AudioSource tokenAudio;
    private AudioSource tokenAudio2;


    public void Initialize(string letter, string rarity, int points)
    {
        this.letter = letter.ToUpper();
        this.rarity = rarity.ToUpper();
        UpdateTokenText();
        UpdateRarity();
        this.points = points;
    }    

    private void UpdateRarity()
    {
        
    }

    private void UpdateTokenText()
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = letter;
        }

    }
  

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        tokenAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            mouseDown = false;
            isHolding = false;
        }

        UpdateTokenText();
    }


    private void OnMouseDown()
    {

        if (gameManager.turn == 2 && gameManager.p2IsAi)
        {
            return;
        }

        mouseDown = true;
        Debug.Log("click:" +  letter);
        gameManager.HandleSelection(gameObject);
    }


    private void OnMouseEnter()
    {
        if (gameManager.turn == 2 && gameManager.p2IsAi)
        {
            return;
        }

        if (mouseDown)
        {
            Debug.Log("hold:" + letter);
            isHolding = true;
            gameManager.HandleSelection(gameObject);
        }

    }

    public void Select()
    {
        isSelected = true;
        // Activate Halo component
        Behaviour halo = (Behaviour)GetComponent("Halo");
        if (halo != null)
        {
            halo.enabled = true;
        }
    }

    public void Unselect()
    {
        isSelected = false;
        // Deactivate Halo component
        Behaviour halo = (Behaviour)GetComponent("Halo");
        if (halo != null)
        {
            halo.enabled = false;
        }
    }

    public void increaseFreezeLevel(int amount)
    {
        freezeLevel += amount;
    }

    public void decreaseFreezeLevel(int amount)
    {
        if (freezeLevel > 0)
        {
            freezeLevel -= amount;
        }
    }

    public bool IsFrozen()
    {
        return freezeLevel > 0;
    }
    public void Freeze()
    {
        Debug.Log("Freezing token at (row, col): "+ row + " " +col);
        isFrozenFx.Play();
        increaseFreezeLevel(2);
    }

    public void UnFreeze()
    {
        decreaseFreezeLevel(1);
        if (freezeLevel == 0)
        {

            isFrozenFx.Stop();
        }
    }


    public void Explode()
    {
        //trigger explosion fx
        if (tokenType == "bomb")
        {
            // Play explosion sound
            GetComponent<Renderer>().enabled = false;
            tokenAudio.PlayOneShot(explosionSound, 1.0f);
            Destroy(gameObject, explosionFX.main.duration);

            // Play explosion particle effect
            explosionFX.Play();

            return;
        }       
    }

    public void FreezeExplode()
    {
        //trigger explosion fx
        if (tokenType == "freeze")
        {
            // Play freeze sound
            GetComponent<Renderer>().enabled = false;
            tokenAudio.PlayOneShot(freezeSound, 1.0f);
            Destroy(gameObject, 2);

            // Play explosion particle effect
            freezeExplodeFX.Play();

            return;
        }
    }

    public void Downgrade()
    {
        if (rarity == "BRONZE")
        {
            Debug.Log("Exploding bronze at (row, col): " + row + " " + col);
            Destroy(gameObject);
            gameManager.pendingRemoveFromGridTokens.Add(gameObject);
        }
        else
        //downgrade rarity
        {
            if (rarity == "SILVER")
            {
                Debug.Log("Exploding silver at (row, col): " + row + " " + col);
                rarity = "BRONZE";
                points--;

                //swap in bronze material
                Renderer renderer = gameObject.GetComponent<Renderer>();
                renderer.material = gameManager.GetMaterialForRarity("Bronze");
            }
            else
            {
                Debug.Log("Exploding gold at (row, col): " + row + " " + col);
                rarity = "SILVER";
                points--;

                //swap in Silver material
                Renderer renderer = gameObject.GetComponent<Renderer>();
                renderer.material = gameManager.GetMaterialForRarity("Silver");
            }
        }
    }
}
