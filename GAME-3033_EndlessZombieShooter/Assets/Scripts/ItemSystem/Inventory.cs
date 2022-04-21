using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory
{
    public event EventHandler OnItemListChanged;
    private Action<Item> UseItemAction;

    private List<Item> item_list_;
    private int max_item_count_ = 7;

    public Inventory(Action<Item> useItemAction)
    {
        UseItemAction = useItemAction;
        item_list_ = new List<Item>();

        AddItem(new Item { item_type = Item.ItemType.POTION, amount = 1 });
        AddItem(new Item { item_type = Item.ItemType.AMMO, amount = 1 });
        UseItemAction = useItemAction;
    }

    public void AddItem(Item item)
    {
        if (item_list_.Count < max_item_count_)
        {
            item_list_.Add(item);
            OnItemListChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RemoveItem(Item item)
    {
        item_list_.Remove(item);
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UseItem(Item item)
    {
        UseItemAction(item);
    }
    
    public List<Item> GetItemList()
    {
        return item_list_;
    }
}
