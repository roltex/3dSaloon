using UnityEngine;

namespace JohnStairs.RPG.Character {
    /// <summary>
    /// Code scaffold taken from Unity's Character Controller documentation at https://docs.unity3d.com/ScriptReference/CharacterController.Move.html
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class MotorExample : MonoBehaviour, IMotor {
        public float MovementSpeed = 4.0f;
        public float RotationSpeed = 180.0f;
        public float JumpHeight = 1.0f;
        public float Gravity = -9.81f;

        protected CharacterController _controller;
        protected Vector3 _playerVelocity;
        protected bool _groundedPlayer;
        protected float _forward;
        protected float _strafe;
        protected bool _jump;

        protected virtual void Awake() {
            _controller = GetComponent<CharacterController>();
        }

        protected virtual void Update() {
            _groundedPlayer = _controller.isGrounded;
            if (_groundedPlayer && _playerVelocity.y < 0) {
                _playerVelocity.y = 0;
            }

            Vector3 move = transform.forward * _forward + transform.right * _strafe;
            _controller.Move(move);

            // Changes the height position of the player
            if (_jump && _groundedPlayer) {
                _jump = false;
                _playerVelocity.y += Mathf.Sqrt(JumpHeight * -3.0f * Gravity);
            }

            _playerVelocity.y += Gravity * Time.deltaTime;
            _controller.Move(_playerVelocity * Time.deltaTime);
        }

        public virtual void Move(float units) {
            _forward = units * MovementSpeed;
        }

        public virtual void Strafe(float units) {
            _strafe = units * MovementSpeed;
        }

        public virtual float Rotate(float degrees) {
            float rotatedDegrees = degrees * RotationSpeed;
            transform.Rotate(Vector3.up, rotatedDegrees, Space.World);
            return rotatedDegrees;
        }

        public virtual void Jump() {
            _jump = true;
        }
    }
}
