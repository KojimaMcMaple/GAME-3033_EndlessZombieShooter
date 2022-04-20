using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  The Source file name: EnemyAtkHitboxController.cs
///  Author's name: Trung Le (Kyle Hunter)
///  Student Number: 101264698
///  Program description: Defines behavior for the enemy's atk hitbox
///  Date last Modified: See GitHub
///  Revision History: See GitHub
/// </summary>
public class EnemyAtkHitboxController : MonoBehaviour
{
    [SerializeField] private EnemyController parent_controller_;

    //void Awake()
    //{
    //    parent_controller_ = transform.root.transform.GetComponent<EnemyController>();
    //}

    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("> EnemyAtkHitboxController OnTriggerStay!");
        if (parent_controller_.IsAtkHitboxActive())
        {
            IDamageable<int> other_interface = other.gameObject.GetComponent<IDamageable<int>>();
            if (other_interface != null)
            {
                if (other_interface.obj_type == GlobalEnums.ObjType.PLAYER)
                {
                    if (other_interface.obj_type != parent_controller_.obj_type)
                    {
                        //if (atk_countdown_ <= 0)
                        //{
                        //    other_interface.ApplyDamage(atk_damage_);
                        //    atk_countdown_ = firerate_; //prevents applying damage every frame
                        //}
                        parent_controller_.DoDealDamageToIDamageable(other_interface);
                    }
                }
            }
        }
    }
}
