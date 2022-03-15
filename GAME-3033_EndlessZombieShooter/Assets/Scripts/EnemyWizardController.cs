using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyWizardController : EnemyController
{
    [Header("Wizard Controller")]
    [SerializeField] private float fov_half_angle_ = 39.0f;
    [SerializeField] private Transform bullet_spawn_pos_;
    [SerializeField] private ParticleSystem muzzle_flash_vfx_;
    [SerializeField] private ParticleSystem atk_indicator_vfx_;

    [Space(10)]
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float fall_cooldown = 0.15f;

    [Header("Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool is_grounded = true;
    [Tooltip("Useful for rough ground")]
    public float grounded_offset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float grounded_radius = 0.28f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask ground_layers;

    private float fall_cooldown_delta_;

    private int anim_id_speed_;
    private int anim_id_grounded_;
    private int anim_id_jump_;
    private int anim_id_freefall_;
    private int anim_id_velocity_y_;
    private int anim_id_input_x_;
    private int anim_id_input_y_;
    private int anim_id_take_dam_;
    private int anim_id_is_dead_;
    private int anim_id_die_;
    private int anim_id_atk1_;
    private int anim_id_atk2_;
    private int anim_id_atk3_;

    // MANAGERS
    protected BulletManager bullet_manager_;

    void Awake()
    {
        DoBaseInit();
        bullet_manager_ = FindObjectOfType<BulletManager>();
        AssignAnimationIDs();
        atk_indicator_vfx_.Stop();
    }

    private void Start()
    {
        int clip_idx = GlobalUtils.GetAnimClipIdxByName(animator_, "Attack01");
        GlobalUtils.AddRuntimeAnimEvent(animator_, clip_idx, 0.267f, "DoShoot", 0.0f);
        GlobalUtils.AddRuntimeAnimEvent(animator_, clip_idx, 0.21f, "DoStartAtkHitbox", 0.0f);
        GlobalUtils.AddRuntimeAnimEvent(animator_, clip_idx, 0.367f, "DoEndAtkHitbox", 0.0f);

        // reset our timeouts on start
        fall_cooldown_delta_ = fall_cooldown;
    }

    void Update()
    {
        JumpAndGravity();
        GroundedCheck();

        switch (state_) //state machine
        {
            case GlobalEnums.EnemyState.IDLE:
                //DoPatrol();
                break;
            case GlobalEnums.EnemyState.MOVE_TO_TARGET:
                MoveToTarget();
                break;
            case GlobalEnums.EnemyState.ATTACK:
                DoReorient();
                DoAttack();
                break;
            case GlobalEnums.EnemyState.FLEE:
                DoFlee();
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
        animator_.SetFloat(anim_id_velocity_y_, nav_.velocity.y);
    }

    private void JumpAndGravity()
    {
        if (is_grounded)
        {
            // reset the fall timeout timer
            fall_cooldown_delta_ = fall_cooldown;

            // update animator if using character
            {
                animator_.SetBool(anim_id_jump_, false);
                animator_.SetBool(anim_id_freefall_, false);
            }

        }
        else
        {
            if (nav_.velocity.y > 0)
            {
                animator_.SetBool(anim_id_jump_, true);
            }

            // fall timeout
            if (fall_cooldown_delta_ >= 0.0f)
            {
                fall_cooldown_delta_ -= Time.deltaTime;
            }
            else
            {
                animator_.SetBool(anim_id_freefall_, true);
            }
        }
    }

    /// <summary>
    /// Convert string to int for anim IDs
    /// </summary>
    private void AssignAnimationIDs()
    {
        anim_id_speed_ = Animator.StringToHash("Speed");
        anim_id_grounded_ = Animator.StringToHash("Grounded");
        anim_id_jump_ = Animator.StringToHash("Jump");
        anim_id_freefall_ = Animator.StringToHash("FreeFall");
        anim_id_velocity_y_ = Animator.StringToHash("VelocityY");
        anim_id_input_x_ = Animator.StringToHash("InputX");
        anim_id_input_y_ = Animator.StringToHash("InputY");
        anim_id_atk1_ = Animator.StringToHash("Attack01");
        anim_id_take_dam_ = Animator.StringToHash("TakeDamage");
        anim_id_is_dead_ = Animator.StringToHash("IsDead");
        anim_id_die_ = Animator.StringToHash("Die");
}

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - grounded_offset, transform.position.z);
        is_grounded = Physics.CheckSphere(spherePosition, grounded_radius, ground_layers, QueryTriggerInteraction.Ignore);

        animator_.SetBool(anim_id_grounded_, is_grounded);
    }

    protected override void DoAttack()
    {
        float distance_to_target = Vector3.Distance(transform.position, target_.transform.position);
        if (distance_to_target > atk_range_)
        {
            SetState(GlobalEnums.EnemyState.MOVE_TO_TARGET);
        }
        else if(!is_atk_)
        {
            animator_.SetTrigger(anim_id_atk1_);
            
            is_atk_ = true;
        }

        // LET ENEMY ATTACK FIRST, THEN CHECK FOR FLEE
        if (distance_to_target < flee_range_)
        {
            SetState(GlobalEnums.EnemyState.FLEE);
        }
    }

    /// <summary>
    /// Aggro if player detected
    /// </summary>
    public override void DoAggro()
    {
        if (is_death_ || is_stunned_)
        {
            return;
        }
        Debug.Log("> Wizard DoAggro");
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
        animator_.SetBool(anim_id_is_dead_, true);
        if (state_ != GlobalEnums.EnemyState.DIE)
        {
            animator_.SetTrigger(anim_id_die_);
            nav_.isStopped = true;
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
            Debug.Log("> Wizard resets to IDLE");
            return;
        }
        if (!nav_.isOnNavMesh) //in case enemy is thrown off navmesh
        {
            //Debug.Log("> Wizard out of nav");
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

    private void DoFlee()
    {
        if (!nav_.isOnNavMesh) //in case enemy is thrown off navmesh
        {
            //Debug.Log("> Wizard out of nav");
            return;
        }

        flee_timer_ += Time.deltaTime;

        if (flee_timer_ > flee_timer_max_ || (has_flee_pos_ && HasReachedNavTarget()))
        {
            flee_timer_ = 0f;
            has_flee_pos_ = false;
            Debug.Log("> Wizard resets from FLEE to MOVE_TO_TARGET");
            SetState(GlobalEnums.EnemyState.MOVE_TO_TARGET);
        }
        else if (!has_flee_pos_)
        {
            // FIND FLEE TARGET 
            if (target_ == null)
            {
                Debug.Log("> Wizard has no target to FLEE");
                return;
            }

            Vector3 target_pos = target_.transform.position;
            target_pos.y = transform.position.y;
            Vector3 flee_dir = (target_pos - transform.position).normalized;
            flee_pos_ = transform.position - (flee_dir * (atk_range_/2.0f));
            NavMeshHit hit;
            if (NavMesh.SamplePosition(flee_pos_, out hit, atk_range_ / 4.0f, NavMesh.AllAreas))
            {
                flee_pos_ = hit.position;
                if (nav_.SetDestination(flee_pos_))
                {
                    has_flee_pos_ = true;
                }
                else
                {
                    Debug.Log("> Wizard FLEE SetDestination failed");
                }
            }
            else
            {
                Debug.Log("> Wizard FLEE SamplePosition failed");
            }
        }
    }

    private void DoReorient()
    {
        if (target_ != null)
        {
            //Vector3 target_center = new Vector3(target_.transform.position.x,
            //                                    transform.position.y, //
            //                                    target_.transform.position.z);
            Vector3 self_to_target = target_.transform.position - transform.position;
            if (Vector3.Angle(transform.forward, self_to_target) > fov_half_angle_)
            {
                self_to_target.y = 0;
                Quaternion rotation = Quaternion.LookRotation(self_to_target);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10.0f);
            }
        }
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

    public void DoShoot()
    {
        if (target_!=null)
        {
            float rand_offset_y = Random.Range(0.9f, 1.4f);
            Vector3 target_center = new Vector3(target_.transform.position.x,
                                                target_.transform.position.y + rand_offset_y,
                                                target_.transform.position.z);
            bullet_manager_.GetBullet(bullet_spawn_pos_.position,
                (target_center - bullet_spawn_pos_.position).normalized,
                GlobalEnums.ObjType.ENEMY);
        }
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
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (is_grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - grounded_offset, transform.position.z), grounded_radius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, atk_range_);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, flee_range_);

        Gizmos.color = Color.blue;
        float half_stopdis = GetComponent<NavMeshAgent>().stoppingDistance / 2.0f;
        Gizmos.DrawWireSphere(transform.position + transform.forward* half_stopdis, half_stopdis);
    }
}
