using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMetalonController : EnemyController
{
    [SerializeField] private ParticleSystem atk_indicator_vfx_;
    private int anim_id_run_;
    private int anim_id_die_;
    private int anim_id_take_dam_;
    private int anim_id_atk1_;
    private int anim_id_atk2_;
    private int anim_id_atk3_;

    void Awake()
    {
        DoBaseInit();
        AssignAnimationIDs();
        atk_indicator_vfx_.Stop();
    }

    private void Start()
    {
        int clip_idx = GlobalUtils.GetAnimClipIdxByName(animator_, "Stab Attack");
        GlobalUtils.AddRuntimeAnimEvent(animator_, clip_idx, 0.06f, "DoStartAtkHitbox", 0.0f);
        GlobalUtils.AddRuntimeAnimEvent(animator_, clip_idx, 0.2f, "DoEndAtkHitbox", 0.0f);
        clip_idx = GlobalUtils.GetAnimClipIdxByName(animator_, "Smash Attack");
        GlobalUtils.AddRuntimeAnimEvent(animator_, clip_idx, 0.11f, "DoStartAtkHitbox", 0.0f);
        GlobalUtils.AddRuntimeAnimEvent(animator_, clip_idx, 0.25f, "DoEndAtkHitbox", 0.0f);
        clip_idx = GlobalUtils.GetAnimClipIdxByName(animator_, "Cast Spell");
        GlobalUtils.AddRuntimeAnimEvent(animator_, clip_idx, 0.05f, "DoStartAtkHitbox", 0.0f);
        GlobalUtils.AddRuntimeAnimEvent(animator_, clip_idx, 0.2f, "DoEndAtkHitbox", 0.0f);
    }

    void Update()
    {
        switch (state_) //state machine
        {
            case GlobalEnums.EnemyState.IDLE:
                animator_.SetBool(anim_id_run_, false);
                //DoPatrol();
                break;
            case GlobalEnums.EnemyState.MOVE_TO_TARGET:
                animator_.SetBool(anim_id_run_, true);
                MoveToTarget();
                break;
            case GlobalEnums.EnemyState.ATTACK:
                animator_.SetBool(anim_id_run_, false);
                DoAttack();
                break;
            case GlobalEnums.EnemyState.STUNNED:
                animator_.SetBool(anim_id_run_, false);
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
    }

    /// <summary>
    /// Convert string to int for anim IDs
    /// </summary>
    private void AssignAnimationIDs()
    {
        anim_id_run_ = Animator.StringToHash("Run Forward");
        anim_id_die_ = Animator.StringToHash("Die");
        anim_id_take_dam_ = Animator.StringToHash("Take Damage");
        anim_id_atk1_ = Animator.StringToHash("Smash Attack");
        anim_id_atk2_ = Animator.StringToHash("Stab Attack");
        anim_id_atk3_ = Animator.StringToHash("Cast Spell");
    }

    protected override void DoAttack()
    {
        if (Vector3.Distance(transform.position, target_.transform.position) > atk_range_)
        {
            SetState(GlobalEnums.EnemyState.MOVE_TO_TARGET);
        }
        else if(!is_atk_)
        {
            int atk_id = Random.Range(0, 3); //[minInclusive..maxExclusive)
            print(">>> atk_id is " + atk_id);
            if (atk_id == 0)
            {
                animator_.SetTrigger(anim_id_atk1_);
            }
            else if (atk_id == 1)
            {
                animator_.SetTrigger(anim_id_atk2_);
            }
            else if (atk_id == 2)
            {
                animator_.SetTrigger(anim_id_atk3_);
            }
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
            animator_.SetTrigger(anim_id_take_dam_);
            flinch_cooldown_delta_ = flinch_cooldown_;
        }
    }

    protected override void DoDeath()
    {
        if (state_ != GlobalEnums.EnemyState.DIE)
        {
            animator_.SetTrigger(anim_id_die_);
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
        atk_indicator_vfx_.Stop();
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
