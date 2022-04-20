using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ConsumableItemSO : PickupItemSO
{
    [Header("Consumable Settings")]
    public int consume_effect = 0;
    public override void UseItem(ThirdPersonController player_controller)
    {
        SetAmount(item_amount - 1);
        if (item_amount < 0)
        {
            DeleteItem(player_controller);
        }
    }
}
