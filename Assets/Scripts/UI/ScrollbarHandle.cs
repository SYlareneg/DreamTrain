using UnityEngine;
using UnityEngine.UI;

public class ScrollbarHandle : MonoBehaviour
{
    [SerializeField] Scrollbar scrollbar;

    void Update()
    {
        Vector2 pivot = transform.GetComponent<RectTransform>().pivot;
        pivot.y = scrollbar.value;
        transform.GetComponent<RectTransform>().pivot = pivot;
    }
}
