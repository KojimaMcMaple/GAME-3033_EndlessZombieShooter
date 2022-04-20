using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class WeaponSO : EquippableItemSO
{
    [Header("Weapon Settings")]
    public int weapon_damage;

    public override void UseItem(ThirdPersonController player_controller)
    {
        if (IsEquipped)
        {

        }
        else
        {

        }
        base.UseItem(player_controller);
    }
}
