using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemCategory
{
    NONE,
    WEAPON,
    CONSUMABLE,
    EQUIPMENT,
    AMMO
}
public abstract class PickupItemSO : ScriptableObject
{
    public string item_name = "ItemName";
    public ItemCategory category = ItemCategory.NONE;
    public GameObject prefab;
    public bool is_stackable = true;
    public int max_size = 1;
    public int item_amount = 1;

    public delegate void AmountChanged();
    public event AmountChanged OnAmountChanged;

    public delegate void ItemDestroyed();
    public event ItemDestroyed OnItemDestroyed;

    public delegate void ItemDropped();
    public event ItemDropped OnItemDropped;

    public Player.ThirdPersonController controller { get; private set; }
    public virtual void Initialize(Player.ThirdPersonController player_controller)
    {
        controller = player_controller;
    }

    public abstract void UseItem(Player.ThirdPersonController player_controller);

    public virtual void DeleteItem(Player.ThirdPersonController player_controller)
    {
        OnItemDestroyed?.Invoke();
    }

    public virtual void DropItem(Player.ThirdPersonController player_controller)
    {
        OnItemDropped?.Invoke();
    }

    public void ChangeAmount(int value)
    {
        item_amount+= value;
        OnAmountChanged?.Invoke();
    }

    public void SetAmount(int value)
    {
        item_amount = value;
        OnAmountChanged?.Invoke();
    }
}
