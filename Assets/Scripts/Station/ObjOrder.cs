using UnityEngine;

public class ObjOrder : MonoBehaviour
{
    [SerializeField] SpriteRenderer playerSR;
    SpriteRenderer sr;
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (playerSR.transform.position.y > sr.transform.position.y) sr.sortingOrder = playerSR.sortingOrder + 1;
        else sr.sortingOrder = playerSR.sortingOrder - 1;
    }
}
