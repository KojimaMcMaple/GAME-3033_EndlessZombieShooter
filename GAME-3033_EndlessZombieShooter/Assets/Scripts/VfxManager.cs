using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  The Source file name: VfxManager.cs
///  Author's name: Trung Le (Kyle Hunter)
///  Student Number: 101264698
///  Program description: Manages the queue
///  Date last Modified: See GitHub
///  Revision History: See GitHub
/// </summary>
[System.Serializable]
public class VfxManager : MonoBehaviour
{
    private Queue<GameObject> hit_vfx_pool_;
    private Queue<GameObject> ultima_vfx_pool_;
    private Queue<GameObject> ultima_vfx2_pool_;
    [SerializeField] private int hit_vfx_num_;
    [SerializeField] private int ultima_vfx_num_;
    [SerializeField] private int ultima_vfx2_num_;
    //public GameObject vfx_obj;

    private VfxFactory factory_;

    private void Awake()
    {
        hit_vfx_pool_ = new Queue<GameObject>();
        ultima_vfx_pool_ = new Queue<GameObject>();
        ultima_vfx2_pool_ = new Queue<GameObject>();
        factory_ = GetComponent<VfxFactory>();
        BuildVfxPool(); //pre-build a certain num of vfxs to improve performance
    }

    /// <summary>
    /// Builds a pool of vfxs in vfx_num amount
    /// </summary>
    private void BuildVfxPool()
    {
        for (int i = 0; i < hit_vfx_num_; i++)
        {
            PreAddVfx(GlobalEnums.VfxType.HIT);
        }
        for (int i = 0; i < ultima_vfx_num_; i++)
        {
            PreAddVfx(GlobalEnums.VfxType.ULTIMA);
        }
        for (int i = 0; i < ultima_vfx2_num_; i++)
        {
            PreAddVfx(GlobalEnums.VfxType.ULTIMA2);
        }
    }

    /// <summary>
    /// Based on AddVfx() without num++, otherwise would cause infinite loop
    /// </summary>
    private void PreAddVfx(GlobalEnums.VfxType type = GlobalEnums.VfxType.HIT)
    {
        //var temp = Instantiate(vfx_obj, this.transform);
        var temp = factory_.CreateVfx(type);

        switch (type)
        {
            case GlobalEnums.VfxType.HIT:
                hit_vfx_pool_.Enqueue(temp);
                break;
            case GlobalEnums.VfxType.ULTIMA:
                ultima_vfx_pool_.Enqueue(temp);
                break;
            case GlobalEnums.VfxType.ULTIMA2:
                ultima_vfx2_pool_.Enqueue(temp);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Uses the factory to spawn one object, add it to the queue, and increase the pool size 
    /// </summary>
    private void AddVfx(GlobalEnums.VfxType type = GlobalEnums.VfxType.HIT)
    {
        //var temp = Instantiate(vfx_obj, this.transform);
        var temp = factory_.CreateVfx(type);

        switch (type)
        {
            case GlobalEnums.VfxType.HIT:
                //temp.SetActive(false);
                hit_vfx_pool_.Enqueue(temp);
                hit_vfx_num_++;
                break;
            case GlobalEnums.VfxType.ULTIMA:
                //temp.SetActive(false);
                ultima_vfx_pool_.Enqueue(temp);
                ultima_vfx_num_++;
                break;
            case GlobalEnums.VfxType.ULTIMA2:
                //temp.SetActive(false);
                ultima_vfx2_pool_.Enqueue(temp);
                ultima_vfx2_num_++;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// // Removes a vfx from the pool and return a ref to it
    /// </summary>
    /// <param name="position"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public GameObject GetVfx(Vector3 position,
                                GlobalEnums.VfxType type = GlobalEnums.VfxType.HIT)
    {
        //Debug.Log(">>> Spawning Vfx...");
        GameObject temp = null;
        GameObject temp2 = null;
        switch (type)
        {
            case GlobalEnums.VfxType.HIT:
                if (hit_vfx_pool_.Count < 1) //add one vfx if pool empty
                {
                    AddVfx(GlobalEnums.VfxType.HIT);
                }
                temp = hit_vfx_pool_.Dequeue();
                break;
            case GlobalEnums.VfxType.ULTIMA:
                if (ultima_vfx_pool_.Count < 1) //add one vfx if pool empty
                {
                    AddVfx(GlobalEnums.VfxType.ULTIMA);
                }
                temp = ultima_vfx_pool_.Dequeue();
                if (ultima_vfx2_pool_.Count < 1) //add one vfx if pool empty
                {
                    AddVfx(GlobalEnums.VfxType.ULTIMA2); //no rotation
                }
                temp2 = ultima_vfx2_pool_.Dequeue();
                break;
            default:
                break;
        }
        temp.transform.position = position;
        temp.SetActive(true);
        temp.GetComponent<VfxController>().DoSpawn();
        if (temp2 != null)
        {
            temp2.transform.position = position;
            temp2.SetActive(true);
            temp2.GetComponent<VfxController>().DoSpawn();
        }
        return temp;
    }

    public GameObject GetVfx(Vector3 position,
                                Vector3 fwd_dir,
                                GlobalEnums.VfxType type = GlobalEnums.VfxType.HIT)
    {
        GameObject temp = null;
        temp = GetVfx(position, type);
        temp.transform.rotation = Quaternion.LookRotation(fwd_dir, Vector3.up);
        return temp;
    }

    /// <summary>
    /// Returns a vfx back into the pool
    /// </summary>
    /// <param name="returned_vfx"></param>
    /// <param name="type"></param>
    public void ReturnVfx(GameObject returned_vfx, GlobalEnums.VfxType type = GlobalEnums.VfxType.HIT)
    {
        returned_vfx.SetActive(false);

        switch (type)
        {
            case GlobalEnums.VfxType.HIT:
                hit_vfx_pool_.Enqueue(returned_vfx);
                break;
            case GlobalEnums.VfxType.ULTIMA:
                ultima_vfx_pool_.Enqueue(returned_vfx);
                break;
            default:
                break;
        }
    }
}
