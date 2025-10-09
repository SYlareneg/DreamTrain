using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(GridLayoutGroup))]
public class ResponsiveGrid : MonoBehaviour
{
    public int constraintCount;
    public Vector2 spacing;
    private GridLayoutGroup grid;
    private RectTransform rect;
    public Canvas canvas;
    public CanvasScaler scaler;

    void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rect = GetComponent<RectTransform>();
        constraintCount = grid.constraintCount;
        spacing = grid.spacing;
        canvas = GetComponentInParent<Canvas>();
        scaler = GetComponentInParent<CanvasScaler>();
    }

    void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight) UpdateGrid();
    }

    private int lastWidth, lastHeight;

    void UpdateGrid()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;

        float totalWidth = 0, cellWidth = 0;
        if (grid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
        {
            totalWidth = rect.rect.height - (grid.padding.top + grid.padding.bottom);
            cellWidth = (totalWidth - spacing.x * (constraintCount - 1)) / constraintCount * GetCanvasScaleFactor();
            grid.cellSize = new Vector2(cellWidth / grid.cellSize.x * grid.cellSize.y, cellWidth);
            grid.spacing = spacing + grid.cellSize * (1 / GetCanvasScaleFactor() - 1);
        }
        else if (grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            totalWidth = rect.rect.width - (grid.padding.left + grid.padding.right);
            cellWidth = (totalWidth - spacing.y * (constraintCount - 1)) / constraintCount * GetCanvasScaleFactor();
            grid.cellSize = new Vector2(cellWidth, cellWidth / grid.cellSize.x * grid.cellSize.y);
            grid.spacing = spacing + grid.cellSize * (1 / GetCanvasScaleFactor() - 1);
        }
    }

    float GetCanvasScaleFactor()
    {
        Vector2 referenceResolution = scaler.referenceResolution;
        float match = scaler.matchWidthOrHeight;
        float logWidth = Mathf.Log(Screen.width / referenceResolution.x, 2);
        float logHeight = Mathf.Log(Screen.height / referenceResolution.y, 2);
        float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, match);
        return Mathf.Pow(2, logWeightedAverage);
    }
}