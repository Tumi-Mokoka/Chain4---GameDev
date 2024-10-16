using UnityEngine;

public class InputField : MonoBehaviour
{
    public int col;
    public GameManager gameManager;

    private void OnMouseDown()
    {
        // Ensure there is a selected token
        if (gameManager != null && gameManager.tokenSelected)
        {
            // Drop the selected token into the column tied to this input field
            gameManager.DropTokenInColumn(col);
        }
        else
        {
            Debug.Log("No token selected to drop!");
        }
    }

   

}
