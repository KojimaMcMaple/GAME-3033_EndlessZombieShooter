using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquippableItemSO : PickupItemSO
{
    [Header("Equippable Settings")]
    private bool is_equipped = false;

    public bool IsEquipped
    {
        get => is_equipped;
        set
        {
            IsEquipped = value;
            OnEquipStatusChanged?.Invoke();
        }
    }

    public delegate void EquipStatusChanged();
    public event EquipStatusChanged OnEquipStatusChanged;

    public override void UseItem(ThirdPersonController player_controller)
    {
        IsEquipped = !IsEquipped;
    }
}
