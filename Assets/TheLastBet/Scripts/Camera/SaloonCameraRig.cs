using UnityEngine;

namespace TheLastBet.Saloon
{
    /// <summary>
    /// GTA-style third-person follow camera. Station CameraAnchors remain available
    /// for cinematic blends; follow resumes afterward or when requested.
    /// </summary>
    public sealed class SaloonCameraRig : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Transform followTarget;
        [SerializeField] private bool followPlayer = true;

        [Header("Follow")]
        [SerializeField] private float distance = 4.7f;
        [SerializeField] private float minDistance = 1.15f;
        [SerializeField] private float height = 1.62f;
        [SerializeField] private float lookHeight = 1.52f;
        [SerializeField] private float shoulderOffset = 0.42f;
        [SerializeField] private float followDamping = 9f;
        [SerializeField] private float collisionRadius = 0.22f;
        [SerializeField] private LayerMask collisionMask = ~0;

        [Header("Look")]
        [SerializeField] private bool invertY = false;
        [SerializeField] private float mouseSensitivity = 0.16f;
        [SerializeField] private float yawSpeed = 210f;
        [SerializeField] private float pitchSpeed = 165f;
        [SerializeField] private float minPitch = -18f;
        [SerializeField] private float maxPitch = 48f;
        [SerializeField] private float lookSmoothing = 22f;

        [Header("Cinematic")]
        [SerializeField] private bool cinematicFeel = true;
        [SerializeField] private float lookAhead = 0.7f;
        [SerializeField] private float lookAheadDamping = 5f;
        [SerializeField] private float idleFov = 40f;
        [SerializeField] private float moveFov = 44f;
        [SerializeField] private float sprintFov = 48f;
        [SerializeField] private float fovDamping = 3.5f;
        [SerializeField] private float swayAmount = 0.045f;
        [SerializeField] private float swaySpeed = 0.55f;

        [Header("Cinematic anchors")]
        [SerializeField] private CameraAnchor initialAnchor;
        [SerializeField] private CameraAnchor[] anchors;

        CameraAnchor _cinematicTarget;
        Vector3 _fromPos;
        Quaternion _fromRot;
        float _fromFov;
        float _blend;
        float _blendDuration = 0.65f;
        float _yaw;
        float _pitch = 10f;
        float _yawCurrent;
        float _pitchCurrent;
        Vector3 _smoothedPosition;
        Vector3 _lookAheadOffset;
        Vector3 _lastTargetPos;
        bool _hasSmooth;
        bool _hasTargetSample;
        float _collisionDistance = -1f;
        bool _firstPersonDuel;
        Transform _fpEyes;
        Transform _fpLook;
        RpgCameraBinder _rpg;

        public Camera GameplayCamera => gameplayCamera;
        public Transform FollowTarget => followTarget;
        public bool IsFollowing => followPlayer && _cinematicTarget == null;

        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
            _hasSmooth = false;
            _hasTargetSample = false;
            _collisionDistance = -1f;
            ApplyTargetScale(target);
            BindRpg(target);
            if (target == null)
                return;

            Vector3 planar = target.forward;
            planar.y = 0f;
            if (planar.sqrMagnitude > 0.001f)
                _yaw = _yawCurrent = Quaternion.LookRotation(planar).eulerAngles.y;
        }

        void BindRpg(Transform target)
        {
            if (target == null)
            {
                SetRpgRoaming(false);
                _rpg = null;
                return;
            }

            if (gameplayCamera == null)
                gameplayCamera = GetComponentInChildren<Camera>();
            _rpg = RpgCameraBinder.Bind(target.gameObject, gameplayCamera);
            followPlayer = false;
        }

        void SetRpgRoaming(bool roaming)
        {
            if (_rpg != null)
                _rpg.SetRoaming(roaming);
        }

        void ApplyTargetScale(Transform target)
        {
            float s = CharacterScale(target);
            distance = 4.7f * Mathf.Lerp(1f, s, 0.85f);
            height = 1.62f * s;
            lookHeight = 1.52f * s;
            shoulderOffset = 0.42f * s;
        }

        static float CharacterScale(Transform target)
        {
            if (target == null)
                return 1f;
            var controller = target.GetComponent<CharacterController>();
            if (controller != null)
                return (controller.height * target.lossyScale.y) / 1.8f;
            return target.lossyScale.y;
        }

        public void ResumeFollow()
        {
            _firstPersonDuel = false;
            _fpEyes = null;
            _fpLook = null;
            _cinematicTarget = null;
            followPlayer = false;
            _hasSmooth = false;
            _hasTargetSample = false;
            if (_rpg == null && followTarget != null)
                BindRpg(followTarget);
            SetRpgRoaming(true);
        }

        public void SetFirstPersonDuel(Transform eyes, Transform lookTarget)
        {
            _firstPersonDuel = true;
            _fpEyes = eyes;
            _fpLook = lookTarget;
            followPlayer = false;
            _cinematicTarget = null;
            SetRpgRoaming(false);
            ApplyFirstPerson();
        }

        public void SetOrbit(float yaw, float pitch)
        {
            _yaw = _yawCurrent = yaw;
            _pitch = _pitchCurrent = Mathf.Clamp(pitch, minPitch, maxPitch);
            _hasSmooth = false;
            _collisionDistance = -1f;
            if (followTarget != null)
            {
                var rpg = followTarget.GetComponent<JohnStairs.RPG.Character.Cam.RPGCameraLite>();
                if (rpg != null)
                {
                    rpg.StartYaw = yaw;
                    rpg.StartPitch = pitch;
                    rpg.StartYawRelativeToCharacterRotation = false;
                    rpg.SetYaw(yaw);
                    rpg.SetPitch(pitch);
                }
            }
        }

        public void HoldScriptedView(Vector3 position, Quaternion rotation, float fov)
        {
            _firstPersonDuel = false;
            _fpEyes = null;
            _fpLook = null;
            followPlayer = false;
            _cinematicTarget = null;
            SetRpgRoaming(false);
            if (gameplayCamera == null)
                gameplayCamera = GetComponentInChildren<Camera>();
            if (gameplayCamera == null)
                return;
            gameplayCamera.transform.SetPositionAndRotation(position, rotation);
            gameplayCamera.fieldOfView = fov;
        }

        void Start()
        {
            if (gameplayCamera == null)
                gameplayCamera = GetComponentInChildren<Camera>();

            int playerLayer = CombatLayers.Player;
            if (playerLayer >= 0)
                collisionMask &= ~(1 << playerLayer);

            if (followTarget != null)
            {
                Vector3 planar = followTarget.forward;
                planar.y = 0f;
                if (planar.sqrMagnitude > 0.001f)
                    _yaw = Quaternion.LookRotation(planar).eulerAngles.y;
            }

            _yawCurrent = _yaw;
            _pitchCurrent = _pitch;

            if (followPlayer && followTarget != null)
                SnapFollow();
        }

        void Update()
        {
            if (KeyboardDigit() is not int index)
                return;

            if (index == 0)
            {
                ResumeFollow();
                return;
            }

            if (anchors == null || index < 0 || index >= anchors.Length || anchors[index] == null)
                return;

            BlendTo(anchors[index]);
        }

        void LateUpdate()
        {
            if (gameplayCamera == null)
                return;

            if (_firstPersonDuel)
            {
                ApplyFirstPerson();
                return;
            }

            if (_cinematicTarget != null)
            {
                ApplyCinematic();
                return;
            }

            if (followPlayer)
                ApplyFollow();
        }

        void ApplyFirstPerson()
        {
            if (_fpEyes == null)
                return;

            float s = CharacterScale(_fpEyes);
            Vector3 pos = _fpEyes.position + Vector3.up * (1.62f * s);
            Vector3 look = _fpLook != null
                ? _fpLook.position + Vector3.up * (1.78f * s)
                : pos + _fpEyes.forward;
            Vector3 dir = look - pos;
            if (dir.sqrMagnitude < 0.0001f)
                dir = _fpEyes.forward;
            gameplayCamera.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(dir, Vector3.up));
            gameplayCamera.fieldOfView = 58f;
        }

        public void BlendTo(CameraAnchor anchor)
        {
            if (anchor == null || gameplayCamera == null)
                return;

            SetRpgRoaming(false);
            _firstPersonDuel = false;
            _fromPos = gameplayCamera.transform.position;
            _fromRot = gameplayCamera.transform.rotation;
            _fromFov = gameplayCamera.fieldOfView;
            _cinematicTarget = anchor;
            _blend = 0f;
            _blendDuration = Mathf.Max(0.05f, anchor.BlendDuration);
        }

        public void SnapTo(CameraAnchor anchor)
        {
            if (anchor == null || gameplayCamera == null)
                return;

            _cinematicTarget = anchor;
            _blend = 1f;
            _blendDuration = 0.01f;
            gameplayCamera.transform.SetPositionAndRotation(anchor.transform.position, anchor.transform.rotation);
            gameplayCamera.fieldOfView = anchor.FieldOfView;
        }

        void ApplyCinematic()
        {
            float t = _blendDuration <= 0.001f ? 1f : Mathf.Clamp01(_blend / _blendDuration);
            t = t * t * (3f - 2f * t);
            Transform cam = gameplayCamera.transform;
            cam.position = Vector3.Lerp(_fromPos, _cinematicTarget.transform.position, t);
            cam.rotation = Quaternion.Slerp(_fromRot, _cinematicTarget.transform.rotation, t);
            gameplayCamera.fieldOfView = Mathf.Lerp(_fromFov, _cinematicTarget.FieldOfView, t);
            if (t < 1f)
                _blend += Time.deltaTime;
        }

        void ApplyFollow()
        {
            if (followTarget == null)
                return;

            ReadLook(out float mouseX, out float mouseY, out float stickX, out float stickY);
            float ySign = invertY ? 1f : -1f;
            _yaw += mouseX * mouseSensitivity + stickX * yawSpeed * Time.deltaTime;
            _pitch = Mathf.Clamp(
                _pitch + (mouseY * mouseSensitivity + stickY * pitchSpeed * Time.deltaTime) * ySign,
                minPitch,
                maxPitch);

            float lookK = 1f - Mathf.Exp(-lookSmoothing * Time.deltaTime);
            _yawCurrent = Mathf.LerpAngle(_yawCurrent, _yaw, lookK);
            _pitchCurrent = Mathf.Lerp(_pitchCurrent, _pitch, lookK);

            Vector3 pivot = followTarget.position + Vector3.up * lookHeight;
            if (cinematicFeel)
                pivot += UpdateLookAhead();

            Quaternion orbit = Quaternion.Euler(_pitchCurrent, _yawCurrent, 0f);
            Vector3 localOffset = new Vector3(shoulderOffset, 0f, -distance);
            Vector3 desired = pivot + orbit * localOffset + Vector3.up * (height - lookHeight);
            if (cinematicFeel)
                desired += orbit * UpdateSway();

            desired = ResolveCollision(pivot, desired);

            if (!_hasSmooth)
            {
                _smoothedPosition = desired;
                _hasSmooth = true;
            }
            else
            {
                float k = 1f - Mathf.Exp(-followDamping * Time.deltaTime);
                _smoothedPosition = Vector3.Lerp(_smoothedPosition, desired, k);
            }

            gameplayCamera.transform.position = _smoothedPosition;
            gameplayCamera.transform.rotation = Quaternion.LookRotation(pivot - _smoothedPosition, Vector3.up);
            if (cinematicFeel)
                ApplyCinematicFov();
        }

        Vector3 UpdateLookAhead()
        {
            Vector3 velocity = Vector3.zero;
            if (_hasTargetSample && Time.deltaTime > 0.0001f)
                velocity = (followTarget.position - _lastTargetPos) / Time.deltaTime;
            _lastTargetPos = followTarget.position;
            _hasTargetSample = true;

            velocity.y = 0f;
            Vector3 desired = Vector3.ClampMagnitude(velocity, 4.5f) * (lookAhead * 0.12f);
            float k = 1f - Mathf.Exp(-lookAheadDamping * Time.deltaTime);
            _lookAheadOffset = Vector3.Lerp(_lookAheadOffset, desired, k);
            return _lookAheadOffset;
        }

        Vector3 UpdateSway()
        {
            float t = Time.unscaledTime * swaySpeed;
            return new Vector3(Mathf.Sin(t * 0.81f) * swayAmount, Mathf.Sin(t) * swayAmount * 0.45f, 0f);
        }

        void ApplyCinematicFov()
        {
            float targetFov = idleFov;
            var walker = followTarget.GetComponent<SaloonWalkPreview>();
            if (walker != null)
            {
                float scale = Mathf.Max(0.01f, CharacterScale(followTarget));
                float speed = walker.PlanarSpeed;
                if (walker.IsSprinting && speed > 0.4f * scale)
                    targetFov = sprintFov;
                else if (speed > 0.2f * scale)
                    targetFov = Mathf.Lerp(idleFov, moveFov, Mathf.InverseLerp(0.2f * scale, 4.2f * scale, speed));
            }

            float k = 1f - Mathf.Exp(-fovDamping * Time.deltaTime);
            gameplayCamera.fieldOfView = Mathf.Lerp(gameplayCamera.fieldOfView, targetFov, k);
        }

        void SnapFollow()
        {
            if (gameplayCamera == null || followTarget == null)
                return;

            _yawCurrent = _yaw;
            _pitchCurrent = _pitch;
            Vector3 pivot = followTarget.position + Vector3.up * lookHeight;
            Quaternion orbit = Quaternion.Euler(_pitchCurrent, _yawCurrent, 0f);
            Vector3 desired = pivot + orbit * new Vector3(shoulderOffset, 0f, -distance);
            gameplayCamera.transform.position = ResolveCollision(pivot, desired);
            gameplayCamera.transform.rotation = Quaternion.LookRotation(pivot - gameplayCamera.transform.position, Vector3.up);
            gameplayCamera.fieldOfView = idleFov;
            _smoothedPosition = gameplayCamera.transform.position;
            _hasSmooth = true;
            _lastTargetPos = followTarget.position;
            _hasTargetSample = true;
        }

        Vector3 ResolveCollision(Vector3 pivot, Vector3 desired)
        {
            Vector3 toCam = desired - pivot;
            float dist = toCam.magnitude;
            if (dist < 0.01f)
                return desired;

            Vector3 dir = toCam / dist;
            RaycastHit[] hits = Physics.SphereCastAll(pivot, collisionRadius, dir, dist, collisionMask, QueryTriggerInteraction.Ignore);
            float nearest = dist;
            bool blocked = false;
            for (int i = 0; i < hits.Length; i++)
            {
                Transform hitTransform = hits[i].transform;
                if (followTarget != null && (hitTransform == followTarget || hitTransform.IsChildOf(followTarget)))
                    continue;
                if (hits[i].distance < nearest)
                {
                    nearest = hits[i].distance;
                    blocked = true;
                }
            }

            float targetDist = blocked ? Mathf.Max(minDistance, nearest - collisionRadius) : dist;
            if (_collisionDistance < 0f)
                _collisionDistance = targetDist;
            else
            {
                float recover = blocked ? 18f : 7f;
                float k = 1f - Mathf.Exp(-recover * Time.deltaTime);
                _collisionDistance = Mathf.Lerp(_collisionDistance, targetDist, k);
            }

            return pivot + dir * _collisionDistance;
        }

        static void ReadLook(out float mouseX, out float mouseY, out float stickX, out float stickY)
        {
            mouseX = 0f;
            mouseY = 0f;
            stickX = 0f;
            stickY = 0f;
#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null && (mouse.rightButton.isPressed || Cursor.lockState == CursorLockMode.Locked))
            {
                Vector2 delta = mouse.delta.ReadValue();
                mouseX = delta.x;
                mouseY = delta.y;
            }

            var pad = UnityEngine.InputSystem.Gamepad.current;
            if (pad != null)
            {
                Vector2 stick = pad.rightStick.ReadValue();
                stickX = stick.x;
                stickY = stick.y;
            }
#else
            if (Input.GetMouseButton(1) || Cursor.lockState == CursorLockMode.Locked)
            {
                mouseX = Input.GetAxis("Mouse X") * 12f;
                mouseY = Input.GetAxis("Mouse Y") * 12f;
            }

            stickX = Input.GetAxis("Look X");
            stickY = Input.GetAxis("Look Y");
#endif
        }

        static int? KeyboardDigit()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null)
                return null;
            if (kb.digit0Key.wasPressedThisFrame) return 0;
            if (kb.digit1Key.wasPressedThisFrame) return 1;
            if (kb.digit2Key.wasPressedThisFrame) return 2;
            if (kb.digit3Key.wasPressedThisFrame) return 3;
            if (kb.digit4Key.wasPressedThisFrame) return 4;
            if (kb.digit5Key.wasPressedThisFrame) return 5;
            if (kb.digit6Key.wasPressedThisFrame) return 6;
            if (kb.digit7Key.wasPressedThisFrame) return 7;
            if (kb.digit8Key.wasPressedThisFrame) return 8;
            if (kb.digit9Key.wasPressedThisFrame) return 9;
            return null;
#else
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                    return i;
            }

            return null;
#endif
        }
    }
}
