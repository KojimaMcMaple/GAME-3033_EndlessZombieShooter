using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAmmoController : MonoBehaviour
{
    private ItemWorld parent_controller_;

    void Awake()
    {
        parent_controller_ = transform.parent.transform.GetComponent<ItemWorld>();
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable<int> other_interface = other.gameObject.GetComponent<IDamageable<int>>();
        if (other_interface != null)
        {
            if (other_interface.obj_type == GlobalEnums.ObjType.PLAYER)
            {
                other.gameObject.GetComponent<Player.ThirdPersonController>().DoAddItem(parent_controller_.GetItem());
                //other.gameObject.GetComponent<Player.ThirdPersonController>().DoResetAmmoReserve();
                parent_controller_.DestroySelf();
            }
        }
    }
}
