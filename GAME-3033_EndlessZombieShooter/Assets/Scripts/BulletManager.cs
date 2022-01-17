using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  The Source file name: BulletManager.cs
///  Author's name: Trung Le (Kyle Hunter)
///  Student Number: 101264698
///  Program description: Manages the queue
///  Date last Modified: See GitHub
///  Revision History: See GitHub
/// </summary>
[System.Serializable]
public class BulletManager : MonoBehaviour
{
    public Queue<GameObject> player_bullet_pool;
    public Queue<GameObject> enemy_bullet_pool;
    public int player_bullet_num;
    public int enemy_bullet_num;
    //public GameObject bullet_obj;

    private BulletFactory factory_;

    private void Awake()
    {
        player_bullet_pool = new Queue<GameObject>();
        enemy_bullet_pool = new Queue<GameObject>();
        factory_ = GetComponent<BulletFactory>();
        BuildBulletPool(); //pre-build a certain num of bullets to improve performance
    }

    /// <summary>
    /// Builds a pool of bullets in bullet_num amount
    /// </summary>
    private void BuildBulletPool()
    {
        for (int i = 0; i < player_bullet_num; i++)
        {
            PreAddBullet(GlobalEnums.ObjType.PLAYER);
        }
        for (int i = 0; i < enemy_bullet_num; i++)
        {
            PreAddBullet(GlobalEnums.ObjType.ENEMY);
        }
    }

    /// <summary>
    /// Based on AddBullet() without num++, otherwise would cause infinite loop
    /// </summary>
    private void PreAddBullet(GlobalEnums.ObjType type = GlobalEnums.ObjType.PLAYER)
    {
        //var temp = Instantiate(bullet_obj, this.transform);
        var temp = factory_.CreateBullet(type);

        switch (type)
        {
            case GlobalEnums.ObjType.PLAYER:
                player_bullet_pool.Enqueue(temp);
                break;
            case GlobalEnums.ObjType.ENEMY:
                enemy_bullet_pool.Enqueue(temp);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Uses the factory to spawn one object, add it to the queue, and increase the pool size 
    /// </summary>
    private void AddBullet(GlobalEnums.ObjType type = GlobalEnums.ObjType.PLAYER)
    {
        //var temp = Instantiate(bullet_obj, this.transform);
        var temp = factory_.CreateBullet(type);

        switch (type)
        {
            case GlobalEnums.ObjType.PLAYER:
                //temp.SetActive(false);
                player_bullet_pool.Enqueue(temp);
                player_bullet_num++;
                break;
            case GlobalEnums.ObjType.ENEMY:
                //temp.SetActive(false);
                enemy_bullet_pool.Enqueue(temp);
                enemy_bullet_num++;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// // Removes a bullet from the pool and return a ref to it
    /// </summary>
    /// <param name="position"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public GameObject GetBullet(Vector3 position,
                                Vector3 forward_direction,
                                GlobalEnums.ObjType type = GlobalEnums.ObjType.PLAYER)
    {
        GameObject temp = null;
        switch (type)
        {
            case GlobalEnums.ObjType.PLAYER:
                if (player_bullet_pool.Count < 1) //add one bullet if pool empty
                {
                    AddBullet(GlobalEnums.ObjType.PLAYER);
                }
                temp = player_bullet_pool.Dequeue();
                break;
            case GlobalEnums.ObjType.ENEMY:
                if (enemy_bullet_pool.Count < 1) //add one bullet if pool empty
                {
                    AddBullet(GlobalEnums.ObjType.ENEMY);
                }
                temp = enemy_bullet_pool.Dequeue();
                break;
            default:
                break;
        }
        temp.transform.position = position;
        //temp.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, forward_direction, 2f * 3.14f, 0.0f), Vector3.up);
        temp.transform.rotation = Quaternion.LookRotation(forward_direction, Vector3.up);
        temp.GetComponent<BulletController>().SetSpawnPos(position);
        temp.GetComponent<BulletController>().SetDir(forward_direction);
        temp.SetActive(true);
        return temp;
    }

    /// <summary>
    /// Returns a bullet back into the pool
    /// </summary>
    /// <param name="returned_bullet"></param>
    /// <param name="type"></param>
    public void ReturnBullet(GameObject returned_bullet, GlobalEnums.ObjType type = GlobalEnums.ObjType.PLAYER)
    {
        returned_bullet.SetActive(false);

        switch (type)
        {
            case GlobalEnums.ObjType.PLAYER:
                player_bullet_pool.Enqueue(returned_bullet);
                break;
            case GlobalEnums.ObjType.ENEMY:
                enemy_bullet_pool.Enqueue(returned_bullet);
                break;
            default:
                break;
        }
    }
}
