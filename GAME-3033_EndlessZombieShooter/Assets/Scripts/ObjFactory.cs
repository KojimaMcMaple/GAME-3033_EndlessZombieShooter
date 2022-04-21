using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  The Source file name: ObjFactory.cs
///  Author's name: Trung Le (Kyle Hunter)
///  Student Number: 101264698
///  Program description: Manages the type of object to spawn
///  Date last Modified: See GitHub
///  Revision History: See GitHub
/// </summary>
[System.Serializable]
public class ObjFactory : MonoBehaviour
{
    [Header("Obj Types")]
    public List<GameObject> objs = new List<GameObject>();

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            objs.Add(child.gameObject);
        }
    }

    /// <summary>
    /// Instantiates an object and returns a reference to it
    /// </summary>
    /// <returns></returns>
    public GameObject CreateObj(GlobalEnums.ObjType type = GlobalEnums.ObjType.PLAYER, int idx = 0)
    {
        GameObject temp = null;
        if (idx < objs.Count)
        {
            temp = Instantiate(objs[idx], this.transform);
        }
        else
        {
            temp = Instantiate(objs[0], this.transform);
        }
        
        temp.SetActive(false);
        return temp;
    }

    public GameObject CreateRandObj(GlobalEnums.ObjType type = GlobalEnums.ObjType.PLAYER)
    {
        GameObject temp = null;
        int idx = Random.Range(0, objs.Count);
        temp = Instantiate(objs[idx], this.transform);

        temp.SetActive(false);
        return temp;
    }
}
