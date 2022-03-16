using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  The Source file name: VfxController.cs
///  Author's name: Trung Le (Kyle Hunter)
///  Student Number: 101264698
///  Program description: Responsible for moving the individual vfxs
///  Date last Modified: See GitHub
///  Revision History: See GitHub
/// </summary>
public class VfxController : MonoBehaviour
{
    [SerializeField] private GlobalEnums.VfxType type_ = GlobalEnums.VfxType.HIT; //compares with hit obj, determines which pool to return to
    [SerializeField] private float despawn_time_ = 3f;
    private float timer_ = 0f;
    private ParticleSystem ps_;
    private VfxManager vfx_manager_;
    [SerializeField] private List<AudioClip> rand_sfx_ = new List<AudioClip>();
    private AudioSource audio_;

    private void Awake()
    {
        ps_ = GetComponent<ParticleSystem>();
        vfx_manager_ = GameObject.FindObjectOfType<VfxManager>();
        audio_ = GetComponent<AudioSource>();
    }

    public void DoSpawn(Vector3 pos)
    {
        PlayRandSfx();

        transform.position = pos;
        ps_.Play();
    }

    public void DoSpawn()
    {
        PlayRandSfx();

        ps_.Play();
    }

    private void PlayRandSfx()
    {
        //audio_.clip = rand_sfx_[Random.Range(0, rand_sfx_.Count)];
        //audio_.Play();

        audio_.PlayOneShot(rand_sfx_[Random.Range(0, rand_sfx_.Count)]);
    }

    private void OnParticleSystemStopped() //ParticleSystem StopAction needs to be set to "Callback"
    {
        vfx_manager_.ReturnVfx(this.gameObject, type_);
    }
}
