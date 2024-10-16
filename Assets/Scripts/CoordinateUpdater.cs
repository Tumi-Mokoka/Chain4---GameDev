using UnityEngine;

public class CoordinateUpdater : MonoBehaviour
{
    public int row;
    public int col;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider other)
    {

        Token t = other.GetComponent<Token>();
        t.row = row;
        t.col = col;
    }
}
