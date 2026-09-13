using System.Collections;
using System.Collections.Generic;
using JohnStairs.RPG.Character.Cam;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JohnStairs.RPG.Character.Controller {
    [RequireComponent(typeof(PlayerInput))]
    public class RPGController : MonoBehaviour {
        /// <summary>
        /// Enum for controlling if the camera should rotate together with the character
        /// </summary>
        public enum RotationSync {
            /// <summary>
            /// Never rotate with the character
            /// </summary>
            Never,
            /// <summary>
            /// The rotation stops while the camera is orbiting around the character
            /// </summary>
            PauseOnOrbiting,
            /// <summary>
            /// Always rotate together with the character
            /// </summary>
            Always
        }

        /// <summary>
        /// Inverts the horizontal orbit axis
        /// </summary>
        [Tooltip("Inverts the horizontal orbit axis.")]
        public bool InvertYawAxis = true;
        /// <summary>
        /// Inverts the vertical orbit axis
        /// </summary>
        [Tooltip("Inverts the vertical orbit axis.")]
        public bool InvertPitchAxis = true;
        /// <summary>
        /// The sensitivity of orbiting the camera on the horizontal axis
        /// </summary>
        [Tooltip("The sensitivity of orbiting the camera on the horizontal axis.")]
        public float YawSensitivity = 0.25f;
        /// <summary>
        /// The sensitivity of orbiting the camera on the vertical axis
        /// </summary>
        [Tooltip("The sensitivity of orbiting the camera on the vertical axis.")]
        public float PitchSensitivity = 0.25f;
        /// <summary>
        /// The sensitivity of the zooming input
        /// </summary>
        [Tooltip("The sensitivity of the zooming input.")]
        public float ZoomSensitivity = 0.01f;
        /// <summary>
        /// Controls if the camera should rotate together with the character
        /// </summary>
        public RotationSync SyncRotations = RotationSync.PauseOnOrbiting;
        /// <summary>
        /// If true, the camera will align with the character's forward direction when it is moving
        /// </summary>
        public bool AlignCameraOnCharacterMovement = true;
        /// <summary>
        /// The number of shakes per second while the camera is shaking
        /// </summary>
        [Tooltip("The number of shakes per second.")]
        public float ShakeFrequency = 5.0f;
        /// <summary>
        /// Maximum rotation change in degrees while the camera is shaking 
        /// </summary>
        [Tooltip("Maximum rotation change in degrees while the camera is shaking.")]
        public float ShakeAmplitude = 10.0f;
        /// <summary>
        /// Maximum amplitude multiplier. E.g. with a value of 2, the amplitude can sometimes be twice as high
        /// </summary>
        [Tooltip("Maximum amplitude multiplier. E.g. with a value of 2, the amplitude can sometimes be twice as high.")]
        public float ShakeAmplitudeVariance = 2.0f;

        /// <summary>
        /// Reference to the Unity's PlayerInput component
        /// </summary>
        protected PlayerInput _playerInput;
        /// <summary>
        /// Reference to the used RPG camera script
        /// </summary>
        protected RPGCameraLite _rpgCamera;
        /// <summary>
        /// Reference to the used motor script
        /// </summary>
        protected IMotor _motor;
        /// <summary>
        /// Reference to the used cursor handler
        /// </summary>
        protected ICursorHandler _cursorHandler;
        /// <summary>
        /// True if the orbiting input is pressed (prerequisite for starting orbiting)
        /// </summary>
        protected bool _cameraOrbitingActivation;
        /// <summary>
        /// True if the camera started the orbiting
        /// </summary>
        protected bool _cameraOrbitingActive;
        /// <summary>
        /// Currently running shaking coroutine
        /// </summary>
        protected IEnumerator _shakingCoroutine;
        #region Input actions
        // RPGCamera
        protected InputAction _activateOrbitingAction;
        protected InputAction _orbitingAmountAction;
        protected InputAction _zoomAction;
        protected InputAction _zoomToMinDistanceAction;
        protected InputAction _zoomToMaxDistanceAction;
        // Motor
        protected InputAction _movementAction;
        protected InputAction _rotateAction;
        protected InputAction _jumpAction;
        #endregion
        #region Input values
        // RPGCamera
        /// <summary>
        /// If true, the orbiting input was started in this frame
        /// </summary>
        protected bool _inputActivateOrbitingStart = false;
        /// <summary>
        /// If true, the orbiting input was released in this frame
        /// </summary>
        protected bool _inputActivateOrbitingStop = false;
        /// <summary>
        /// Camera orbiting amount
        /// </summary>
        protected Vector2 _inputOrbitingAmount;
        /// <summary>
        /// Zoom in/out input axis
        /// </summary>
        protected float _inputZoomAmount = 0;
        /// <summary>
        /// Fast zoom to minimum camera distance
        /// </summary>
        protected bool _inputMinDistanceZoom = false;
        /// <summary>
        /// Fast zoom out to maximum camera distance
        /// </summary>
        protected bool _inputMaxDistanceZoom = false;
        // Motor
        /// <summary>
        /// 2D axis input for the movement direction
        /// </summary>
        protected Vector2 _inputMovement;
        /// <summary>
        /// Horizontal character rotation input
        /// </summary>
        protected float _inputRotate = 0;
        /// <summary>
        /// Jump input
        /// </summary>
        protected bool _inputJump = false;
        #endregion

        protected virtual void Awake() {
            _playerInput = GetComponent<PlayerInput>();
            _rpgCamera = GetComponent<RPGCameraLite>();
            _cursorHandler = GetComponent<ICursorHandler>();
            _motor = GetComponent<IMotor>();
        }

        protected virtual void Start() {
            InitializeInputActions();
        }

        protected virtual void Update() {
            GetInputs();

            if (_cameraOrbitingActive) {
                _cursorHandler?.HideCursor();
                _rpgCamera?.Yaw(_inputOrbitingAmount.x * YawSensitivity * Utils.BoolToSign(InvertYawAxis));
                _rpgCamera?.Pitch(_inputOrbitingAmount.y * PitchSensitivity * Utils.BoolToSign(!InvertPitchAxis));
            } else {
                _cursorHandler?.ShowCursor();
            }

            if (AlignCameraWithCharacter(out bool opposed)) {
                _rpgCamera?.AlignWithTransform(transform, opposed);
            }

            if (_inputMinDistanceZoom) {
                _rpgCamera?.ZoomToMinDistance();
            } else if (_inputMaxDistanceZoom) {
                _rpgCamera?.ZoomToMaxDistance();
            } else {
                _rpgCamera?.Zoom(-_inputZoomAmount * ZoomSensitivity);
            }

            _motor?.Move(_inputMovement.y * Time.deltaTime);
            _motor?.Strafe(_inputMovement.x * Time.deltaTime);

            float characterRotation = _motor?.Rotate(_inputRotate * Time.deltaTime) ?? 0;
            if (SyncRotations == RotationSync.Always
                || SyncRotations == RotationSync.PauseOnOrbiting && !_cameraOrbitingActive) {
                _rpgCamera?.Yaw(characterRotation, true);
            }

            if (_inputJump) {
                _motor?.Jump();
            }
        }

        /// <summary>
        /// Initializes the internal input action variables from the PlayerInput component
        /// </summary>
        public virtual void InitializeInputActions(bool logWarnings = false) {
            if (_playerInput == null) {
                _playerInput = GetComponent<PlayerInput>();
            }
            if (_playerInput.actions == null) {
                Debug.LogWarning("There is no Input Action Asset assigned to the Player Input component on game object " + gameObject.name + "! This is a requirement for the RPG Controller to work", gameObject);
                return;
            }
            // RPGCamera
            _activateOrbitingAction = GetInputAction("Activate Orbiting", logWarnings);
            _orbitingAmountAction = GetInputAction("Orbiting Amount", logWarnings);
            _zoomAction = GetInputAction("Zoom", logWarnings);
            _zoomToMinDistanceAction = GetInputAction("Zoom To Min Distance", logWarnings);
            _zoomToMaxDistanceAction = GetInputAction("Zoom To Max Distance", logWarnings);
            // Motor
            _movementAction = GetInputAction("Movement", logWarnings);
            _rotateAction = GetInputAction("Rotate", logWarnings);
            _jumpAction = GetInputAction("Jump", logWarnings);
        }

        protected InputAction GetInputAction(string actionName, bool logWarnings = false) {
            try {
                return _playerInput.actions[actionName];
            } catch (KeyNotFoundException) {
                if (logWarnings) {
                    Debug.LogWarning("Input action " + actionName + " not found in " + _playerInput.actions.name, _playerInput);
                }
                return null;
            }
        }

        /// <summary>
        /// Tries to get the input values used by this script
        /// </summary>
        protected virtual void GetInputs() {
            // RPGCamera
            _inputActivateOrbitingStart = _activateOrbitingAction?.WasPressedThisFrame() ?? false;
            _inputActivateOrbitingStop = _activateOrbitingAction?.WasReleasedThisFrame() ?? false;
            _inputOrbitingAmount = _orbitingAmountAction?.ReadValue<Vector2>() ?? Vector2.zero;
            _inputZoomAmount = _zoomAction?.ReadValue<float>() ?? 0;
            _inputMinDistanceZoom = _zoomToMinDistanceAction?.WasPressedThisFrame() ?? false;
            _inputMaxDistanceZoom = _zoomToMaxDistanceAction?.WasPressedThisFrame() ?? false;
            // Motor
            _inputMovement = _movementAction?.ReadValue<Vector2>() ?? Vector2.zero;
            _inputRotate = _rotateAction?.ReadValue<float>() ?? 0;
            _inputJump = _jumpAction?.WasPressedThisFrame() ?? false;

            SetCameraOrbitingActive();
        }

        protected virtual void SetCameraOrbitingActive() {
            _cameraOrbitingActivation = _cameraOrbitingActivation || (_inputActivateOrbitingStart && !IsCursorOverUI());
            _cameraOrbitingActive = _cameraOrbitingActive || (_cameraOrbitingActivation && _inputOrbitingAmount.magnitude > 0);
            if (_inputActivateOrbitingStop) {
                _cameraOrbitingActivation = false;
                _cameraOrbitingActive = false;
            }
        }

        protected virtual bool IsCursorOverUI() {
            return _cursorHandler?.IsCursorOverUI() ?? false;
        }

        protected virtual bool AlignCameraWithCharacter(out bool opposed) {
            opposed = _inputMovement.y < 0;
            return AlignCameraOnCharacterMovement
                    && _inputMovement.y != 0
                    && !_cameraOrbitingActive;
        }

        /// <summary>
        /// Starts shaking the camera with the given parameters
        /// </summary>
        /// <param name="frequency">Number of shakes per second while the camera is shaking</param>
        /// <param name="amplitude">Maximum rotation change in degrees while the camera is shaking</param>
        /// <param name="variance">Maximum amplitude multiplier. E.g. with a value of 2, the amplitude can sometimes be twice as high</param>
        public virtual void StartShakingCamera(float frequency, float amplitude, float variance) {
            if (IsShakingCamera()) {
                StopShakingCamera();
            }

            _shakingCoroutine = ShakingCameraCoroutine(frequency, amplitude, variance);
            StartCoroutine(_shakingCoroutine);
            _rpgCamera?.SetRotationSmoothTime(1.0f / frequency);
        }

        /// <summary>
        /// Stops shaking the camera
        /// </summary>
        public virtual void StopShakingCamera() {
            if (IsShakingCamera()) {
                StopCoroutine(_shakingCoroutine);
                _shakingCoroutine = null;
                _rpgCamera?.ResetRotationSmoothTime();
            }
        }

        /// <summary>
        /// Checks if the camera is currently being shaked
        /// </summary>
        /// <returns>True if shaked, otherwise false</returns>
        public virtual bool IsShakingCamera() {
            return _shakingCoroutine != null;
        }

        /// <summary>
        /// Coroutine for shaking the camera
        /// </summary>
        protected virtual IEnumerator ShakingCameraCoroutine(float frequency, float amplitude, float variance) {
            Vector2 lastDelta;
            Vector2 _shakingDelta = Vector2.zero;
            while (true) {
                lastDelta = _shakingDelta;
                _shakingDelta = amplitude * Random.Range(1.0f / variance, variance) * Random.insideUnitCircle.normalized;

                if (Vector2.Angle(lastDelta, _shakingDelta) < 90.0f) {
                    // Make it more extreme
                    _shakingDelta *= -1.0f;
                }

                _rpgCamera?.Yaw(_shakingDelta.x);
                _rpgCamera?.Pitch(_shakingDelta.y);
                yield return new WaitForSeconds(1.0f / frequency * Random.Range(0.5f, 0.7f));
                _rpgCamera?.Yaw(-_shakingDelta.x);
                _rpgCamera?.Pitch(-_shakingDelta.y);
            }
        }
    }
}