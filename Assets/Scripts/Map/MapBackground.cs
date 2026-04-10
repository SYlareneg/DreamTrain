using UnityEngine;

public class MapBackground : MonoBehaviour
{
    [SerializeField] Sprite[] backgroundSprites; // 배경 이미지 배열
    Vector2 originalMousePos;
    Vector2 originalCamPos;

    public void SetBackground(int actNum)
    {
        if (actNum < backgroundSprites.Length)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = backgroundSprites[actNum];
            }
        }
    }
    void OnMouseDown()
    {
        originalMousePos = Input.mousePosition;
        if(MapManager.Inst.mapCamera) originalCamPos = MapManager.Inst.mapCamera.transform.position;
    }
    void OnMouseDrag()
    {
        Vector2 newCamPos = originalCamPos;
        newCamPos.y += (originalMousePos.y - Input.mousePosition.y) * 0.01f;
        if(MapManager.Inst.mapCamera) MapManager.Inst.mapCamera.MoveCamera(newCamPos.y);
    }

    void Start()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if(collider != null && spriteRenderer != null)
        {
            collider.size = spriteRenderer.sprite.bounds.size;
            collider.offset = spriteRenderer.sprite.bounds.center;
        }

        SetBackground(DataManager.Inst.actSO.curActNum);
    }
}
