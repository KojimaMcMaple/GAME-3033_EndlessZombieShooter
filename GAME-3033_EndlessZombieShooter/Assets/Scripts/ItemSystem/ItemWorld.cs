using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWorld : MonoBehaviour
{
    [SerializeField] private Item.ItemType item_type_;
    [SerializeField] private int amount_ = 1;
    private Item item_;

    private void Awake()
    {
        item_ = new Item { item_type = item_type_, amount = amount_ };
    }

    public static ItemWorld SpawnItemWorld(Vector3 position, Item item)
    {
        Transform temp = Instantiate(item.GetPrefab(), position, Quaternion.identity);
        ItemWorld iw = temp.GetComponent<ItemWorld>();
        iw.SetItem(item);

        return iw;
    }

    public Item GetItem()
    {
        return item_;
    }

    public void SetItem(Item item)
    {
        item_ = item;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
