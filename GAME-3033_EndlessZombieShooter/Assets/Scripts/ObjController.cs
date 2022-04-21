using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  The Source file name: ObjController.cs
///  Author's name: Trung Le (Kyle Hunter)
///  Student Number: 101264698
///  Program description: Responsible for moving the individual objs
///  Date last Modified: See GitHub
///  Revision History: See GitHub
/// </summary>
public class ObjController : MonoBehaviour
{
    [SerializeField] private GlobalEnums.ObjType type_ = GlobalEnums.ObjType.ENEMY; //compares with hit obj, determines which pool to return to
    [SerializeField] private float speed_ = 10f;
    [SerializeField] private float active_timer_ = 7f;
    private Vector3 spawn_pos_;
    private Vector3 dir_ = Vector3.down;
    private ObjManager obj_manager_;
    private Rigidbody rb_;
    private float timer_ = 0f;
    private bool is_activated_ = false;
    public bool is_interactable = true;
    public bool is_held = false;

    private void Awake()
    {
        obj_manager_ = GameObject.FindObjectOfType<ObjManager>();
        rb_ = GetComponent<Rigidbody>();
    }

    //private void Start()
    //{
    //    rb_.velocity = dir_ * speed_;
    //}

    //private void OnEnable()
    //{
    //    rb_.velocity = dir_ * speed_;
    //}

    private void FixedUpdate()
    {
        CheckActive();
    }

    /// <summary>
    /// 
    /// </summary>
    private void CheckActive()
    {
        if (is_activated_ && is_interactable)
        {
            timer_ += Time.deltaTime;
            if (timer_ > active_timer_)
            {
                SetObjInactive();
            }
        }
        if (is_held)
        {
            timer_ = 0f;
        }
    }

    private void SetObjInactive()
    {
        is_interactable = false;
        GetComponent<Renderer>().material = obj_manager_.inactive_mat;
        rb_.isKinematic = true;
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

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "ObjActivateTrigger")
        {
            is_activated_ = true;
        }
    }
}
