using UnityEngine;

namespace TheLastBet.Saloon
{
    public sealed class CharacterAnimDriver : MonoBehaviour
    {
        static readonly int SpeedId = Animator.StringToHash("Speed");
        static readonly int GroundedId = Animator.StringToHash("Grounded");
        static readonly int JumpId = Animator.StringToHash("Jump");
        static readonly int ShootId = Animator.StringToHash("Shoot");
        static readonly int DeadId = Animator.StringToHash("Dead");

        [SerializeField] private Animator animator;
        CharacterController _controller;
        Transform _leftHand;
        Transform _rightHand;
        Quaternion _leftHandBind;
        Quaternion _rightHandBind;

        void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            _controller = GetComponent<CharacterController>();
            CacheBindHands();
        }

        void CacheBindHands()
        {
            if (animator == null || !animator.isHuman)
                return;
            _leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (_leftHand != null)
                _leftHandBind = _leftHand.localRotation;
            if (_rightHand != null)
                _rightHandBind = _rightHand.localRotation;
        }

        void LateUpdate()
        {
            PlantFeetOnRoot();
            RestoreBindHands();
        }

        void RestoreBindHands()
        {
            if (_leftHand != null)
                _leftHand.localRotation = _leftHandBind;
            if (_rightHand != null)
                _rightHand.localRotation = _rightHandBind;
        }

        void PlantFeetOnRoot()
        {
            if (animator == null || !animator.enabled || !animator.isHuman)
                return;
            if (_controller != null && !_controller.isGrounded && _controller.velocity.y > 1f)
                return;

            Transform left = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform right = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            if (left == null || right == null)
                return;

            float feetY = Mathf.Min(left.position.y, right.position.y);
            Transform leftToe = animator.GetBoneTransform(HumanBodyBones.LeftToes);
            Transform rightToe = animator.GetBoneTransform(HumanBodyBones.RightToes);
            if (leftToe != null)
                feetY = Mathf.Min(feetY, leftToe.position.y);
            if (rightToe != null)
                feetY = Mathf.Min(feetY, rightToe.position.y);

            float delta = transform.position.y - feetY;
            float maxPlant = 1.6f * Mathf.Max(1f, transform.lossyScale.y);
            if (Mathf.Abs(delta) < 0.003f || Mathf.Abs(delta) > maxPlant)
                return;

            animator.transform.position += new Vector3(0f, delta, 0f);
        }

        public void SetLocomotion(float speed, bool grounded)
        {
            if (animator == null)
                return;
            animator.SetFloat(SpeedId, speed);
            animator.SetBool(GroundedId, grounded);
        }

        public void TriggerJump()
        {
            if (animator != null)
                animator.SetTrigger(JumpId);
        }

        public void TriggerShoot()
        {
            if (animator != null)
                animator.SetTrigger(ShootId);
        }

        public void SetDead()
        {
            if (animator == null)
                return;
            animator.ResetTrigger(JumpId);
            animator.ResetTrigger(ShootId);
            animator.SetBool(DeadId, true);
        }

        public void ResetAlive()
        {
            if (animator == null)
                return;
            animator.SetBool(DeadId, false);
            animator.SetFloat(SpeedId, 0f);
            animator.Play("Locomotion", 0, 0f);
        }
    }
}
