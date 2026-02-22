using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FakeScrollbar : MonoBehaviour, IDragHandler
{
    [SerializeField] Scrollbar scrollbar;
    RectTransform rt;
    Vector3 scrollbarPos;

    public void OnDrag(PointerEventData eventData)
    {
        rt.position += new Vector3(0, eventData.delta.y, 0);
        rt.position = new Vector3(rt.position.x, Mathf.Clamp(rt.position.y, scrollbarPos.y + scrollbar.gameObject.GetComponent<RectTransform>().rect.yMin, scrollbarPos.y + scrollbar.gameObject.GetComponent<RectTransform>().rect.yMax), rt.position.z);
        scrollbar.value = Mathf.Clamp((rt.position.y - (scrollbarPos.y + scrollbar.gameObject.GetComponent<RectTransform>().rect.yMin)) / scrollbar.gameObject.GetComponent<RectTransform>().rect.height, 0f, 1f);
    }

    void Start()
    {
        rt = GetComponent<RectTransform>();
        scrollbarPos = scrollbar.gameObject.GetComponent<RectTransform>().position;
        rt.position = new Vector3(rt.position.x, scrollbarPos.y + Mathf.Clamp(scrollbar.gameObject.GetComponent<RectTransform>().rect.yMin + scrollbar.value * scrollbar.gameObject.GetComponent<RectTransform>().rect.height, scrollbar.gameObject.GetComponent<RectTransform>().rect.yMin, scrollbar.gameObject.GetComponent<RectTransform>().rect.yMax), rt.position.z);
    }
}
