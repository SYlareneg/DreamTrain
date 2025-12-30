using UnityEngine;

[ExecuteAlways]
public class RouletteBuffAllign : MonoBehaviour
{
    public float radius = 2f;
    public float allignAngle = 0f; // degrees
    public float spacingAngle = 10f; // degrees
    public float maxAngle = 120f; // degrees
    public Vector2 elementSize = Vector2.one;

    public void AlignChildrenInCircle()
    {
        int count = transform.childCount;
        if (count == 0) return;

        float spacing = spacingAngle;
        if(count > 1 && (count - 1) * spacingAngle > maxAngle)
        {
            spacing = maxAngle / (count - 1);
        }

        for (int i = 0; i < count; i++)
        {
            float angle = allignAngle - (count - 1) * spacing / 2f + spacing * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 pos = transform.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;

            Transform child = transform.GetChild(i);
            child.position = pos;
            child.GetComponent<RectTransform>().sizeDelta = elementSize;
        }
    }

    void Update()
    {
        AlignChildrenInCircle();
    }
}
