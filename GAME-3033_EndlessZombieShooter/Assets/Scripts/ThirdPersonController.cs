using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif
using Cinemachine;
using UnityEngine.Animations.Rigging;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace Player
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
#endif
	public class ThirdPersonController : MonoBehaviour, IDamageable<int>
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float move_speed = 2.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float sprint_speed = 5.335f;
		[Tooltip("How fast the character turns to face movement direction")]
		[Range(0.0f, 0.3f)]
		public float rotation_smooth_time = 0.12f;
		[Tooltip("Acceleration and deceleration")]
		public float speed_change_rate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float jump_height = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float player_gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float jump_cooldown = 0.50f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float fall_cooldown = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool is_grounded = true;
		[Tooltip("Useful for rough ground")]
		public float grounded_offset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float grounded_radius = 0.28f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask ground_layers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject cinemachine_cam_target;
		[Tooltip("How far in degrees can you move the camera up")]
		public float top_pitch_clamp = 70.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float bottom_pitch_clamp = -30.0f;
		[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
		public float cam_angle_override = 0.0f;
		[Tooltip("For locking the camera position on all axis")]
		public bool is_cam_pos_locked = false;
		[Tooltip("How sensitive the camera rotation should be when NOT aiming")]
		[SerializeField] private float look_sensitivity_ = 0.6f;
		[Tooltip("How sensitive the camera rotation should be when aiming")]
		[SerializeField] private float aim_sensitivity_ = 0.3f;

		[Header("Aiming")]
		[SerializeField] private CinemachineVirtualCamera aim_cam_;
		[SerializeField] private float shoot_cam_shake_;
		[SerializeField] private float shoot_cam_shake_time_;
		[SerializeField] private GameObject aim_crosshair_;
		[SerializeField] private LayerMask aim_collider_mask_ = new LayerMask();
		[SerializeField] private Transform debug_transform_;
		[SerializeField] private Transform bullet_spawn_pos_;
		[SerializeField] private Rig aim_rig_;

		[Header("Abilities")]
		[SerializeField] private CinemachineVirtualCamera zoom_cam_;
		[SerializeField] private float ultima_cam_shake_;
		[SerializeField] private float ultima_cam_shake_time_;
		[SerializeField] private float aoe_force_ = 100.0f; //33.5f
		[SerializeField] private float aoe_radius_ = 7.0f;

		[Header("Gameplay Stats")]
		[SerializeField] private int max_hp_ = 100;
		[SerializeField] private Transform root_pos_;
		[SerializeField] private int ammo_max_ = 130; //total possible ammo in mag + inventory
		[SerializeField] private int ammo_reserve_ = 100; //ammo outside mag
		[SerializeField] private int ammo_mag_ = 30; //size of mag
		[SerializeField] private int ammo_curr_ = 30; //curr ammo in mag
		[SerializeField] private int ultima_damage_ = 40;
		[SerializeField] private float ultima_cooldown_ = 0.175f; //only triggers after ultima is finished
		private float ultima_cooldown_delta_ = 0.0f;

		[Header("VFX SFX")]
		[SerializeField] private ParticleSystem jump_vfx1_;
		[SerializeField] private ParticleSystem jump_vfx2_;
		private Coroutine jump_vfx_coroutine_ = null;
		[SerializeField] private AudioClip jump_sfx_;
		[SerializeField] private ParticleSystem muzzle_flash_vfx_;
		[SerializeField] private ParticleSystem casing_vfx_;
		[SerializeField] private List<AudioClip> shoot_sfx_ = new List<AudioClip>();
		[SerializeField] private AudioClip empty_mag_sfx_;
		[SerializeField] private AudioClip reload_sfx_;
		[SerializeField] private List<AudioClip> damaged_sfx_ = new List<AudioClip>();
		private AudioSource audio_;

		[Header("UI")]
		[SerializeField] private TMP_Text ammo_txt_;
		[SerializeField] private TMP_Text killcount_txt_;
		[SerializeField] private Slider hp_slider_;
		[SerializeField] private UI_Inventory ui_inventory_;

		// cinemachine
		private float cinemachine_target_yaw_;
		private float cinemachine_target_pitch_;
		private float cam_sensitivity_;
		private CinemachineBasicMultiChannelPerlin cam_perlin_;
		private float cam_shake_timer_;
		private float cam_shake_timer_total_;
		private float cam_shake_start_intensity_;

		// player
		private float speed_;
		private float anim_blend_;
		private float target_rotation_ = 0.0f;
		private float rotation_velocity_;
		private float vertical_velocity_;
		private float terminal_velocity_ = 53.0f;
		private bool can_player_rotate_ = true;
		private bool can_double_jump_ = true;
		private UnityEngine.InputSystem.PlayerInput player_input_;

		// timeout deltatime
		private float jump_cooldown_delta_;
		private float fall_cooldown_delta_;

		// animation IDs
		private int anim_id_speed_;
		private int anim_id_grounded_;
		private int anim_id_jump_;
		private int anim_id_freefall_;
		private int anim_id_motion_speed_;
		private int anim_id_input_x_;
		private int anim_id_input_y_;
		private int anim_id_shoot_;
		private int anim_id_reload_;
		private int anim_id_ultima_;
		private int upper_body_layer_idx_ = 1;
		private int lower_body_layer_idx_ = 2;

		private float aim_rig_weight_;
		private bool has_animator_;

		private Animator animator_;
		private CharacterController controller_;
		private ActionMappingsInputs input_;
		private GameObject main_cam_;
		private BulletManager bullet_manager_;
		private VfxManager vfx_manager_;
		private GameManager game_manager_;

		private const float threshold_ = 0.01f;

		// gameplay
		private bool is_dead_ = false;
		private bool is_reload_ = false;
		private bool is_ultima_ = false;
		private int killcount_ = 0;
		private SaveFlag save_flag_;
		private Vector3 start_pos_ = Vector3.zero;

		// inventory
		private Inventory inventory_;

		private void Awake()
		{
			// get a reference to our main camera
			if (main_cam_ == null)
			{
				main_cam_ = GameObject.FindGameObjectWithTag("MainCamera");
			}
			aim_cam_.gameObject.SetActive(false);
			zoom_cam_.gameObject.SetActive(false);
			player_input_ = GetComponent<UnityEngine.InputSystem.PlayerInput>();
			bullet_manager_ = FindObjectOfType<BulletManager>();
			vfx_manager_ = FindObjectOfType<VfxManager>();
			audio_ = GetComponent<AudioSource>();
			game_manager_ = FindObjectOfType<GameManager>();

			start_pos_ = transform.position;

			Init(); //IDamageable method

			save_flag_ = FindObjectOfType<SaveFlag>();
            if (save_flag_ != null)
            {
				DoLoadSaveData();
			}

			ammo_curr_ = ammo_curr_ > ammo_mag_ ? ammo_mag_ : ammo_curr_;

			DoUpdateAmmoTxt();
			DoUpdateKillcountTxt();
		}

		private void Start()
		{
			has_animator_ = TryGetComponent(out animator_);
			controller_ = GetComponent<CharacterController>();
			input_ = GetComponent<ActionMappingsInputs>();

			// convert string to int for anim IDs
			AssignAnimationIDs();

			// SET ANIM EVENT AT RUNTIME
			int clip_idx = GlobalUtils.GetAnimClipIdxByName(animator_, "Reload");
			GlobalUtils.AddRuntimeAnimEvent(animator_, clip_idx, 2.3f, "DoEndReload", 0.0f);
			clip_idx = GlobalUtils.GetAnimClipIdxByName(animator_, "JumpAttack");
			GlobalUtils.AddRuntimeAnimEvent(animator_, clip_idx, 1.8f, "DoSpawnUltimaHit", 0.0f);
			GlobalUtils.AddRuntimeAnimEvent(animator_, clip_idx, 2.5f, "DoEndUltima", 0.0f);
			//GlobalUtils.AddRuntimeAnimEvent(animator_, clip_idx, 1.35f, "DoSpawnUltimaHit", 0.0f);
			//GlobalUtils.AddRuntimeAnimEvent(animator_, clip_idx, 1.875f, "DoEndUltima", 0.0f);

			// reset our timeouts on start
			jump_cooldown_delta_ = jump_cooldown;
			fall_cooldown_delta_ = fall_cooldown;

			inventory_ = new Inventory(DoUseItem);
			ui_inventory_.SetInventory(inventory_);
		}

		private void Update()
		{
			has_animator_ = TryGetComponent(out animator_);

			CheckInputPause();
			if (!is_ultima_)
			{
				CheckInputUltima();
				CheckInputAimAndShoot();
				CheckInputReload();
				CheckInputUseItem();
			}
			JumpAndGravity();
			GroundedCheck();
			Move();

			aim_rig_.weight = Mathf.Lerp(aim_rig_.weight, aim_rig_weight_, Time.deltaTime * 20f);

			// VFX
			CheckCamShake();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		/// <summary>
		/// Convert string to int for anim IDs
		/// </summary>
		private void AssignAnimationIDs()
		{
			anim_id_speed_ = Animator.StringToHash("Speed");
			anim_id_grounded_ = Animator.StringToHash("Grounded");
			anim_id_jump_ = Animator.StringToHash("Jump");
			anim_id_freefall_ = Animator.StringToHash("FreeFall");
			anim_id_motion_speed_ = Animator.StringToHash("MotionSpeed");
			anim_id_input_x_ = Animator.StringToHash("InputX");
			anim_id_input_y_ = Animator.StringToHash("InputY");
			anim_id_shoot_ = Animator.StringToHash("Shoot");
			anim_id_reload_ = Animator.StringToHash("Reload");
			anim_id_ultima_ = Animator.StringToHash("Ultima");
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - grounded_offset, transform.position.z);
			is_grounded = Physics.CheckSphere(spherePosition, grounded_radius, ground_layers, QueryTriggerInteraction.Ignore);

			// update animator if using character
			if (has_animator_)
			{
				animator_.SetBool(anim_id_grounded_, is_grounded);
			}
		}

		private void CameraRotation()
		{
			// if there is an input and camera position is not fixed
			if (input_.look.sqrMagnitude >= threshold_ && !is_cam_pos_locked)
			{
				cinemachine_target_yaw_ += input_.look.x * cam_sensitivity_ * Time.deltaTime;
				cinemachine_target_pitch_ += input_.look.y * cam_sensitivity_ * Time.deltaTime;
			}

			// clamp our rotations so our values are limited 360 degrees
			cinemachine_target_yaw_ = ClampAngle(cinemachine_target_yaw_, float.MinValue, float.MaxValue);
			cinemachine_target_pitch_ = ClampAngle(cinemachine_target_pitch_, bottom_pitch_clamp, top_pitch_clamp);

			// Cinemachine will follow this target
			cinemachine_cam_target.transform.rotation = Quaternion.Euler(cinemachine_target_pitch_ + cam_angle_override, cinemachine_target_yaw_, 0.0f);
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = input_.sprint ? sprint_speed : move_speed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (input_.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(controller_.velocity.x, 0.0f, controller_.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = input_.analogMovement ? input_.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				speed_ = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speed_change_rate);

				// round speed to 3 decimal places
				speed_ = Mathf.Round(speed_ * 1000f) / 1000f;
			}
			else
			{
				speed_ = targetSpeed;
			}
			anim_blend_ = Mathf.Lerp(anim_blend_, targetSpeed, Time.deltaTime * speed_change_rate);

			// normalise input direction
			Vector3 inputDirection = new Vector3(input_.move.x, 0.0f, input_.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (input_.move != Vector2.zero)
			{
				target_rotation_ = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + main_cam_.transform.eulerAngles.y;
				float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, target_rotation_, ref rotation_velocity_, rotation_smooth_time);

                // rotate to face input direction relative to camera position
                if (can_player_rotate_)
                {
					transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
				}
			}

			Vector3 targetDirection = Quaternion.Euler(0.0f, target_rotation_, 0.0f) * Vector3.forward;

			// move the player
			controller_.Move(targetDirection.normalized * (speed_ * Time.deltaTime) + new Vector3(0.0f, vertical_velocity_, 0.0f) * Time.deltaTime);

			// update animator if using character
			if (has_animator_)
			{
				animator_.SetFloat(anim_id_speed_, anim_blend_);
				animator_.SetFloat(anim_id_motion_speed_, inputMagnitude);
				animator_.SetFloat(anim_id_input_x_, inputDirection.x);
				animator_.SetFloat(anim_id_input_y_, inputDirection.z);
			}
		}

		private void JumpAndGravity()
		{
			if (is_grounded)
			{
				// reset the fall timeout timer
				fall_cooldown_delta_ = fall_cooldown;
				can_double_jump_ = true;

				// update animator if using character
				if (has_animator_)
				{
					animator_.SetBool(anim_id_jump_, false);
					animator_.SetBool(anim_id_freefall_, false);
				}

				// stop our velocity dropping infinitely when grounded
				if (vertical_velocity_ < 0.0f)
				{
					vertical_velocity_ = -2f;
				}

				// Jump
				if (input_.jump && jump_cooldown_delta_ <= 0.0f && !is_ultima_)
				{
					DoJump();
					input_.jump = false; //bug fix for audio playing twice
				}

				// jump timeout
				if (jump_cooldown_delta_ >= 0.0f)
				{
					jump_cooldown_delta_ -= Time.deltaTime;
				}
			}
			else if (can_double_jump_)
			{
				// Double Jump
				if (input_.jump && !is_ultima_)
				{
					// reset the fall timeout timer
					fall_cooldown_delta_ = fall_cooldown;
					can_double_jump_ = false;

					DoJump();
					input_.jump = false; //bug fix for audio playing twice
				}
			}
			else
			{
				// reset the jump timeout timer
				jump_cooldown_delta_ = jump_cooldown;

				// fall timeout
				if (fall_cooldown_delta_ >= 0.0f)
				{
					fall_cooldown_delta_ -= Time.deltaTime;
				}
				else
				{
					// update animator if using character
					if (has_animator_)
					{
						animator_.SetBool(anim_id_freefall_, true);
					}
				}

				// if we are not grounded, do not jump
				input_.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (vertical_velocity_ < terminal_velocity_)
			{
				vertical_velocity_ += player_gravity * Time.deltaTime;
			}
		}

		private void DoJump()
		{
			// the square root of H * -2 * G = how much velocity needed to reach desired height
			vertical_velocity_ = Mathf.Sqrt(jump_height * -2f * player_gravity);

			// update animator if using character
			if (has_animator_)
			{
				animator_.SetBool(anim_id_jump_, true);
				animator_.SetLayerWeight(lower_body_layer_idx_, 0f); //disable aiming lower body
			}

			// VFX - SFX
			if (jump_vfx_coroutine_ != null)
			{
				StopCoroutine(jump_vfx_coroutine_);
			}
			jump_vfx_coroutine_ = StartCoroutine(PlayJumpVfx());
			audio_.PlayOneShot(jump_sfx_); //SFX
		}

		private void CheckInputAimAndShoot()
		{
			// AIMING
			Vector3 mouse_world_pos = Vector3.zero;
			if (input_.is_aiming && !input_.sprint) //no aim while sprinting
			{
				// Create crosshair
				aim_cam_.gameObject.SetActive(true);
				aim_crosshair_.SetActive(true);
				cam_sensitivity_ = aim_sensitivity_;
				can_player_rotate_ = false;
				Vector2 screen_center_point = new Vector2(Screen.width / 2f, Screen.height / 2f);
				Ray ray = Camera.main.ScreenPointToRay(screen_center_point);
				if (Physics.Raycast(ray, out RaycastHit hit, 999f, aim_collider_mask_))
				{
					//debug_transform_.position = hit.point;
					//mouse_world_pos = hit.point;
				}
				//else 
				{
					debug_transform_.position = ray.GetPoint(10);
					mouse_world_pos = ray.GetPoint(10); //bug fix for when player's aim doesn't hit anything
				}

				// Lock player rotation
				Vector3 aim_target_world_pos = mouse_world_pos;
				aim_target_world_pos.y = transform.position.y;
				Vector3 aim_look_dir = (aim_target_world_pos - transform.position).normalized;
				transform.forward = Vector3.Lerp(transform.forward, aim_look_dir, Time.deltaTime * 20f);

				// Animation
				if (!is_reload_)
				{
					animator_.SetLayerWeight(upper_body_layer_idx_, Mathf.Lerp(animator_.GetLayerWeight(1), 1f, Time.deltaTime * 10f)); //upper body
				}
				if (is_grounded)
				{
					animator_.SetLayerWeight(lower_body_layer_idx_, Mathf.Lerp(animator_.GetLayerWeight(1), 1f, Time.deltaTime * 10f)); //lower body
				}
				aim_rig_weight_ = 1f;

				// SHOOTING
				if (input_.is_shooting)
				{
					if (ammo_curr_ > 0)
					{
						Vector3 aim_shoot_dir = (mouse_world_pos - bullet_spawn_pos_.position).normalized; //get dir from bullet_spawn_pos_ to crosshair
																										   //_bulletManager.GetBullet(bullet_spawn_pos_.position, Quaternion.LookRotation(aim_dir, Vector3.up), GlobalEnums.ObjType.PLAYER);
						bullet_manager_.GetBullet(bullet_spawn_pos_.position, aim_shoot_dir, GlobalEnums.ObjType.PLAYER);
						input_.is_shooting = false;

						// Animation
						animator_.SetTrigger(anim_id_shoot_);

						// VFX - SFX
						muzzle_flash_vfx_.Play();
						DoCamShake(aim_cam_, shoot_cam_shake_, shoot_cam_shake_time_);
						audio_.PlayOneShot(shoot_sfx_[Random.Range(0, shoot_sfx_.Count)]); //SFX
						casing_vfx_.Play();

						// Logic
						ammo_curr_--;

						// UI
						DoUpdateAmmoTxt();
					}
                    else if (ammo_reserve_ <= 0)
                    {
						audio_.PlayOneShot(empty_mag_sfx_); //SFX
						input_.is_shooting = false; //kill input to prevent shooting when not intending to
					}
					else if (!is_reload_)
					{
						DoReload();
						input_.is_shooting = false; //kill input to prevent shooting when not intending to
					}
				}
			}
			else
			{
				if (input_.is_shooting)
				{
					input_.is_shooting = false; //kill input to prevent shooting when not intending to
				}
				aim_cam_.gameObject.SetActive(false);
				aim_crosshair_.SetActive(false);
				cam_sensitivity_ = look_sensitivity_;
				can_player_rotate_ = true;

				// Animation
				if (!is_reload_)
				{
					animator_.SetLayerWeight(upper_body_layer_idx_, Mathf.Lerp(animator_.GetLayerWeight(1), 0f, Time.deltaTime * 10f)); //upper body
				}
				if (is_grounded)
				{
					animator_.SetLayerWeight(lower_body_layer_idx_, Mathf.Lerp(animator_.GetLayerWeight(1), 0f, Time.deltaTime * 10f)); //lower body
				}
				aim_rig_weight_ = 0f;
			}

			//animator_.SetLayerWeight(upper_body_layer_idx_, 1); //upper body //DEBUG
			//if (is_grounded)
			//{
			//    animator_.SetLayerWeight(lower_body_layer_idx_, 1); //lower body //DEBUG
			//}
			//aim_rig_weight_ = 1f; //DEBUG

			//aim_rig_.weight = Mathf.Lerp(aim_rig_.weight, aim_rig_weight_, Time.deltaTime * 20f);
		}

		private void CheckInputReload()
		{
			if (input_.is_reloading)
			{
				DoReload();
				input_.is_reloading = false;
			}
		}

		private void CheckInputUseItem()
		{
			if (input_.is_use_item)
			{
                if (inventory_.GetItemList().Count > 0)
                {
					DoUseItem(inventory_.GetItemList()[0]);
					audio_.PlayOneShot(reload_sfx_); //SFX
				}
				
				input_.is_use_item = false;
			}
		}

		private void CheckInputUltima()
		{
			if (is_reload_) { return; }

            if (ultima_cooldown_delta_ > 0)
            {
				input_.is_ultima = false;
				ultima_cooldown_delta_ -= Time.deltaTime;
            }

			if (input_.is_ultima)
			{
				Debug.Log("> Do Ultima");
				input_.is_aiming = false;
				is_ultima_ = true;
				ultima_cooldown_delta_ = ultima_cooldown_;
				//player_input_.actions.Disable(); //disabling input is too restrictive
				ResetAnimLayerAndRigWeight();

				// DoJump();
				// the square root of H * -2 * G = how much velocity needed to reach desired height
				vertical_velocity_ = Mathf.Sqrt(jump_height * -2.3f * player_gravity);

				animator_.SetTrigger(anim_id_ultima_);

				// VFX - SFX
				if (jump_vfx_coroutine_ != null)
				{
					StopCoroutine(jump_vfx_coroutine_);
				}
				jump_vfx_coroutine_ = StartCoroutine(PlayJumpVfx());
				audio_.PlayOneShot(jump_sfx_); //SFX

				input_.is_ultima = false;
				zoom_cam_.gameObject.SetActive(true);
			}
		}

		private void CheckInputPause()
		{
			if (input_.pause)
			{
				game_manager_.DoTogglePauseGame();
				input_.pause = false;
			}
		}

		public void SetPlayerInputEnabled(bool value)
        {
			player_input_.enabled = value;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		public Vector3 GetRootPos()
		{
			return root_pos_.position;
		}

		private void ResetAnimLayerAndRigWeight()
		{
			animator_.SetLayerWeight(upper_body_layer_idx_, 0f); //upper body
			animator_.SetLayerWeight(lower_body_layer_idx_, 0f); //lower body
			aim_rig_weight_ = 0f;
		}

		public void DoReload()
		{
			if (is_reload_ ||
				ammo_reserve_ <= 0 ||
				ammo_curr_ >= ammo_mag_)
			{
				return;
			}

			is_reload_ = true;
			animator_.SetTrigger(anim_id_reload_);
			//animator_.SetLayerWeight(upper_body_layer_idx_, Mathf.Lerp(animator_.GetLayerWeight(1), 1f, Time.deltaTime * 10f)); //upper body
			animator_.SetLayerWeight(upper_body_layer_idx_, 1); //upper body
			ammo_txt_.text = "Reloading...";

			audio_.PlayOneShot(reload_sfx_); //SFX
		}

		public void DoEndReload()
		{
			int ammo_to_load = (ammo_mag_ - ammo_curr_);
			int ammo_can_load = (ammo_reserve_ >= ammo_to_load) ? ammo_to_load : ammo_reserve_;
			ammo_reserve_ -= ammo_can_load; //100 - (30 - 2) =  72 in reserve
											//25 - 30 = -5
			ammo_curr_ += ammo_can_load;
			is_reload_ = false;

			DoUpdateAmmoTxt();
		}

		public void DoResetAmmoReserve()
        {
			ammo_reserve_ = 100;
			DoUpdateAmmoTxt();
		}

		public void DoUpdateAmmoTxt()
        {
			ammo_txt_.text = ammo_curr_.ToString() + "/" + ammo_reserve_.ToString();
		}

		public void DoUpdateKillcountTxt()
		{
			killcount_txt_.text = killcount_.ToString();
		}

		private void DoEndUltima()
		{
			//player_input_.actions.Enable();
			zoom_cam_.gameObject.SetActive(false);
			is_ultima_ = false;
		}

		private void DoSpawnUltimaHit()
		{
			//Time.timeScale = 0;
			CheckForAoEHits();

			vfx_manager_.GetVfx(GlobalEnums.VfxType.ULTIMA, transform.position, transform.forward);
			DoCamShake(zoom_cam_, ultima_cam_shake_, ultima_cam_shake_time_);
		}

		private void CheckForAoEHits()
		{
			Collider[] colliders = Physics.OverlapSphere(transform.position, aoe_radius_);
			foreach (Collider c in colliders)
			{
				EnemyController ec = c.GetComponent<EnemyController>();
				if (ec != null)
				{
					ec.DoLaunchedToAir(transform.position, aoe_force_, ultima_damage_);
				}
			}
		}

		public void DoCamShake(CinemachineVirtualCamera cam, float intensity, float time)
        {
			cam_perlin_ = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
			cam_perlin_.m_AmplitudeGain = intensity;

			cam_shake_start_intensity_ = intensity;
			cam_shake_timer_total_ = time;
			cam_shake_timer_ = time;
		}

		public void CheckCamShake()
        {
            if (cam_shake_timer_ > 0)
            {
				cam_shake_timer_ -= Time.deltaTime;
                if (cam_shake_timer_ <= 0f)
                {
                    if (cam_perlin_ != null)
                    {
						cam_perlin_.m_AmplitudeGain = 0f;
						Mathf.Lerp(cam_shake_start_intensity_, 0f, 1f - (cam_shake_timer_ / cam_shake_timer_total_));
					}
				}
			}
        }

		private IEnumerator PlayJumpVfx()
		{
			if (jump_vfx1_.isPlaying)
			{
				jump_vfx1_.Stop();
				jump_vfx1_.Play();
			}
			else
			{
				jump_vfx1_.Play();
			}

			if (jump_vfx2_.isPlaying)
			{
				jump_vfx2_.Stop();
				jump_vfx2_.Play();
			}
			else
			{
				jump_vfx2_.Play();
			}

			yield return new WaitForSeconds(0.59f);
			jump_vfx1_.Stop();
			jump_vfx2_.Stop();
		}

		public void DoAddItem(Item item)
        {
			inventory_.AddItem(item);
        }

		public void DoUseItem(Item item)
        {
            switch (item.item_type)
            {
                case Item.ItemType.NONE:
                    break;
                case Item.ItemType.POTION:
					HealDamage(20);
					inventory_.RemoveItem(item);
					break;
                case Item.ItemType.AMMO:
					ammo_reserve_ = 100;
					DoUpdateAmmoTxt();
					inventory_.RemoveItem(item);
					break;
                case Item.ItemType.MISSILE:
                    break;
                default:
                    break;
            }
        }

		public void IncrementKillcount()
        {
			killcount_++;
			DoUpdateKillcountTxt();
        }

		public void DoSaveData()
        {
			PlayerPrefs.SetFloat("PlayerPosX", transform.position.x);
			PlayerPrefs.SetFloat("PlayerPosY", transform.position.y);
			PlayerPrefs.SetFloat("PlayerPosZ", transform.position.z);
			PlayerPrefs.SetInt("PlayerHP", health);
			PlayerPrefs.SetInt("PlayerAmmoCurr", ammo_curr_);
			PlayerPrefs.SetInt("PlayerAmmoReserve", ammo_reserve_);
			PlayerPrefs.SetInt("PlayerKillcount", killcount_);
        }

		public void DoLoadSaveData()
		{
			float x = PlayerPrefs.GetFloat("PlayerPosX", start_pos_.x);
			float y = PlayerPrefs.GetFloat("PlayerPosY", start_pos_.y);
			float z = PlayerPrefs.GetFloat("PlayerPosZ", start_pos_.z);
			health = PlayerPrefs.GetInt("PlayerHP", max_hp_);
			ammo_curr_ = PlayerPrefs.GetInt("PlayerAmmoCurr", ammo_curr_);
			ammo_reserve_ = PlayerPrefs.GetInt("PlayerAmmoReserve", ammo_reserve_);
			killcount_ = PlayerPrefs.GetInt("PlayerKillcount", killcount_);

			var player_controller = GetComponent<CharacterController>();
			player_controller.enabled = false;
			transform.position = new Vector3(x,y+1,z);
			player_controller.enabled = true;
		}

		/// <summary>
		/// IDamageable methods
		/// </summary>
		public void Init() //Link hp to class hp
		{
			health = max_hp_;
			obj_type = GlobalEnums.ObjType.PLAYER;
		}
		public int health { get; set; } //Health points
		public GlobalEnums.ObjType obj_type { get; set; } //Type of gameobject

		public void ApplyDamage(int damage_value, GlobalEnums.FlinchType flinch_mode = GlobalEnums.FlinchType.NO_FLINCH) //Deals damage to this object
		{
			//StartCoroutine(cam_controller_.DoShake(0.15f, 0.4f));
			health -= damage_value;
			health = health < 0 ? 0 : health; //Clamps health so it doesn't go below 0
											  //game_manager_.SetUIHPBarValue((float)health / (float)hp_); //Updates UI
											  //flash_vfx_.DoFlash();
			audio_.PlayOneShot(damaged_sfx_[Random.Range(0, damaged_sfx_.Count)]); //SFX
			hp_slider_.value = ((float)health / (float)max_hp_); //Updates UI

			if (health == 0)
			{
				is_dead_ = true;
				//explode_manager_.GetObj(this.transform.position, obj_type);
				//gameObject.SetActive(false);
				game_manager_.DoGameOver();
			}
			Debug.Log(">>> Player HP is " + health.ToString());
		}
		public void HealDamage(int heal_value) //Adds health to object
		{
			if (health == max_hp_) //If full HP, IncrementScore
			{
				//game_manager_.IncrementScore(heal_value);
				//audio_source_.PlayOneShot(food_score_sfx_);
			}
			else
			{
				health += heal_value;
				health = health > max_hp_ ? max_hp_ : health; //Clamps health so it doesn't exceed hp_
															  //game_manager_.SetUIHPBarValue((float)health / (float)hp_); //Updates UI
				hp_slider_.value = ((float)health / (float)max_hp_); //Updates UI
			}
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (is_grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;
			
			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - grounded_offset, transform.position.z), grounded_radius);

			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, aoe_radius_);
		}
    }
}