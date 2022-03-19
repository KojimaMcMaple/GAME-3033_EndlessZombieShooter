using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyRobotController : EnemyController
{
    [SerializeField] private ParticleSystem atk_indicator_vfx_;
    private int anim_id_speed_;
    private int anim_id_atk1_;

    void Awake()
    {
        DoBaseInit();
        AssignAnimationIDs();
        atk_indicator_vfx_.Stop();
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
            case GlobalEnums.EnemyState.STUNNED:
                DoEndAtkHitbox();
                break;
            case GlobalEnums.EnemyState.DIE:
                break;
            default:
                break;
        }

        if (hit_cooldown_delta_ > 0)
        {
            hit_cooldown_delta_ -= Time.deltaTime;
        }
        if (flinch_cooldown_delta_ > 0)
        {
            flinch_cooldown_delta_ -= Time.deltaTime;
        }

        animator_.SetFloat(anim_id_speed_, nav_.velocity.magnitude);
    }

    /// <summary>
    /// Convert string to int for anim IDs
    /// </summary>
    private void AssignAnimationIDs()
    {
        anim_id_speed_ = Animator.StringToHash("Speed");
        anim_id_atk1_ = Animator.StringToHash("Atk1");
    }

    protected override void DoAttack()
    {
        if (Vector3.Distance(transform.position, target_.transform.position) > atk_range_)
        {
            SetState(GlobalEnums.EnemyState.MOVE_TO_TARGET);
        }
        else if(!is_atk_)
        {
            animator_.SetBool(anim_id_atk1_, true);
            is_atk_ = true;
        }
    }

    /// <summary>
    /// Aggro if player detected
    /// </summary>
    public override void DoAggro()
    {
        if (state_ == GlobalEnums.EnemyState.DIE || is_stunned_)
        {
            return;
        }
        Debug.Log("> Metalon DoAggro");
        if (target_ == null)
        {
            SetTarget(FindObjectOfType<Player.ThirdPersonController>().gameObject); //should be set by EnemyFieldOfVisionController, but this is safer, and the function has more utility this way
        }
        SetState(GlobalEnums.EnemyState.MOVE_TO_TARGET);
    }

    protected override void DoFlinch(GlobalEnums.FlinchType flinch_mode = GlobalEnums.FlinchType.DEFAULT)
    {
        if (flinch_mode == GlobalEnums.FlinchType.NO_FLINCH)
        {
            return;
        }
        if (flinch_mode == GlobalEnums.FlinchType.ABSOLUTE || flinch_cooldown_delta_ <= 0)
        {
            flinch_cooldown_delta_ = flinch_cooldown_;
        }
    }

    protected override void DoDeath()
    {
        if (state_ != GlobalEnums.EnemyState.DIE)
        {
            if (nav_.isOnNavMesh) //in case enemy is thrown off navmesh
            {
                nav_.isStopped = true;
            }
        }
        SetState(GlobalEnums.EnemyState.DIE);
        StartCoroutine(Despawn());
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
        if (!nav_.isOnNavMesh) //in case enemy is thrown off navmesh
        {
            return;
        }

        if (Vector3.Distance(transform.position, target_.transform.position) < atk_range_)
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

        //// Check if we've reached the destination http://answers.unity.com/answers/746157/view.html
        //if (!nav_.pathPending)
        //{
        //    if (nav_.remainingDistance <= nav_.stoppingDistance)
        //    {
        //        if (!nav_.hasPath || nav_.velocity.sqrMagnitude == 0f)
        //        {
        //            animator_.SetBool(anim_id_run_, false);
        //        }
        //    }
        //}
    }

    public void DoStartAtkHitbox()
    {
        SetAtkHitboxActive();
        atk_indicator_vfx_.Play();
    }

    public void DoEndAtkHitbox()
    {
        SetAtkHitboxInactive();
        //atk_indicator_vfx_.Stop();
    }

    /*private void OnCollisionEnter(Collision collision)
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
        //if (IsAtkHitboxActive())
        //{
        //    IDamageable<int> other_interface = collision.gameObject.GetComponent<IDamageable<int>>();
        //    if (other_interface != null)
        //    {
        //        if (other_interface.obj_type != type_)
        //        {
        //            if (atk_countdown_ <= 0)
        //            {
        //                other_interface.ApplyDamage(atk_damage_);
        //                atk_countdown_ = firerate_; //prevents applying damage every frame
        //            }
        //        }
        //    }
        //}
    }*/

    /// <summary>
    /// Visual debug
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, atk_range_);

        Gizmos.color = Color.blue;
        float half_stopdis = GetComponent<NavMeshAgent>().stoppingDistance / 2.0f;
        Gizmos.DrawWireSphere(transform.position + transform.forward * half_stopdis, half_stopdis);
    }
}
