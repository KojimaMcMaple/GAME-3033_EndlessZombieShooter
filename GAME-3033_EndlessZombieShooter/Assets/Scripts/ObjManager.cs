using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  The Source file name: ObjManager.cs
///  Author's name: Trung Le (Kyle Hunter)
///  Student Number: 101264698
///  Program description: Manages the queue
///  Date last Modified: See GitHub
///  Revision History: See GitHub
/// </summary>
[System.Serializable]
public class ObjManager : MonoBehaviour
{
    public Queue<GameObject> obj_pool;
    public int obj_num;
    public Material inactive_mat;
    public List<Transform> spawn_locs = new List<Transform>();
    public Vector2 spawn_interval = new Vector2(5.0f, 10.0f);
    private float spawn_timer_ = 0.0f;
    private float timer_ = 0.0f;
    //public GameObject obj_obj;

    private ObjFactory factory_;

    private Player.ThirdPersonController player_;

    private void Awake()
    {
        obj_pool = new Queue<GameObject>();
        factory_ = GetComponent<ObjFactory>();
        spawn_timer_ = Random.Range(spawn_interval.x, spawn_interval.y);

        player_ = FindObjectOfType<Player.ThirdPersonController>();
    }

    private void FixedUpdate()
    {
        timer_ += Time.deltaTime;
        if (timer_ > spawn_timer_)
        {
            timer_ = 0;
            int idx = Random.Range(0, spawn_locs.Count);
            Vector3 spawn_pos = spawn_locs[idx].position;
            GetObj(spawn_pos);
            spawn_timer_ = Random.Range(spawn_interval.x, spawn_interval.y);
        }
    }

    //private void Start()
    //{
    //    BuildObjPool(); //pre-build a certain num of objs to improve performance
    //}

    /// <summary>
    /// Builds a pool of objs in obj_num amount
    /// </summary>
    private void BuildObjPool()
    {
        for (int i = 0; i < obj_num; i++)
        {
            PreAddObj(GlobalEnums.ObjType.ENEMY);
        }
    }

    /// <summary>
    /// Based on AddObj() without num++, otherwise would cause infinite loop
    /// </summary>
    private void PreAddObj(GlobalEnums.ObjType type = GlobalEnums.ObjType.ENEMY)
    {
        //var temp = Instantiate(obj_obj, this.transform);
        var temp = factory_.CreateObj(type);

        switch (type)
        {
            case GlobalEnums.ObjType.ENEMY:
                obj_pool.Enqueue(temp);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Uses the factory to spawn one object, add it to the queue, and increase the pool size 
    /// </summary>
    private void AddObj(GlobalEnums.ObjType type = GlobalEnums.ObjType.ENEMY, int idx = -1)
    {
        //var temp = Instantiate(obj_obj, this.transform);
        GameObject temp = null;
        if (idx == -1)
        {
            temp = factory_.CreateRandObj(type);
        }
        else
        {
            temp = factory_.CreateObj(type, idx);
        }

        if (temp == null) //safety break
        {
            return;
        }

        switch (type)
        {
            case GlobalEnums.ObjType.ENEMY:
                //temp.SetActive(false);
                obj_pool.Enqueue(temp);
                obj_num++;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// // Removes a obj from the pool and return a ref to it
    /// </summary>
    /// <param name="position"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public GameObject GetObj(Vector3 position,
                                GlobalEnums.ObjType type = GlobalEnums.ObjType.ENEMY)
    {
        GameObject temp = null;
        switch (type)
        {
            case GlobalEnums.ObjType.ENEMY:
                if (obj_pool.Count < 1) //add one obj if pool empty
                {
                    AddObj(GlobalEnums.ObjType.ENEMY);
                }
                temp = obj_pool.Dequeue();
                break;
            default:
                break;
        }
        temp.transform.position = position;
        temp.GetComponent<EnemyController>().DoSpawnIn();
        //temp.SetActive(true);
        return temp;
    }

    /// <summary>
    /// Returns a obj back into the pool
    /// </summary>
    /// <param name="returned_obj"></param>
    /// <param name="type"></param>
    public void ReturnObj(GameObject returned_obj, GlobalEnums.ObjType type = GlobalEnums.ObjType.ENEMY)
    {
        player_.IncrementKillcount();
        returned_obj.SetActive(false);

        switch (type)
        {
            case GlobalEnums.ObjType.ENEMY:
                obj_pool.Enqueue(returned_obj);
                break;
            default:
                break;
        }
    }
}
