using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  The Source file name: IDamageable.cs
///  Author's name: Trung Le (Kyle Hunter)
///  Student Number: 101264698
///  Program description: Interface for entities that can be damaged
///  Date last Modified: See GitHub
///  Revision History: See GitHub
/// </summary>
public interface IDamageable<T>
{
    void Init(); //Link vars to class vars
    T health { get; set; } //Health points
    GlobalEnums.ObjType obj_type { get; set; } //Type of gameobject
    void ApplyDamage(T damage_value); //Deals damage to object
    void HealDamage(T heal_value); //Adds health to object
}
