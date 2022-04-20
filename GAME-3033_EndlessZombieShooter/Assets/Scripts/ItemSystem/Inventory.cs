using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory
{
    public event EventHandler OnItemListChanged;
    
    private List<Item> item_list_;

    public Inventory()
    {
        item_list_ = new List<Item>();

        AddItem(new Item { item_type = Item.ItemType.POTION, amount = 1 });
        AddItem(new Item { item_type = Item.ItemType.AMMO, amount = 1 });
    }

    public void AddItem(Item item)
    {
        item_list_.Add(item);
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public List<Item> GetItemList()
    {
        return item_list_;
    }
}
