using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  The Source file name: EnemyController.cs
///  Author's name: Trung Le (Kyle Hunter)
///  Student Number: 101264698
///  Program description: Defines behavior for the enemy's field of vision
///  Date last Modified: See GitHub
///  Revision History: See GitHub
/// </summary>
public class EnemyFieldOfVisionController : MonoBehaviour
{
    private EnemyController parent_controller_;

    void Awake()
    {
        parent_controller_ = transform.parent.transform.GetComponent<EnemyController>();
    }

    /// <summary>
    /// Enters ATTACK state if player triggers collider
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        IDamageable<int> other_interface = other.gameObject.GetComponent<IDamageable<int>>();
        if (other_interface != null)
        {
            if (other_interface.obj_type == GlobalEnums.ObjType.PLAYER)
            {
                parent_controller_.SetTarget(other.gameObject);
                parent_controller_.DoAggro();
                gameObject.SetActive(false);
            }
        }
    }

    ///// <summary>
    ///// Enters IDLE state if player leaves collider
    ///// </summary>
    ///// <param name="other"></param>
    //private void OnTriggerExit2D(Collider2D other)
    //{
    //    IDamageable<int> other_interface = other.gameObject.GetComponent<IDamageable<int>>();
    //    if (other_interface != null)
    //    {
    //        if (other_interface.obj_type == GlobalEnums.ObjType.PLAYER)
    //        {
    //            parent_controller_.SetTarget(null);
    //            parent_controller_.SetState(GlobalEnums.EnemyState.IDLE);
    //        }
    //    }
    //}
}
