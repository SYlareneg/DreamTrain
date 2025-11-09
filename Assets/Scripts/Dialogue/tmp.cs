using UnityEngine;

public class tmp : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        OnDisable();
    }
    void OnDisable()
    {
        Debug.LogWarning("[TRACE] moooPanel got disabled!", this);
        Debug.Log(System.Environment.StackTrace);
    }
}
