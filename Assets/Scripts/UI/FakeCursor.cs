using UnityEngine;

public class FakeCursor : MonoBehaviour
{
    public RectTransform cursorUI;
    public float scale = 1.5f;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        cursorUI.position = Input.mousePosition;
        cursorUI.localScale = Vector3.one * scale;
        if (Cursor.visible) Cursor.visible = false;
        if (Cursor.lockState != CursorLockMode.Confined)
            Cursor.lockState = CursorLockMode.Confined;
    }
}
