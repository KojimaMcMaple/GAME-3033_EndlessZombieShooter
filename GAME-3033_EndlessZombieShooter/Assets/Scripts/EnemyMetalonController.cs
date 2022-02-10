using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMetalonController : EnemyController
{
    

    void Awake()
    {
        DoBaseInit();
    }

    void Update()
    {
        switch (state_) //state machine
        {
            case GlobalEnums.EnemyState.IDLE:
                //DoPatrol();
                break;
            case GlobalEnums.EnemyState.MOVE_TO_TARGET:
                MoveToTarget();
                break;
            case GlobalEnums.EnemyState.ATTACK:
                DoAttack();
                break;
            default:
                break;
        }

        if (atk_countdown_ > 0)
        {
            atk_countdown_ -= Time.deltaTime;
        }
    }

    protected override void DoAttack()
    {
        if (Vector2.Distance(transform.position, target_.transform.position) > 1.25f)
        {
            SetState(GlobalEnums.EnemyState.MOVE_TO_TARGET);
            SetAtkHitboxActive(false);
        }
        else
        {
            animator_.SetTrigger("Smash Attack");
        }
    }

    /// <summary>
    /// Aggro if player detected
    /// </summary>
    public override void DoAggro()
    {
        Debug.Log("> Metalon DoAggro");
        Debug.Log("> target_ is " + target_.name);
        if (target_ == null)
        {
            SetTarget(FindObjectOfType<Player.ThirdPersonController>().gameObject);
        }
        SetState(GlobalEnums.EnemyState.MOVE_TO_TARGET);
    }

    private void Move()
    {
        
    }

    private void DoPatrol()
    {
        Move();
    }

    private void MoveToTarget()
    {
        if (target_ == null)
        {
            SetState(GlobalEnums.EnemyState.IDLE);
            return;
        }

        if (Vector3.Distance(transform.position, target_.transform.position) < 1.25f)
        {
            SetState(GlobalEnums.EnemyState.ATTACK);
        }
        else
        {
            Player.ThirdPersonController player_controller = target_.GetComponent<Player.ThirdPersonController>();
            if (player_controller != null)
            {
                nav_.destination = player_controller.GetRootPos();
            }
            else
            {
                nav_.destination = target_.transform.position;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(""))
        {
            
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(""))
        {
            
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (IsAtkHitboxActive())
        {
            IDamageable<int> other_interface = collision.gameObject.GetComponent<IDamageable<int>>();
            if (other_interface != null)
            {
                if (other_interface.obj_type != type_)
                {
                    if (atk_countdown_ <= 0)
                    {
                        other_interface.ApplyDamage(collision_damage_);
                        atk_countdown_ = firerate_; //prevents applying damage every frame
                    }
                }
            }
        }
    }

    /// <summary>
    /// Visual debug
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        
    }
}
