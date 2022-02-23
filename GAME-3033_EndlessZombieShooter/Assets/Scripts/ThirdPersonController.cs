using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif
using Cinemachine;
using UnityEngine.Animations.Rigging;
using TMPro;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace Player
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
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
		[SerializeField] private GameObject aim_crosshair_;
		[SerializeField] private LayerMask aim_collider_mask_ = new LayerMask();
		[SerializeField] private Transform debug_transform_;
		[SerializeField] private Transform bullet_spawn_pos_;
		[SerializeField] private Rig aim_rig_;

		[Header("Gameplay Stats")]
		[SerializeField] private int hp_ = 100;
		[SerializeField] private Transform root_pos_;
		[SerializeField] private int ammo_max_ = 130; //total possible ammo in mag + inventory
		[SerializeField] private int ammo_reserve_ = 100; //ammo outside mag
		[SerializeField] private int ammo_mag_ = 30; //size of mag
		[SerializeField] private int ammo_curr_ = 30; //curr ammo in mag

		[Header("VFX_SFX")]
		[SerializeField] ParticleSystem muzzle_flash_vfx_;

		[Header("UI")]
		[SerializeField] TMP_Text ammo_txt_;

		// cinemachine
		private float cinemachine_target_yaw_;
		private float cinemachine_target_pitch_;
		private float cam_sensitivity_;

		// player
		private float speed_;
		private float anim_blend_;
		private float target_rotation_ = 0.0f;
		private float rotation_velocity_;
		private float vertical_velocity_;
		private float terminal_velocity_ = 53.0f;
		private bool can_player_rotate_ = true;

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
		private int clip_idx_reload_;

		private Animator animator_;
		private CharacterController controller_;
		private ActionMappingsInputs input_;
		private GameObject main_cam_;
		private BulletManager bullet_manager_;

		private float aim_rig_weight_;

		private const float threshold_ = 0.01f;

		private bool has_animator_;

		// gameplay
		private bool is_dead_ = false;
		private bool is_reload_ = false;

		private void Awake()
		{
			// get a reference to our main camera
			if (main_cam_ == null)
			{
				main_cam_ = GameObject.FindGameObjectWithTag("MainCamera");
			}
			bullet_manager_ = FindObjectOfType<BulletManager>();

			ammo_curr_ = ammo_curr_ > ammo_mag_ ? ammo_mag_ : ammo_curr_;

			Init(); //IDamageable method

			DoUpdateAmmoTxt();
		}

		private void Start()
		{
			has_animator_ = TryGetComponent(out animator_);
			controller_ = GetComponent<CharacterController>();
			input_ = GetComponent<ActionMappingsInputs>();

			// convert string to int for anim IDs
			AssignAnimationIDs();

			// reset our timeouts on start
			jump_cooldown_delta_ = jump_cooldown;
			fall_cooldown_delta_ = fall_cooldown;

			clip_idx_reload_ = GetAnimClipIdxByName(animator_, "Reload");
			AddRuntimeAnimEvent(animator_, clip_idx_reload_, 2.3f, "DoEndReload", 0.0f);
		}

		private void Update()
		{
			has_animator_ = TryGetComponent(out animator_);
			
			JumpAndGravity();
			GroundedCheck();
			Move();

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
					animator_.SetLayerWeight(1, Mathf.Lerp(animator_.GetLayerWeight(1), 1f, Time.deltaTime * 10f)); //upper body
				}
                if (is_grounded)
                {
					animator_.SetLayerWeight(2, Mathf.Lerp(animator_.GetLayerWeight(1), 1f, Time.deltaTime * 10f)); //lower body
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

						// VFX
						muzzle_flash_vfx_.Play();
						
						// Logic
						ammo_curr_--;

						// UI
						DoUpdateAmmoTxt();
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
					animator_.SetLayerWeight(1, Mathf.Lerp(animator_.GetLayerWeight(1), 0f, Time.deltaTime * 10f)); //upper body
				}
				if (is_grounded)
				{
					animator_.SetLayerWeight(2, Mathf.Lerp(animator_.GetLayerWeight(1), 0f, Time.deltaTime * 10f)); //lower body
				}
				aim_rig_weight_ = 0f;
			}

			//animator_.SetLayerWeight(1, Mathf.Lerp(animator_.GetLayerWeight(1), 1f, Time.deltaTime * 10f)); //upper body //DEBUG
			//if (is_grounded)
			//{
			//    animator_.SetLayerWeight(2, Mathf.Lerp(animator_.GetLayerWeight(1), 1f, Time.deltaTime * 10f)); //lower body //DEBUG
			//}
			//aim_rig_weight_ = 1f; //DEBUG

			aim_rig_.weight = Mathf.Lerp(aim_rig_.weight, aim_rig_weight_, Time.deltaTime * 20f);
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
				if (input_.jump && jump_cooldown_delta_ <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					vertical_velocity_ = Mathf.Sqrt(jump_height * -2f * player_gravity);

					// update animator if using character
					if (has_animator_)
					{
						animator_.SetBool(anim_id_jump_, true);
						animator_.SetLayerWeight(2, 0f); //disable aiming lower body
					}
				}

				// jump timeout
				if (jump_cooldown_delta_ >= 0.0f)
				{
					jump_cooldown_delta_ -= Time.deltaTime;
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

		public void DoReload()
        {
            if (ammo_reserve_ > 0)
            {
				is_reload_ = true;
				animator_.SetTrigger(anim_id_reload_);
				animator_.SetLayerWeight(1, Mathf.Lerp(animator_.GetLayerWeight(1), 1f, Time.deltaTime * 10f)); //upper body
				ammo_txt_.text = "Reloading...";
			}
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

		public void DoUpdateAmmoTxt()
        {
			ammo_txt_.text = ammo_curr_.ToString() + "/" + ammo_reserve_.ToString();
		}

		/// <summary>
		/// IDamageable methods
		/// </summary>
		public void Init() //Link hp to class hp
		{
			health = hp_;
			obj_type = GlobalEnums.ObjType.PLAYER;
		}
		public int health { get; set; } //Health points
		public GlobalEnums.ObjType obj_type { get; set; } //Type of gameobject
		public void ApplyDamage(int damage_value) //Deals damage to this object
		{
			//StartCoroutine(cam_controller_.DoShake(0.15f, 0.4f));
			health -= damage_value;
			health = health < 0 ? 0 : health; //Clamps health so it doesn't go below 0
											  //game_manager_.SetUIHPBarValue((float)health / (float)hp_); //Updates UI
											  //flash_vfx_.DoFlash();
											  //audio_source_.PlayOneShot(damaged_sfx_);
			if (health == 0)
			{
				is_dead_ = true;
				//explode_manager_.GetObj(this.transform.position, obj_type);
				gameObject.SetActive(false);
			}
			Debug.Log(">>> Player HP is " + health.ToString());

			//OnHealthChanged.Invoke(health);
		}
		public void HealDamage(int heal_value) //Adds health to object
		{
			if (health == hp_) //If full HP, IncrementScore
			{
				//game_manager_.IncrementScore(heal_value);
				//audio_source_.PlayOneShot(food_score_sfx_);
			}
			else
			{
				health += heal_value;
				health = health > hp_ ? hp_ : health; //Clamps health so it doesn't exceed hp_
													  //game_manager_.SetUIHPBarValue((float)health / (float)hp_); //Updates UI
			}
			//OnHealthChanged.Invoke(health);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (is_grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;
			
			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - grounded_offset, transform.position.z), grounded_radius);
		}

		public int GetAnimClipIdxByName(Animator anim, string name)
        {
			AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
			for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].name == name)
                {
					return i;
                }
            }
			return -1;
        }

        public void AddRuntimeAnimEvent(Animator anim, int clip_idx, float time, string functionName, float floatParameter)
        {
            AnimationEvent animationEvent = new AnimationEvent();
            animationEvent.functionName = functionName;
            animationEvent.floatParameter = floatParameter;
            animationEvent.time = time;
            AnimationClip clip = anim.runtimeAnimatorController.animationClips[clip_idx];
            clip.AddEvent(animationEvent);
        }
    }
}