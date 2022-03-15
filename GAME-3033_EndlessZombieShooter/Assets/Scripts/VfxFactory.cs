using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  The Source file name: VfxFactory.cs
///  Author's name: Trung Le (Kyle Hunter)
///  Student Number: 101264698
///  Program description: Manages the type of object to spawn
///  Date last Modified: See GitHub
///  Revision History: See GitHub
/// </summary>
[System.Serializable]
public class VfxFactory : MonoBehaviour
{
    [Header("Vfx Types")]
    [SerializeField] private GameObject default_hit_vfx_;
    [SerializeField] private GameObject ultima_vfx1_;
    [SerializeField] private GameObject ultima_vfx2_;

    /// <summary>
    /// Instantiates an object and returns a reference to it
    /// </summary>
    /// <returns></returns>
    public GameObject CreateVfx(GlobalEnums.VfxType type = GlobalEnums.VfxType.HIT)
    {
        GameObject temp = null;
        switch (type)
        {
            case GlobalEnums.VfxType.HIT:
                temp = Instantiate(default_hit_vfx_, this.transform);
                break;
            case GlobalEnums.VfxType.ULTIMA:
                temp = Instantiate(ultima_vfx1_, this.transform);
                break;
            case GlobalEnums.VfxType.ULTIMA2:
                temp = Instantiate(ultima_vfx2_, this.transform);
                break;
            default:
                break;
        }
        temp.SetActive(false);
        return temp;
    }
}
