using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///  The Source file name: EnemyController.cs
///  Author's name: Trung Le (Kyle Hunter)
///  Student Number: 101264698
///  Program description: Defines behavior for the enemy
///  Date last Modified: See GitHub
///  Revision History: See GitHub
/// </summary>
public class EnemyController : MonoBehaviour, IDamageable<int>
{
    // BASE STATS
    [SerializeField] protected int hp_ = 50;
    [SerializeField] protected int score_ = 50;
    [SerializeField] protected float speed_ = 0.75f;
    [Tooltip("Prevents applying damage twice in the same attack")]
    [SerializeField] protected float hit_cooldown_ = 0.47f;
    protected float hit_cooldown_delta_ = 0.0f;
    [SerializeField] protected float atk_range_ = 1.5f;
    [SerializeField] protected float flee_range_ = 1.0f; //should be smaller than nav_.stoppingDistance
    [Tooltip("How long enemy should stay fleeing, prevents fleeing forever")]
    [SerializeField] protected float flee_timer_max_ = 7.0f; //prevents fleeing forever, enemy has to attack
    protected float flee_timer_ = 0f; //prevents fleeing forever, enemy has to attack
    [SerializeField] protected int atk_damage_ = 10;
    [SerializeField] protected float flinch_cooldown_ = 1.25f;
    protected float flinch_cooldown_delta_ = 0.0f;
    [SerializeField] protected float launched_recover_cooldown_ = 5.0f;
    protected Vector3 start_pos_;

    // UNITY COMPONENTS
    protected Animator animator_;
    protected NavMeshAgent nav_;
    protected Rigidbody rb_;
    protected Collider collider_;
    [SerializeField] protected GameObject rig_;
    [SerializeField] protected Rigidbody hip_rb_;
    protected List<Collider> ragdoll_cols_ = new List<Collider>();
    protected List<Rigidbody> ragdoll_rbs_ = new List<Rigidbody>();

    // LOGIC
    protected GlobalEnums.ObjType type_ = GlobalEnums.ObjType.ENEMY;
    protected GlobalEnums.EnemyState state_ = GlobalEnums.EnemyState.IDLE;
    protected Transform fov_;
    protected GameObject target_;
    protected bool can_move_ = true;
    protected Vector3 flee_pos_;
    protected bool has_flee_pos_ = false;
    protected bool is_atk_hitbox_active_ = false;
    protected bool is_atk_ = false;
    protected bool is_stunned_ = false;
    protected Coroutine launched_coroutine_ = null;
    protected bool is_death_ = false;

    // MANAGERS
    protected VfxManager vfx_manager_;
    private ObjManager obj_manager_;

    // SFX
    [SerializeField] protected List<AudioClip> attack_sfx_ = new List<AudioClip>();
    [SerializeField] protected List<AudioClip> damaged_sfx_ = new List<AudioClip>();
    protected AudioSource audio_source_;

    protected void DoBaseInit()
    {
        start_pos_ = transform.position;
        animator_ = GetComponent<Animator>();
        nav_ = GetComponent<NavMeshAgent>();
        rb_ = GetComponent<Rigidbody>();
        collider_ = GetComponent<Collider>();
        audio_source_ = GetComponent<AudioSource>();
        fov_ = transform.Find("EnemyFoV");

        vfx_manager_ = FindObjectOfType<VfxManager>();
        hit_cooldown_delta_ = hit_cooldown_;

        obj_manager_ = FindObjectOfType<ObjManager>();

        // RAGDOLL
        if (rig_ != null)
        {
            Collider[] cols = rig_.GetComponentsInChildren<Collider>();
            foreach (Collider item in cols)
            {
                if (!item.CompareTag("FieldOfVision") && !item.CompareTag("AtkHitBox"))
                {
                    ragdoll_cols_.Add(item);
                }
            }

            Rigidbody[] rbs = rig_.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody item in rbs)
            {
                if (!item.CompareTag("FieldOfVision") && !item.CompareTag("AtkHitBox"))
                {
                    ragdoll_rbs_.Add(item);
                }
            }
            SetRagdollMode(false);
        }

        Init(); //IDamageable method
    }

    public void DoSpawnIn()
    {
        health = hp_;
        is_death_ = false;
        state_ = GlobalEnums.EnemyState.IDLE;
        SetNavMeshMode(false);
        SetRagdollMode(true);
        gameObject.SetActive(true);
        StartCoroutine(DoEndSpawnIn());
    }

    protected IEnumerator DoEndSpawnIn()
    {
        yield return new WaitForSeconds(4.0f);
        SetRagdollMode(false);
        SetNavMeshMode(true);
        DoAggro();
    }

    protected void DoBaseUpdate()
    {
        switch (state_) //state machine
        {
            case GlobalEnums.EnemyState.IDLE:
                animator_.SetBool("IsAttacking", false);
                break;
            case GlobalEnums.EnemyState.ATTACK:
                animator_.SetBool("IsAttacking", true);
                DoAttack();
                break;
            default:
                break;
        }
    }

    protected void DoBaseFixedUpdate()
    {
        if (!animator_.enabled)
        {
            transform.position = hip_rb_.position;
        }
    }

    /// <summary>
    /// Attack behaviour
    /// </summary>
    protected virtual void DoAttack()
    {
    }

    /// <summary>
    /// Aggro behaviour
    /// </summary>
    public virtual void DoAggro()
    {
        Debug.Log("> Base DoAggro");
        //set target and state?
    }

    protected virtual void DoDeath()
    {

    }

    protected virtual void DoFlinch(GlobalEnums.FlinchType flinch_mode = GlobalEnums.FlinchType.DEFAULT)
    {
        //animation?
    }

    protected virtual void DoSetCanMove(bool value)
    {
        can_move_ = value;
    }

    public virtual void DoStartAtkHitbox()
    {
        DoSetCanMove(false);
        SetAtkHitboxActive();
        //atk_indicator_vfx_.Play();
    }

    public virtual void DoEndAtkHitbox()
    {
        DoSetCanMove(true);
        SetAtkHitboxInactive();
        //atk_indicator_vfx_.Stop();
    }

    public virtual void SetNavMeshMode(bool value) //can be integrated in SetRagdollMode ?
    {
        if (value)
        {
            nav_.enabled = true;
            nav_.updatePosition = true;
            nav_.updateRotation = true;
            nav_.isStopped = false;
            rb_.isKinematic = true;
            DoSetCanMove(true);
        }
        else
        {
            DoSetCanMove(false);
            if (nav_.isOnNavMesh) //in case enemy is thrown off navmesh
            {
                nav_.isStopped = true;
            }
            nav_.updatePosition = false;
            nav_.updateRotation = false;
            nav_.enabled = false;
            rb_.isKinematic = false;
        }
    }

    public virtual void SetRagdollMode(bool value)
    {
        animator_.enabled = !value;
        foreach (Collider item in ragdoll_cols_)
        {
            item.enabled = value;
        }
        foreach (Rigidbody item in ragdoll_rbs_)
        {
            item.isKinematic = !value;
        }
        //collider_.enabled = !value;
        rb_.isKinematic = !value;
    }

    public void DoLaunchedToAir(Vector3 src_pos, float force, int damage)
    {
        state_ = GlobalEnums.EnemyState.STUNNED;
        is_stunned_ = true;

        SetNavMeshMode(false);

        //animator_.applyRootMotion = false; //very important

        float rand_height = Random.Range(3, 4.5f);
        float rand_force = force * Random.Range(0.8f, 1.15f);
        Vector3 dir = (new Vector3(transform.position.x, transform.position.y + rand_height, transform.position.z) - src_pos).normalized;
        if (rig_ != null) //has ragdoll
        {
            SetRagdollMode(true);
            rb_.AddForce(dir * rand_force, ForceMode.Impulse);
            //rb_.AddTorque(new Vector3(0f, 0f, -1.0f) * rand_force, ForceMode.Impulse);
            //hip_rb_.AddForce(dir * rand_force, ForceMode.Impulse);
        }
        else //no ragdoll
        {
            rb_.AddForce(dir * rand_force, ForceMode.Impulse);
            rb_.AddTorque(new Vector3(0f, 0f, -1.0f) * rand_force, ForceMode.Impulse);
            //rb_.AddTorque(dir * rand_force, ForceMode.Impulse);
        }

        ApplyDamage(damage, GlobalEnums.FlinchType.ABSOLUTE);

        if (launched_coroutine_ != null)
        {
            StopCoroutine(launched_coroutine_);
        }
        launched_coroutine_ = StartCoroutine(TryLaunchedRecover());
    }

    public void DoEndLaunchedToAir()
    {
        if (health > 0)
        {
            if (rig_ != null) //has ragdoll
            {
                SetRagdollMode(false);
            }

            SetNavMeshMode(true);

            is_stunned_ = false;
            DoAggro();
        }
    }

    //public IEnumerator TryLaunchRagdoll(Vector3 force)
    //{
    //    yield return new WaitForSeconds(0.5f);
    //    foreach (Rigidbody item in ragdoll_rbs_)
    //    {
    //        item.AddForce(force, ForceMode.Impulse);
    //    }
    //}

    public IEnumerator TryLaunchedRecover()
    {
        yield return new WaitForSeconds(launched_recover_cooldown_);
        DoEndLaunchedToAir();
    }

    public bool HasReachedNavTarget() //http://answers.unity.com/answers/746157/view.html
    {
        if (!nav_.isOnNavMesh) //in case enemy is thrown off navmesh
        {
            //Debug.Log("> Wizard out of nav");
            return true; //or return false ???
        }

        if (!nav_.pathPending)
        {
            if (nav_.remainingDistance <= nav_.stoppingDistance)
            {
                if (!nav_.hasPath || nav_.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Mutator for private variable
    /// </summary>
    public void SetState(GlobalEnums.EnemyState value)
    {
        state_ = value;
        Debug.Log(state_);
    }

    /// <summary>
    /// Accessor for private variable
    /// </summary>
    public GameObject GetTarget()
    {
        return target_;
    }

    /// <summary>
    /// Mutator for private variable
    /// </summary>
    public void SetTarget(GameObject obj)
    {
        target_ = obj;
    }

    /// <summary>
    /// Accessor for private variable
    /// </summary>
    public bool IsAtkHitboxActive()
    {
        return is_atk_hitbox_active_;
    }

    /// <summary>
    /// Mutator for private variable
    /// </summary>
    public void SetAtkHitboxActive()
    {
        SetAtkHitboxActive(true);
        audio_source_.PlayOneShot(attack_sfx_[Random.Range(0, attack_sfx_.Count)]);
    }

    /// <summary>
    /// Mutator for private variable
    /// </summary>
    public void SetAtkHitboxActive(bool value)
    {
        is_atk_hitbox_active_ = value;
    }

    /// <summary>
    /// Mutator for private variable
    /// </summary>
    public void SetAtkHitboxInactive()
    {
        SetAtkHitboxActive(false);
        is_atk_ = false;
    }

    public void DoDealDamageToIDamageable(IDamageable<int> other)
    {
        if (hit_cooldown_delta_ <= 0)
        {
            other.ApplyDamage(atk_damage_);
            hit_cooldown_delta_ = hit_cooldown_; //prevents applying damage every frame
        }
    }

    protected IEnumerator Despawn()
    {
        yield return new WaitForSeconds(8.0f);
        //this.gameObject.SetActive(false); //SetActive in ReturnObj
        obj_manager_.ReturnObj(this.gameObject, type_);
    }

    /// <summary>
    /// IDamageable methods
    /// </summary>
    public void Init() //Link hp to class hp
    {
        health = hp_;
        obj_type = GlobalEnums.ObjType.ENEMY;
    }
    public int health { get; set; } //Health points
    public GlobalEnums.ObjType obj_type { get; set; } //Type of gameobject
    public void ApplyDamage(int damage_value, GlobalEnums.FlinchType flinch_mode = GlobalEnums.FlinchType.DEFAULT) //Deals damage to this object
    {
        if (!is_death_)
        {
            DoAggro();
            health -= damage_value;
            if (health <= 0)
            {
                //explode_manager_.GetObj(this.transform.position, obj_type);
                //game_manager_.IncrementScore(score_);
                //this.gameObject.SetActive(false);
                is_death_ = true;
                if (state_ != GlobalEnums.EnemyState.DIE)
                {
                    DoDeath();
                }
            }
            else
            {
                DoFlinch(flinch_mode);
            }
        }
        audio_source_.PlayOneShot(damaged_sfx_[Random.Range(0, damaged_sfx_.Count)]); //SFX
        //Debug.Log(">>> Enemy HP is " + health.ToString());
    }
    public void HealDamage(int heal_value) { } //Adds health to object

}
