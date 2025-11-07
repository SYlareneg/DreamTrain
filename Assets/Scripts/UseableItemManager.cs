using UnityEngine;

public class UseableItemManager : MonoBehaviour
{
    public static UseableItemManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] GameObject useableItemPrefab;
    public RectTransform itemArea;
    public GameObject itemListScroll;
    public Canvas itemCanvas;
    [Tooltip("플레이어 아이템")] public UseableItemSO playerItemSO;

    public void InitUseableItemList()
    {
        foreach (var uItem in playerItemSO.useableItems)
        {
            var uItemObj = Instantiate(useableItemPrefab, itemListScroll.transform, false);
            UseableItem useableItem = uItemObj.GetComponent<UseableItem>();
            useableItem.Setup(uItem);

        }
    }
    
    private void Start()
    {
        InitUseableItemList();
    }
}
