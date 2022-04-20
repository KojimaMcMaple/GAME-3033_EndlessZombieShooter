using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWorld : MonoBehaviour
{
    [SerializeField] private Item item_;

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
