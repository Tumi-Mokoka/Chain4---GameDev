using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.UI;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{

    //AI
    public bool p2IsAi = true;
    public int aiPowerUpUsageRate = 15;
    public int aiMaxChainLength = 4;
    public List<List<GameObject>> allChains;



    //Game Over
    public int maxScore = 10;
    public GameOverScreen gameOverScreen;
    

    //default formation time
    public float formationTime = 10f;


    public List<GameObject> sequence = new List<GameObject>();
    public int minWordLength = 3;
    public string sequenceString = "";
    public TextMeshProUGUI displayText;
    public int turn = 1;        //1 or 2
    public string gamePhase = "selection"; //pre-selection or selection or formation or gameover//


    public Image progressbar;  // Slider for the timer bar
    public Button skipButton; // Button to skip the turn
    public TextMeshProUGUI timerText;  // Reference to the Timer TextMeshPro object
    private bool timerRunning = false; // Track if the timer is currently running

    public List<GameObject> pendingRemoveFromGridTokens;    

    public List<GameObject> gridTokens;

    public GameObject[] spawnLocs;
    public GameObject tokenPrefab;
    public GameObject bombTokenPrefab;
    public GameObject freezeTokenPrefab;

    public GameObject[] p1BankLocations;
    public GameObject[] p2BankLocations;

    public string[] p1Bank;
    public List<GameObject> p1BankObjs = new List<GameObject>();

    public string[] p2Bank;
    public List<GameObject> p2BankObjs = new List<GameObject>();

    public List<string> foundWords = new List<string>();
    public TextMeshProUGUI foundWordsText;

    private List<string> dictionary = new List<string>();

    //AI vocab, file to load dependent on difficulty
    public List<string> aiVocabulary = new List<string>();


    // variables for player letter selection
    public GameObject selectedToken; // Stores the selected token from the bank
    public bool tokenSelected = false; // Tracks if the player has selected a token


    

    public Material Bronze;
    public Material Silver;
    public Material Gold;

    public int player1Score = 0;
    public int player2Score = 0;

    public TextMeshProUGUI player1ScoreText; // UI Text for displaying Player 1 score
    public TextMeshProUGUI player2ScoreText; // UI Text for displaying Player 2 score

    public TMPro.TextMeshProUGUI player1TurnText; // TextMeshPro for "Player 1"
    public TMPro.TextMeshProUGUI player2TurnText; // TextMeshPro for "Player 2"

    //game logic functions (AI or human can use these methods as an interface to play the game)
    public void SpawnTokenInColumn(int colNumber) //spawn token at selected column
    {
        Debug.Log("spawning token in col: " + colNumber);
        
        //only instantiate if game in selection phase (add later)
        //spawn token as a function of letter bank selection for current turn (add later)

        Instantiate(tokenPrefab, spawnLocs[colNumber - 1].transform.position, tokenPrefab.transform.rotation);

        TakeTurn();
    }

    public void InitializePlayerBanks()
    {

        Debug.Log("Initializing Player Banks");

        for (int i = 0; i < 8; i++)
        {
            (string letter, string rarity, int points) tokenData = generateToken();
            GameObject token1 = Instantiate(tokenPrefab, p1BankLocations[i].transform.position, tokenPrefab.transform.rotation);
            Token tokenScript1 = token1.GetComponent<Token>();
            tokenScript1.Initialize(tokenData.letter, tokenData.rarity, tokenData.points);

            // Assign material based on rarity
            Renderer renderer1 = token1.GetComponent<Renderer>();
            renderer1.material = GetMaterialForRarity(tokenData.rarity);

            p1BankObjs.Add(token1);


            (string letter, string rarity, int points) tokenData2 = generateToken();
            GameObject token2 = Instantiate(tokenPrefab, p2BankLocations[i].transform.position, tokenPrefab.transform.rotation);
            Token tokenScript2 = token2.GetComponent<Token>();
            tokenScript2.Initialize(tokenData2.letter, tokenData2.rarity, tokenData2.points);

            Renderer renderer2 = token2.GetComponent<Renderer>();
            renderer2.material = GetMaterialForRarity(tokenData2.rarity);

            p2BankObjs.Add(token2);

        }

        //add powerups
        GameObject bomb1 = Instantiate(bombTokenPrefab, p1BankLocations[8].transform.position, bombTokenPrefab.transform.rotation);
        GameObject bomb2 = Instantiate(bombTokenPrefab, p2BankLocations[8].transform.position, bombTokenPrefab.transform.rotation);
        p1BankObjs.Add(bomb1);
        p2BankObjs.Add(bomb2);

        GameObject freeze1 = Instantiate(freezeTokenPrefab, p1BankLocations[9].transform.position, freezeTokenPrefab.transform.rotation);
        GameObject freeze2 = Instantiate(freezeTokenPrefab, p2BankLocations[9].transform.position, freezeTokenPrefab.transform.rotation);
        p1BankObjs.Add(freeze1);
        p2BankObjs.Add(freeze2);


        Debug.Log("Player 1 Bank = " +  p1BankObjs.Count);
        Debug.Log("Player 2 Bank = " + p2BankObjs.Count);

    }

    public Material GetMaterialForRarity(string rarity)
    {
        switch (rarity)
        {
            case "Bronze":
                return Bronze;
            case "Silver":
                return Silver;
            case "Gold":
                return Gold;
            default:
                return Bronze;  // Default to common if rarity is not recognized
        }
    }

    public void TakeTurn()
    {
        //switch to formation phase allowing player to select from 
        gamePhase = "formation";

        //trigger count down timer here
        StartCoroutine(FormationPhaseTimer());

        // Continue with switching turns when the timer ends
        Debug.Log("Formation phase started for player " + turn);


    }

    private IEnumerator FormationPhaseTimer()
    {
        float remainingTime = formationTime;  // Set timer to 10 seconds
        timerRunning = true;

        // Make the timer text visible when the timer starts
        timerText.gameObject.SetActive(true);

        // Make the timer bar visible when the timer starts
        progressbar.gameObject.SetActive(true);
        progressbar.fillAmount = 1f;

        skipButton.gameObject.SetActive(true);  // Show the skip button
        skipButton.onClick.AddListener(SkipTurn);  // Attach SkipTurn method to the button


        while (remainingTime > 0 && timerRunning)
        {
            // Update the text on the screen
            timerText.text = Mathf.CeilToInt(remainingTime).ToString() + "s";

            // Update the slider value
            progressbar.fillAmount = remainingTime / formationTime;

            // Wait for 1 second
            yield return new WaitForSeconds(1f);

            // Decrease the remaining time
            remainingTime -= 1f;
        }

        // After the timer ends, hide the text
        timerText.gameObject.SetActive(false);

        // After the timer ends, hide the timer bar and skip button
        progressbar.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        skipButton.onClick.RemoveListener(SkipTurn);  // Detach the listener to avoid duplicates


        // End the formation phase and switch turns
        UnselectAll();
        switchTurns();
        DecreaseFreezeLevelAll();
        gamePhase = "selection";
        timerRunning = false;

        if (isGameover())
        {
            string gameoverText;
            if (player1Score == player2Score)
            {
                gameoverText = "Draw";
            }
            else if (player1Score > player2Score)
            {
                gameoverText = "Player 1 Wins";
            }
            else
            {
                gameoverText = "Player 2 Wins";
            }

            gameOverScreen.Setup(gameoverText);
            gamePhase = "gameOver";
            Debug.Log("Gameover!");
        }

        if (turn == 2 && p2IsAi && gamePhase != "gameOver")
        {
            Debug.Log("Simulating AI thinking");
            int thinkTime = UnityEngine.Random.Range(3, 6);
            yield return new WaitForSeconds(thinkTime);
            Debug.Log("Finished thinking");

            //randomly decide to use a powerup
            int decision = UnityEngine.Random.Range(0, aiPowerUpUsageRate);
            Debug.Log("Powerup decision: " + decision);
            
            if (decision == 0 && p2BankObjs.Count > 8)
            {

                int chosenCol = -1;

                //get random column that is not full and not empty
                chosenCol = GetRandomColNotFullNotEmpty();
                
                
                //else get random column that is not Full;
                if (chosenCol == -1)
                {
                    chosenCol = GetRandomColNotFull();
                }

                //decide which powerup to use
                if (p2BankObjs.Count == 10)
                {
                    int decision2 = UnityEngine.Random.Range(0, 2);
                    selectedToken = p2BankObjs[p2BankObjs.Count - 1 - decision2];
                }
                else
                {
                    selectedToken = p2BankObjs[8];

                }

                
                yield return new WaitForSeconds(1);
                DropTokenInColumn(chosenCol);

                yield return new WaitForSeconds(2);
            }

            
            
            int bankIdx = UnityEngine.Random.Range(0, 7);
            int playCol = GetRandomColNotFull();

            List<List<GameObject>> allChains = GetAllValidChains(gridTokens, aiMaxChainLength);
                                 
            //select token to drop
            selectedToken = p2BankObjs[bankIdx];
            yield return new WaitForSeconds(2);
            
            
            DropTokenInColumn(playCol);


            //simulate selection
            Debug.Log("Selecting");
            foreach (List<GameObject> seq in allChains)
            {
                if (timerRunning)
                {
                    foreach (GameObject tok in seq)
                    {
                        HandleSelection(tok);
                        yield return new WaitForSeconds(1.5f);
                    }
                    HandleSelection(seq[seq.Count - 1]);
                    yield return new WaitForSeconds(UnityEngine.Random.Range(1, 11));
                }
                string str = ToString(seq);
            }


        }
    }

    public List<List<GameObject>> GetAllValidChains(List<GameObject> gridState, int maxLength)
    {
        List<List<GameObject>> chains = new List<List<GameObject>>();

        // Helper method to perform DFS and find chains
        void FindChains(GameObject currentToken, List<GameObject> currentChain, HashSet<GameObject> visited, int maxLength)
        {
            Token currentTokenScript = currentToken.GetComponent<Token>();

            // If token is frozen, it can't be part of a valid chain
            if (currentTokenScript.IsFrozen())
            {
                return;
            }

            // Add the current token to the chain
            currentChain.Add(currentToken);
            visited.Add(currentToken);

            // If the current chain length is between 3 and maxLength and it's a valid word, add to results
            if (currentChain.Count >= 3 && currentChain.Count <= maxLength)
            {
                string chainString = ToString(currentChain);
                if (InDictionary(chainString) && !AlreadyFound(chainString))
                {
                    chains.Add(new List<GameObject>(currentChain)); // Add a copy of the current chain
                }
            }

            // If the chain has reached the max length, stop exploring further
            if (currentChain.Count == maxLength)
            {
                currentChain.RemoveAt(currentChain.Count - 1);
                visited.Remove(currentToken);
                return;
            }

            // Explore adjacent tokens to extend the chain
            foreach (GameObject neighbor in gridState)
            {
                if (!visited.Contains(neighbor) && Adjacent(currentToken, neighbor))
                {
                    FindChains(neighbor, currentChain, visited, maxLength);
                }
            }

            // Backtrack: remove the current token from the chain and visited set
            currentChain.RemoveAt(currentChain.Count - 1);
            visited.Remove(currentToken);
        }

        // Iterate through all tokens in the grid and attempt to form chains
        foreach (GameObject token in gridState)
        {
            List<GameObject> currentChain = new List<GameObject>();
            HashSet<GameObject> visited = new HashSet<GameObject>(); // Keep track of visited tokens in the current chain
            FindChains(token, currentChain, visited, maxLength);

            // If we already have 3 chains, we can return early as we only need 3 or less
            if (chains.Count >= 3)
            {
                return chains;
            }
        }

        return chains;
    }


    public int GetRandomColNotFull()
    {
        List<int> notFull = new List<int>();
        for (int i = 1; i <= 7; i++)
        {
            if (ColumnCount(i) < 6)
            {
                notFull.Add(i);
            }
        }
        if (notFull.Count == 0)
        {
            Debug.Log("GET RANDOMCOL NOT FULL: " + "-1");
            return -1;
            
        }

        int result = notFull[UnityEngine.Random.Range(0, notFull.Count)];
        Debug.Log("GET RANDOMCOL NOT FULL: " + result);


        return result;

    }

    public List<int> GetColNotFullList()
    {
        List<int> notFull = new List<int>();
        for (int i = 1; i <= 7; i++)
        {
            if (ColumnCount(i) < 6)
            {
                notFull.Add(i);
            }
        }
        return notFull;

    }

    public int GetRandomColNotFullNotEmpty()
    {
        List<int> notFullNotEmpty = new List<int>();
        for (int i = 1; i <= 7; i++)
        {
            if (ColumnCount(i) < 6 && ColumnCount(i) > 0)
            {
                notFullNotEmpty.Add(i);
            }
        }
        if (notFullNotEmpty.Count == 0)
        {
            Debug.Log("Random col not empty not full: " + "-1");
            return -1;
        }

        int result = notFullNotEmpty[UnityEngine.Random.Range(0, notFullNotEmpty.Count)];
        Debug.Log("Random col not empty not full: " + result);
        return result;
    }

        private void SkipTurn()
    {
        if (timerRunning)
        {
            timerRunning = false;  // Stop the timer
            progressbar.gameObject.SetActive(false);  // Hide the timer bar
            skipButton.gameObject.SetActive(false);  // Hide the skip button
            UnselectAll();
            //switchTurns();  // End the current turn
            gamePhase = "selection";  // Move to the next phase
        }
    }

    public void switchTurns() 
    {
        if (turn == 1)
        {
            turn = 2;
        }
        else
        {
            turn = 1;
        }

        // Set the gamePhase back to selection for the next player's turn
        gamePhase = "selection";

        // Update the player turn highlight
        UpdatePlayerTurnHighlight();
    }


    public bool Adjacent(GameObject token, int row, int col)
    {
        if (token == null)
        {
            return false;
        }
        Token t = token.GetComponent<Token>();

        // Check horizontal adjacency
        if (t.row == row && Math.Abs(t.col - col) == 1)
        {
            return true;
        }

        // Check vertical adjacency
        if (t.col == col && Math.Abs(t.row - row) == 1)
        {
            return true;
        }

        // Check diagonal adjacency
        if (Math.Abs(t.row - row) == 1 && Math.Abs(t.col - col) == 1)
        {
            return true;
        }

        // If none of the above conditions are met, they are not adjacent
        return false;
    }

    public bool Adjacent(GameObject token1, GameObject token2)
    {
        // Get the Token component of both tokens
        Token t1 = token1.GetComponent<Token>();
        Token t2 = token2.GetComponent<Token>();

        // Check horizontal adjacency
        if (t1.row == t2.row && Math.Abs(t1.col - t2.col) == 1)
        {
            return true;
        }

        // Check vertical adjacency
        if (t1.col == t2.col && Math.Abs(t1.row - t2.row) == 1)
        {
            return true;
        }

        // Check diagonal adjacency
        if (Math.Abs(t1.row - t2.row) == 1 && Math.Abs(t1.col - t2.col) == 1)
        {
            return true;
        }

        // If none of the above conditions are met, they are not adjacent
        return false;
    }


    public void UpdateFormationTimerLength()
    {
        if (gridTokens.Count <= 6)
        {
            formationTime = 10f;
        }
        else if (gridTokens.Count <= 12)
        {
            formationTime = 15f;
        }
        else if (gridTokens.Count <= 18)
        {
            formationTime = 20f;
        }
        else if (gridTokens.Count <= 24)
        {
            formationTime = 25f;
        }
        else if (gridTokens.Count <= 42)
        {
            formationTime = 30f;
        }

    }

    public bool isGameover()
    {
        return player1Score >= maxScore || player2Score >= maxScore || gridTokens.Count == 6 * 7;
    }

    public void DropTokenInColumn(int colNumber)
    {
        if (selectedToken == null)
        {
            Debug.Log("No token selected!");
            return;
        }

        //check if column is full
        if (ColumnCount(colNumber) == 6)
        {
            return;
        }

        //update formation timer length based on number of items in grid
        UpdateFormationTimerLength();

       
        
        // Get the token's script and renderer from the selected token
        Token tokenScript = selectedToken.GetComponent<Token>();
        Renderer tokenRenderer = selectedToken.GetComponent<Renderer>();

        if (tokenScript.tokenType == "bomb")
        {
            if (turn ==1)
            {
                p1BankObjs.Remove(selectedToken);
            }
            else
            {
                p2BankObjs.Remove(selectedToken);
            }
            Destroy(selectedToken);
            selectedToken = null;
            tokenSelected = false;
                        
            Debug.Log("Dropping bomb in column: " + colNumber);
            GameObject bomb = Instantiate(bombTokenPrefab, spawnLocs[colNumber - 1].transform.position, bombTokenPrefab.transform.rotation);

            //start explosion countDown timer
            Debug.Log("starting bomb timer");

            StartCoroutine(ExplodeBomb(bomb, colNumber));
            gridTokens.RemoveAll(item => item == null);
            return;
        }

        if (tokenScript.tokenType == "freeze")
        {
            if (turn == 1)
            {
                p1BankObjs.Remove(selectedToken);
            }
            else
            {
                p2BankObjs.Remove(selectedToken);
            }

            Destroy(selectedToken);
            selectedToken = null;
            tokenSelected = false;

            Debug.Log("Dropping freeze in column: " + colNumber);
            GameObject freezeTok = Instantiate(freezeTokenPrefab, spawnLocs[colNumber - 1].transform.position, freezeTokenPrefab.transform.rotation);

            //start explosion countDown timer
            Debug.Log("starting freeze timer");
            StartCoroutine(FreezeEffect(freezeTok, colNumber));
            return;
        }

        // Move the selected token to the chosen column in the grid
        Debug.Log("Dropping token " + tokenScript.letter + " in column: " + colNumber);
        GameObject newToken = Instantiate(tokenPrefab, spawnLocs[colNumber - 1].transform.position, tokenPrefab.transform.rotation);
        Token newTokenScript = newToken.GetComponent<Token>();
        newTokenScript.Initialize(tokenScript.letter, tokenScript.rarity, tokenScript.points);  // Transfer letter and rarity

        // Apply the same material to the new token in the grid
        Renderer newTokenRenderer = newToken.GetComponent<Renderer>();
        newTokenRenderer.material = tokenRenderer.material;

        //add new token to gridTokens List
        gridTokens.Add(newToken);

        // Determine the player bank array based on the current turn
        List<GameObject> currentPlayerBankObjs = (turn == 1) ? p1BankObjs : p2BankObjs;
        GameObject[] currentPlayerBankLocations = (turn == 1) ? p1BankLocations : p2BankLocations;

        // Find the index of the selected token in the player's bank
        int selectedTokenIndex = currentPlayerBankObjs.IndexOf(selectedToken);

        // Check if the selected token is actually in the player's bank
        if (selectedTokenIndex == -1)
        {
            Debug.LogError("Selected token not found in the player's bank!");
            return;
        }

        // Get the preview token (index 7) and move it to the selected token's bank location
        GameObject previewToken = currentPlayerBankObjs[7];
        previewToken.transform.position = currentPlayerBankLocations[selectedTokenIndex].transform.position;

        // Update the bank array to reflect the moved token
        currentPlayerBankObjs[selectedTokenIndex] = previewToken;

        // Remove the selected token from the player's bank
        currentPlayerBankObjs.RemoveAt(7);  // Remove the old preview token from the end of the list
        Destroy(selectedToken);  // Destroy the original token

        // Generate a new token for the preview slot and place it at the preview location
        (string letter, string rarity, int points) tokenData = generateToken();
        GameObject newPreviewToken = Instantiate(tokenPrefab, currentPlayerBankLocations[7].transform.position, tokenPrefab.transform.rotation);
        string newPreviewRarity = tokenData.rarity;
        Token newPreviewTokenScript = newPreviewToken.GetComponent<Token>();
        newPreviewTokenScript.Initialize(tokenData.letter, tokenData.rarity, tokenData.points);

        // Assign the material to the new preview token
        Renderer newPreviewTokenRenderer = newPreviewToken.GetComponent<Renderer>();
        newPreviewTokenRenderer.material = GetMaterialForRarity(newPreviewRarity);

        // Add the new preview token to the player's bank
        currentPlayerBankObjs.Insert(7,newPreviewToken);

        // Reset the selected token and game state
        selectedToken = null;
        tokenSelected = false;

        // Proceed to the next player's turn
        TakeTurn();
        
    }

    IEnumerator ExplodeBomb(GameObject bomb, int col)
    {
        Debug.Log("Waiting for 3 seconds...");

        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        bomb.GetComponent<Token>().Explode();
        //downgrade adjacent bombs
        int explostionCol = col;
        int explosionRow = 6 - ColumnCount(col);

        DowngradeAdjacent(explosionRow, explostionCol);
        Debug.Log("3 seconds have passed!");
    }

    IEnumerator FreezeEffect(GameObject freezeTok, int col)
    {
        Debug.Log("Waiting for 3 seconds...");

        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        freezeTok.GetComponent<Token>().FreezeExplode();
        //freeze adjacent toks
        int freezeCol = col;
        int freezeRow = 6 - ColumnCount(col);

        FreezeAdjacent(freezeRow, freezeCol);
        Debug.Log("3 seconds have passed!");
    }

    public void DowngradeAdjacent(int row, int col)
    {
        foreach (GameObject token in gridTokens)
        {
            if (Adjacent(token, row, col))
            {
                token.GetComponent<Token>().Downgrade();
            }
        }

        foreach (GameObject tok in pendingRemoveFromGridTokens)
        {
            gridTokens.Remove(tok);
        }

        pendingRemoveFromGridTokens.Clear();
    }

    public void FreezeAdjacent(int row, int col)
    {
        foreach (GameObject token in gridTokens)
        {
            if (Adjacent(token, row, col))
            {
                token.GetComponent<Token>().Freeze();
            }
        }
    }

    public int ColumnCount(int col)
    {
        int count = 0;
        foreach (GameObject tok in gridTokens)
        {
            if (tok.GetComponent<Token>().col == col)
            {
                count += 1;
            }

        }
        return count;
    }



    public void HandleSelection(GameObject token)
    {
        if (turn == 1 && !p1BankObjs.Contains(token) && gamePhase=="selection" || turn == 2 && !p2BankObjs.Contains(token) && gamePhase =="selection") {
            return;
        }

        if (turn == 1 && p1BankObjs[7].Equals(token) || turn == 2 && p2BankObjs[7].Equals(token))
        {
            return;
        }

        if (gamePhase == "formation")
        {
            if (token.GetComponent<Token>().tokenType != "letter")
            {
                return;
            }

            if (p1BankObjs.Contains(token) || p2BankObjs.Contains(token))
            {
                return;
            }

            if (token.GetComponent<Token>().IsFrozen()) {
                return;
            }

            Token t = token.GetComponent<Token>();

            if (sequence.Count == 0)
            {
                // First token selected
                sequence.Add(token);
                t.Select();
                UpdateText(t.letter + " (" + CalculateTotalPoints() + ")");
            }
            else
            {
                GameObject last = sequence[sequence.Count - 1];
                Token tLast = last.GetComponent<Token>();

                // If token is adjacent and not already selected
                if (Adjacent(last, token) && !t.isSelected)
                {
                    sequence.Add(token);
                    t.Select();
                    UpdateText(UpdateSequenceString() + " (" + CalculateTotalPoints() + ")");
                }
                // If token is not adjacent and not selected, reset the selection
                else if (!Adjacent(last, token) && !t.isSelected)
                {
                    UnselectAll();
                    sequence.Clear();
                    sequence.Add(token);
                    t.Select();
                    UpdateText(t.letter + " (" + CalculateTotalPoints() + ")");
                }
                // If token is already selected, unselect all tokens after it
                else if (sequence.Contains(token) && sequence.IndexOf(token) < sequence.Count - 1)
                {
                    int idx = sequence.IndexOf(token);  // Find the index of the token

                    // Unselect and remove everything after that token
                    for (int i = sequence.Count - 1; i > idx; i--)
                    {
                        Token tToUnselect = sequence[i].GetComponent<Token>();
                        tToUnselect.Unselect();
                        sequence.RemoveAt(i);  // Remove from the sequence
                    }

                    UpdateText(UpdateSequenceString() + " (" + CalculateTotalPoints() + ")");
                }
                else
                {
                    SubmitSeq();
                }
            }
        }
        else if (gamePhase == "selection")
        {
            Token tokenScript = token.GetComponent<Token>();

            // If a token is already selected, unselect it before selecting a new one
            if (tokenSelected)
            {
                selectedToken.GetComponent<Token>().Unselect();
            }

            // Select the new token and store it as the selected token
            selectedToken = token;
            tokenScript.Select();
            tokenSelected = true;
            Debug.Log("Token selected: " + tokenScript.letter);
        }
        else
        {
            return;
        }
    }

    private int CalculateTotalPoints()
    {
        int totalPoints = 0;

        foreach (GameObject token in sequence)
        {
            Token t = token.GetComponent<Token>();
            totalPoints += t.points;
        }

        return totalPoints;
    }

    private int CalculateTotalPoints(List<GameObject> seq)
    {
        int totalPoints = 0;

        foreach (GameObject token in seq)
        {
            Token t = token.GetComponent<Token>();
            totalPoints += t.points;
        }

        return totalPoints;
    }

    private string UpdateSequenceString()
    {
        sequenceString = "";  // Clear the existing string
        foreach (GameObject token in sequence)
        {
            Token t = token.GetComponent<Token>();
            sequenceString += t.letter;  // Append the letters without spaces
        }
        return sequenceString;  // Return the final word
    }




    // Word Checking Logic 
    public void SubmitSeq()
    {
        Debug.Log("SUBMITTED SEQUENCE");
        Debug.Log("Sequence length: " + sequence.Count);

        // Generate the final sequence string without spaces
        sequenceString = UpdateSequenceString().Trim().ToUpper();

        // Check sequence length before clearing
        if (sequence.Count < minWordLength)
        {
            UpdateText("Too short");
        }
        else if (AlreadyFound(sequenceString))
        {
            UpdateText("Already found");
        }
        else if (!InDictionary(sequenceString))
        {
            UpdateText("Invalid word");
        }
        else
        {
            // Compute points by summing points of each token in the sequence
            int points = 0;
            foreach (GameObject token in sequence)
            {
                Token t = token.GetComponent<Token>();
                points += t.points;
            }

            // Add points to the current player's score
            if (turn == 1)
            {
                player1Score += points;
                UpdateScoreDisplay(1);
            }
            else
            {
                player2Score += points;
                UpdateScoreDisplay(2);
            }

            // Display the points earned for this word
            UpdateText("+" + points + " points");
            AddToFound(sequenceString);
        }

        UpdateFoundWordsText();
        UnselectAll(); // Unselect all before clearing
        sequence.Clear(); // Now clear the sequence
        sequenceString = "";
    }




    public void UpdateScoreDisplay(int player)
    {
        if (player == 1)
        {
            player1ScoreText.text = "Score: " + player1Score;
        }
        else if (player == 2)
        {
            player2ScoreText.text = "Score: " + player2Score;
        }
    }


    public void UnselectAll()
    {
        for (int i = sequence.Count - 1; i >= 0; i--)
        {
            Token t = sequence[i].GetComponent<Token>();
            t.Unselect();
            sequence.RemoveAt(i);
        }
    }

    public void DecreaseFreezeLevelAll()
    {
        foreach (GameObject tok in gridTokens)
        {
            if (tok != null)
            {
                tok.GetComponent<Token>().UnFreeze();

            }
        }
    }

    public void UpdateText(string newText)
    {
        displayText.text = newText;
    }

    // A weighted list of letters based on their frequency in the English language
    char[] letters = {
    'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E',    // 12.7%
    'T', 'T', 'T', 'T', 'T', 'T', 'T', 'T', 'T',                         // 9.1%
    'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A',                              // 8.2%
    'O', 'O', 'O', 'O', 'O', 'O', 'O',                                   // 7.5%
    'I', 'I', 'I', 'I', 'I', 'I', 'I',                                   // 7.0%
    'N', 'N', 'N', 'N', 'N', 'N',                                        // 6.7%
    'S', 'S', 'S', 'S', 'S', 'S',                                        // 6.3%
    'H', 'H', 'H', 'H', 'H', 'H',                                        // 6.1%
    'R', 'R', 'R', 'R', 'R', 'R',                                        // 6.0%
    'D', 'D', 'D', 'D',                                                  // 4.3%
    'L', 'L', 'L', 'L',                                                  // 4.0%
    'C', 'C', 'C',                                                       // 2.8%
    'U', 'U', 'U',                                                       // 2.8%
    'M', 'M', 'M',                                                       // 2.4%
    'W', 'W', 'W',                                                       // 2.4%
    'F', 'F',                                                            // 2.2%
    'G', 'G',                                                            // 2.0%
    'Y', 'Y',                                                            // 2.0%
    'P', 'P',                                                            // 1.9%
    'B', 'B',                                                            // 1.5%
    'V',                                                                 // 1.0%
    'K',                                                                 // 0.8%
    'J', 'X', 'Q', 'Z'                                                   // 0.2%-0.1%
};

    private (string letter, string rarity, int points) generateToken()
    {
        int randomIndex = UnityEngine.Random.Range(0, letters.Length);
        string letter =  letters[randomIndex] + "";

        // Generate a random number between 0 and 9 (for 10 possible outcomes)
        int randomValue = UnityEngine.Random.Range(0, 10);
        int points;
        string rarity;
        // Use the random number to return the rarity based on the 7:2:1 ratio
        if (randomValue < 7)  // 0 to 6 (7 values) for "Bronze"
        {
            rarity = "Bronze";
            points = 1;
        }
        else if (randomValue < 9)  // 7 to 8 (2 values) for "Silver"
        {
            rarity =  "Silver";
            points = 2;
        }
        else  // 9 (1 value) for "Gold"
        {
            rarity = "Gold";
            points = 3;
        }

        return (letter, rarity, points);
    }

    public void LoadDictionary()
    {
        // Load all words and ensure they are trimmed and converted to uppercase
        string[] allWords = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, "ScrabbleWords.txt"));

        // Convert all words to uppercase and trim spaces/newline characters
        dictionary = new List<string>(allWords.Select(word => word.Trim().ToUpper()));

        Debug.Log("Words list read in = " + dictionary.Count);
    }

    public bool AlreadyFound(string query)
    {
        return foundWords.Contains(query.ToUpper());
    }

    public bool InDictionary(string query)
    {
        return dictionary.Contains(query.ToUpper());
    }

    public string ToString(List<GameObject> tokens)
    {
        string sequence = "";

        foreach (GameObject tok in tokens)
        {
            sequence += tok.GetComponent<Token>().letter;
        }

        return sequence;
    }

    public void AddToFound(string word)
    {
        Debug.Log("Adding word to found words list: " + word);
        foundWords.Add(word.ToUpper());
    }


    public void UpdateFoundWordsText()
    {
        if (foundWords.Count == 0)
        {
            foundWordsText.text = "\nNo words found";
            return;
        }


        // Sort the found words alphabetically
        List<string> sortedWords = foundWords.OrderBy(word => word).ToList();

        // Determine the number of columns and rows needed
        int totalWords = sortedWords.Count;
        int numColumns = 6;
        int numRows = Mathf.CeilToInt(totalWords / (float)numColumns); // Round up to handle uneven word counts


        // Create the display string
        string displayText = "";

        // Loop through and add words row by row, fitting into 4 columns
        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numColumns; col++)
            {
                int wordIndex = row + col * numRows;
                if (wordIndex < totalWords)
                {
                    displayText += sortedWords[wordIndex].PadRight(15); // Adjust padding for even column spacing
                }
            }
            displayText += "\n"; // Newline after each row
        }

        // Update the Text component with the formatted string
        foundWordsText.text = displayText;
    }

    public void UpdatePlayerTurnHighlight()
    {
        if (turn == 2)
        {
            // Highlight Player 1's turn
            player1TurnText.color = Color.red; // Highlight Player 1

            // Reset Player 2's text style
            player2TurnText.color = Color.white; // Default color for Player 2
            player2TurnText.fontStyle = FontStyles.Normal;
        }
        else
        {
            // Highlight Player 2's turn
            player2TurnText.color = Color.blue; // Highlight Player 2

            // Reset Player 1's text style
            player1TurnText.color = Color.white; // Default color for Player 1
            player1TurnText.fontStyle = FontStyles.Normal;
        }
    }


    void Start()
    {
        Debug.Log("Game starting...");
        InitializePlayerBanks();
        LoadDictionary();
        gamePhase = "selection";

        // Initialize score display
        player1ScoreText.text = "Score: 0";
        player2ScoreText.text = "Score: 0";

        // Ensure the timer is hidden when the game starts
        timerText.gameObject.SetActive(false);

        // Highlight the current player's turn at the start
        UpdatePlayerTurnHighlight();

    }

    public void Exit()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
