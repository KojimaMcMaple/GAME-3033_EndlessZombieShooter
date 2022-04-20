using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAssets : MonoBehaviour
{
    public static ItemAssets Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public Transform potion_prefab;
    public Transform ammo_prefab;
    public Transform missile_prefab;

    public Sprite potion_sprite;
    public Sprite ammo_sprite;
    public Sprite missile_sprite;
}
