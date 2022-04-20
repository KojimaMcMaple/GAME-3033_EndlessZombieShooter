using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Item
{
    public enum ItemType
    {
        NONE,
        POTION,
        AMMO,
        MISSILE
    }

    public ItemType item_type;
    public int amount;

    public Sprite GetSprite()
    {
        switch (item_type)
        {
            case ItemType.NONE:
                break;
            case ItemType.POTION:
                return ItemAssets.Instance.potion_sprite;
                break;
            case ItemType.AMMO:
                return ItemAssets.Instance.ammo_sprite;
                break;
            case ItemType.MISSILE:
                return ItemAssets.Instance.missile_sprite;
                break;
            default:
                break;
        }

        return null;
    }

    public Transform GetPrefab()
    {
        switch (item_type)
        {
            case ItemType.NONE:
                break;
            case ItemType.POTION:
                return ItemAssets.Instance.potion_prefab;
                break;
            case ItemType.AMMO:
                return ItemAssets.Instance.ammo_prefab;
                break;
            case ItemType.MISSILE:
                return ItemAssets.Instance.missile_prefab;
                break;
            default:
                break;
        }

        return null;
    }
}
