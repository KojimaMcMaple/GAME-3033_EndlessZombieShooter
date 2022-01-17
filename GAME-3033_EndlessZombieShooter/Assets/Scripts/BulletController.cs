using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  The Source file name: BulletController.cs
///  Author's name: Trung Le (Kyle Hunter)
///  Student Number: 101264698
///  Program description: Responsible for moving the individual bullets
///  Date last Modified: See GitHub
///  Revision History: See GitHub
/// </summary>
public class BulletController : MonoBehaviour
{
    [SerializeField] private GlobalEnums.ObjType type_ = GlobalEnums.ObjType.ENEMY; //compares with hit obj, determines which pool to return to
    [SerializeField] private float speed_ = 10f;
    [SerializeField] private int damage_ = 10;
    [SerializeField] private float despawn_time_ = 3f;
    private Vector3 spawn_pos_;
    private Vector3 dir_;
    private BulletManager bullet_manager_;
    private Rigidbody rb_;
    private float timer_ = 0f;

    private void Awake()
    {
        bullet_manager_ = GameObject.FindObjectOfType<BulletManager>();
        rb_ = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb_.velocity = dir_ * speed_;
    }

    private void OnEnable()
    {
        rb_.velocity = dir_ * speed_;
    }

    private void FixedUpdate()
    {
        Move();
        CheckBounds();
    }

    /// <summary>
    /// Moves the bullet
    /// </summary>
    private void Move()
    {
        
    }

    /// <summary>
    /// When bullet 
    /// </summary>
    private void CheckBounds()
    {
        timer_ += Time.deltaTime;
        if (timer_ > despawn_time_)
        {
            timer_ = 0f;
            bullet_manager_.ReturnBullet(this.gameObject, type_);
        }
    }

    /// <summary>
    /// Mutator for private variable
    /// </summary>
    /// <param name="value"></param>
    public void SetSpawnPos(Vector3 value)
    {
        spawn_pos_ = value;
    }

    public void SetDir(Vector3 value)
    {
        dir_ = value;
    }

    /// <summary>
    /// When bullet collides with something, move bullet back to pool
    /// </summary>
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "FieldOfVision")
        {
            return;
        }
        IDamageable<int> other_interface = other.gameObject.GetComponent<IDamageable<int>>();
        if (other_interface != null)
        {
            if (other_interface.obj_type != type_)
            {
                other_interface.ApplyDamage(damage_);
            }
        }
        bullet_manager_.ReturnBullet(this.gameObject, type_);
    }
}
