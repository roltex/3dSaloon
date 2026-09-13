using System;
using UnityEngine;

namespace TheLastBet.Saloon
{
    /// <summary>
    /// Player motor: camera-relative WASD, Shift sprint, Space jump, LMB shoot, R reload.
    /// Mouse look lives on SaloonCameraRig. Esc toggles cursor lock during play.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class SaloonWalkPreview : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4.2f;
        [SerializeField] private float sprintSpeed = 6.4f;
        [SerializeField] private float turnSpeed = 720f;
        [SerializeField] private float jumpSpeed = 4.5f;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private bool manageCursor;

        CharacterController _controller;
        CharacterAnimDriver _anim;
        HitscanWeapon _weapon;
        Health _health;
        float _vertical;
        float _groundStick = -0.8f;
        bool _jumpQueued;

        public float PlanarSpeed { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool MovementLocked { get; set; }
        public bool CombatEnabled { get; set; } = true;

        public event Action ShotAttempted;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _anim = GetComponent<CharacterAnimDriver>();
            _weapon = GetComponent<HitscanWeapon>();
            _health = GetComponent<Health>();
        }

        void OnEnable()
        {
            if (_health != null)
                _health.Died += OnDied;
        }

        void Start()
        {
            float scale = Mathf.Max(0.01f, transform.lossyScale.y);
            moveSpeed = 4.2f;
            sprintSpeed = 6.4f;
            jumpSpeed = 4.5f * Mathf.Lerp(1f, scale, 0.35f);
            _groundStick = -0.8f * scale;

            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            if (manageCursor)
                LockCursor(true);
        }

        void OnDisable()
        {
            if (_health != null)
                _health.Died -= OnDied;
            if (manageCursor)
                LockCursor(false);
        }

        public void SetCamera(Transform cam)
        {
            cameraTransform = cam;
        }

        public void SetCursorManaged(bool value)
        {
            manageCursor = value;
        }

        void Update()
        {
            if (_health != null && _health.IsDead)
                return;

            if (ToggleCursorPressed())
                LockCursor(Cursor.lockState != CursorLockMode.Locked);

            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            ReadMove(out float x, out float z, out bool sprint, out bool jump);
            if (MovementLocked)
            {
                x = 0f;
                z = 0f;
                sprint = false;
                jump = false;
            }

            Vector3 move = CameraRelativeMove(x, z);
            if (move.sqrMagnitude > 0.0001f)
            {
                Quaternion facing = Quaternion.LookRotation(move, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, turnSpeed * Time.deltaTime);
            }

            if (_controller.isGrounded)
            {
                _vertical = _groundStick;
                if (jump)
                {
                    _vertical = jumpSpeed;
                    _jumpQueued = true;
                }
            }
            else
            {
                _vertical += Physics.gravity.y * Time.deltaTime;
            }

            float speed = sprint ? sprintSpeed : moveSpeed;
            PlanarSpeed = move.magnitude * speed;
            IsSprinting = sprint && PlanarSpeed > 0.2f;
            Vector3 motion = move * speed;
            motion.y = _vertical;
            _controller.Move(motion * Time.deltaTime);

            float animSpeed = PlanarSpeed / Mathf.Max(0.01f, transform.lossyScale.y);
            _anim?.SetLocomotion(animSpeed, _controller.isGrounded);
            if (_jumpQueued)
            {
                _anim?.TriggerJump();
                _jumpQueued = false;
            }

            if (Cursor.lockState == CursorLockMode.Locked)
                HandleCombat();
        }

        void HandleCombat()
        {
            if (_weapon == null)
                return;

            if (ReloadPressed() && CombatEnabled)
                _weapon.Reload();

            if (!FirePressed())
                return;

            ShotAttempted?.Invoke();
            if (!CombatEnabled)
                return;

            Vector3 origin = cameraTransform != null ? cameraTransform.position : transform.position + Vector3.up * 1.4f;
            Vector3 direction = cameraTransform != null ? cameraTransform.forward : transform.forward;
            if (!_weapon.TryFire(origin, direction, gameObject))
                return;

            _anim?.TriggerShoot();
            Vector3 aim = direction;
            aim.y = 0f;
            if (aim.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(aim, Vector3.up);
        }

        void OnDied(Health health, GameObject source)
        {
            PlanarSpeed = 0f;
            IsSprinting = false;
            if (_controller != null)
                _controller.enabled = false;
            _anim?.SetDead();
            enabled = false;
        }

        Vector3 CameraRelativeMove(float x, float z)
        {
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;
            if (cameraTransform != null)
            {
                forward = cameraTransform.forward;
                right = cameraTransform.right;
            }

            forward.y = 0f;
            right.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
                forward = transform.forward;
            if (right.sqrMagnitude < 0.001f)
                right = transform.right;
            forward.Normalize();
            right.Normalize();

            Vector3 move = forward * z + right * x;
            if (move.sqrMagnitude > 1f)
                move.Normalize();
            return move;
        }

        static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        static void ReadMove(out float x, out float z, out bool sprint, out bool jump)
        {
            x = 0f;
            z = 0f;
            sprint = false;
            jump = false;
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed) x -= 1f;
                if (kb.dKey.isPressed) x += 1f;
                if (kb.sKey.isPressed) z -= 1f;
                if (kb.wKey.isPressed) z += 1f;
                sprint = kb.leftShiftKey.isPressed;
                jump = kb.spaceKey.wasPressedThisFrame;
            }

            var pad = UnityEngine.InputSystem.Gamepad.current;
            if (pad != null)
            {
                Vector2 stick = pad.leftStick.ReadValue();
                x += stick.x;
                z += stick.y;
                sprint = sprint || pad.leftStickButton.isPressed;
                jump = jump || pad.buttonSouth.wasPressedThisFrame;
            }
#else
            x = Input.GetAxisRaw("Horizontal");
            z = Input.GetAxisRaw("Vertical");
            sprint = Input.GetKey(KeyCode.LeftShift);
            jump = Input.GetKeyDown(KeyCode.Space);
#endif
        }

        static bool FirePressed()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            var pad = UnityEngine.InputSystem.Gamepad.current;
            return (mouse != null && mouse.leftButton.wasPressedThisFrame) ||
                   (pad != null && pad.rightTrigger.wasPressedThisFrame);
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        static bool ReloadPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            return kb != null && kb.rKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.R);
#endif
        }

        static bool ToggleCursorPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            return kb != null && kb.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
    }
}
