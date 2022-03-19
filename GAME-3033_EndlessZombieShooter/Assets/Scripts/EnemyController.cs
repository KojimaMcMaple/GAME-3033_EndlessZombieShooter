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

    // LOGIC
    protected GlobalEnums.ObjType type_ = GlobalEnums.ObjType.ENEMY;
    protected GlobalEnums.EnemyState state_ = GlobalEnums.EnemyState.IDLE;
    protected Transform fov_;
    protected GameObject target_;
    protected Vector3 flee_pos_;
    protected bool has_flee_pos_ = false;
    protected bool is_atk_hitbox_active_ = false;
    protected bool is_atk_ = false;
    protected bool is_stunned_ = false;
    protected Coroutine launched_coroutine_ = null;
    protected bool is_death_ = false;

    // MANAGERS
    protected VfxManager vfx_manager_;

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
        audio_source_ = GetComponent<AudioSource>();
        fov_ = transform.Find("FieldOfVision");

        vfx_manager_ = FindObjectOfType<VfxManager>();
        hit_cooldown_delta_ = hit_cooldown_;

        Init(); //IDamageable method
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

    public void DoLaunchedToAir(Vector3 src_pos, float force, int damage)
    {
        state_ = GlobalEnums.EnemyState.STUNNED;
        is_stunned_ = true;

        nav_.isStopped = true;
        nav_.updatePosition = false;
        nav_.updateRotation = false;
        nav_.enabled = false;

        rb_.isKinematic = false;

        //animator_.applyRootMotion = false; //very important

        float rand_height = Random.Range(3, 4.5f);
        float rand_force = force * Random.Range(0.8f, 1.15f);
        Vector3 dir = (new Vector3(transform.position.x, transform.position.y + rand_height, transform.position.z) - src_pos).normalized;
        rb_.AddForce(dir * rand_force, ForceMode.Impulse);
        rb_.AddTorque(dir * rand_force, ForceMode.Impulse);

        ApplyDamage(damage, GlobalEnums.FlinchType.ABSOLUTE);

        if (launched_coroutine_ != null)
        {
            StopCoroutine(launched_coroutine_);
        }
        launched_coroutine_ = StartCoroutine(TryLaunchedRecover());
    }

    public void DoEndLaunchedToAir()
    {
        nav_.enabled = true;
        nav_.updatePosition = true;
        nav_.updateRotation = true;
        nav_.isStopped = false;
        rb_.isKinematic = true;

        if (health > 0)
        {
            is_stunned_ = false;
            DoAggro();
        }
    }

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
        this.gameObject.SetActive(false);
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
