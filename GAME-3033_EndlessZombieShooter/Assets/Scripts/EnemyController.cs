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
    [SerializeField] protected int hp_ = 100;
    [SerializeField] protected int score_ = 50;
    [SerializeField] protected float speed_ = 0.75f;
    [SerializeField] protected float firerate_ = 0.47f;
    [SerializeField] protected int collision_damage_ = 20;
    protected float atk_countdown_ = 0.0f;
    protected Vector3 start_pos_;

    // UNITY COMPONENTS
    protected Animator animator_;
    protected NavMeshAgent nav_;

    // LOGIC
    protected Transform fov_;
    protected GlobalEnums.EnemyState state_ = GlobalEnums.EnemyState.IDLE;
    protected GameObject target_;
    protected bool is_atk_hitbox_active_ = false;
    protected GlobalEnums.ObjType type_ = GlobalEnums.ObjType.ENEMY;

    // MANAGERS
    protected BulletManager bullet_manager_;
    //protected ExplosionManager explode_manager_;
    //protected FoodManager food_manager_;
    //protected GameManager game_manager_;

    // VFX
    //protected VfxSpriteFlash flash_vfx_;

    // SFX
    [SerializeField] protected AudioClip attack_sfx_;
    [SerializeField] protected AudioClip damaged_sfx_;
    protected AudioSource audio_source_;

    protected void DoBaseInit()
    {
        start_pos_ = transform.position;
        animator_ = GetComponent<Animator>();
        nav_ = GetComponent<NavMeshAgent>();
        //explode_manager_ =   GameObject.FindObjectOfType<ExplosionManager>();
        //food_manager_ =     GameObject.FindObjectOfType<FoodManager>();
        //game_manager_ =     GameObject.FindObjectOfType<GameManager>();
        //flash_vfx_ = GetComponent<VfxSpriteFlash>();
        audio_source_ = GetComponent<AudioSource>();
        fov_ = transform.Find("FieldOfVision");

        bullet_manager_ = GameObject.FindObjectOfType<BulletManager>();
        atk_countdown_ = firerate_;

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
    }

    /// <summary>
    /// Mutator for private variable
    /// </summary>
    public void SetState(GlobalEnums.EnemyState value)
    {
        state_ = value;
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
    public void SetAtkHitboxActive(bool value)
    {
        is_atk_hitbox_active_ = value;
    }

    /// <summary>
    /// Mutator for private variable
    /// </summary>
    public void SetAtkHitboxActive()
    {
        SetAtkHitboxActive(true);
        audio_source_.PlayOneShot(attack_sfx_);
    }

    /// <summary>
    /// Mutator for private variable
    /// </summary>
    public void SetAtkHitboxInactive()
    {
        SetAtkHitboxActive(false);
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
    public void ApplyDamage(int damage_value) //Deals damage to this object
    {
        DoAggro();
        health -= damage_value;
        //flash_vfx_.DoFlash();
        audio_source_.PlayOneShot(damaged_sfx_);
        if (health <= 0)
        {
            //explode_manager_.GetObj(this.transform.position, obj_type);
            //food_manager_.GetObj(this.transform.position, (GlobalEnums.FoodType)Random.Range(0, (int)GlobalEnums.FoodType.NUM_OF_TYPE));
            //game_manager_.IncrementScore(score_);
            this.gameObject.SetActive(false);
        }
        Debug.Log(">>> Enemy HP is " + health.ToString());
    }
    public void HealDamage(int heal_value) { } //Adds health to object
}
