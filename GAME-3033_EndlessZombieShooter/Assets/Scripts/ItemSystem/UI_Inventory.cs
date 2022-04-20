using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory : MonoBehaviour
{
    private Inventory inventory_;
    private Transform item_slot_container_;
    private Transform item_slot_template_;

    private void Awake()
    {
        item_slot_container_ = transform.Find("ItemSlotContainer");
        item_slot_template_ = item_slot_container_.Find("ItemSlotTemplate");
    }

    public void SetInventory(Inventory value)
    {
        inventory_ = value;
        inventory_.OnItemListChanged += HandleOnItemListChangedEvent;
        RefreshInventoryItems();
    }

    private void HandleOnItemListChangedEvent(object sender, System.EventArgs e)
    {
        RefreshInventoryItems();
    }

    private void RefreshInventoryItems()
    {
        foreach (Transform child in item_slot_container_)
        {
            if (child != item_slot_template_)
            {
                Destroy(child.gameObject);
            }
        }
        int x = 0;
        int y = 0;
        float item_slot_cell_size = 100f;
        foreach (Item item in inventory_.GetItemList())
        {
            RectTransform item_slot_transform = Instantiate(item_slot_template_, item_slot_container_).GetComponent<RectTransform>();
            item_slot_transform.gameObject.SetActive(true);
            item_slot_transform.anchoredPosition= new Vector2(x* item_slot_cell_size, y* item_slot_cell_size);
            Image item_icon = item_slot_transform.Find("Icon").GetComponent<Image>();
            item_icon.sprite = item.GetSprite();
            x++;
            if (x>8)
            {
                x = 0;
                y++;
            }
        }
    }
}
