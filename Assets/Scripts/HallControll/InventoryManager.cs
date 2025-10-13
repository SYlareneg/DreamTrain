using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public Image[] itemSlots;
    public Sprite emptySlotSprite;
    private List<InteractableObjectData> collectedItems = new List<InteractableObjectData>();
    public static event System.Action OnInventoryFull;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (var slot in itemSlots)
        {
            slot.sprite = emptySlotSprite;
        }
    }

    public bool CollectItem(InteractableObjectData itemData)
    {
        if (collectedItems.Count >= itemSlots.Length)
        {
            Debug.Log("인벤토리가 꽉 찼습니다.");
            return false;
        }

        collectedItems.Add(itemData);
        UpdateInventoryUI();
        if (collectedItems.Count >= itemSlots.Length)
        {
            OnInventoryFull?.Invoke(); // 이벤트 발생!
            Debug.Log("Inventory is full! Event fired.");
        }

        return true; 
    }

    private void UpdateInventoryUI()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i < collectedItems.Count) itemSlots[i].sprite = collectedItems[i].itemIcon;
            else itemSlots[i].sprite = emptySlotSprite;
        }
    }
}