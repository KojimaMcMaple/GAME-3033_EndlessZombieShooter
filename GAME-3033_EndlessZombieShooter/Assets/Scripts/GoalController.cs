using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    private GameManager game_manager_;

    void Awake()
    {
        game_manager_ = FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable<int> other_interface = other.gameObject.GetComponent<IDamageable<int>>();
        if (other_interface != null)
        {
            if (other_interface.obj_type == GlobalEnums.ObjType.PLAYER)
            {
                game_manager_.DoGameOver();
            }
        }
    }
}
