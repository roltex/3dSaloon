using UnityEngine;
using UnityEngine.AI;

namespace TheLastBet.Saloon
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class GunslingerAI : MonoBehaviour
    {
        [SerializeField] private float detectRange = 16f;
        [SerializeField] private float attackRange = 8.5f;
        [SerializeField] private float loseRange = 22f;
        [SerializeField] private float fireInterval = 1.05f;
        [SerializeField] private float turnSpeed = 540f;

        NavMeshAgent _agent;
        Health _health;
        HitscanWeapon _weapon;
        CharacterAnimDriver _anim;
        Transform _player;
        float _nextFire;
        float _flinchUntil;
        bool _aggro;

        public bool IsAlive => _health == null || !_health.IsDead;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<Health>();
            _weapon = GetComponent<HitscanWeapon>();
            _anim = GetComponent<CharacterAnimDriver>();
            _agent.speed = 3.4f;
            _agent.angularSpeed = 360f;
            _agent.stoppingDistance = 6.4f;
            _agent.acceleration = 10f;
        }

        void OnEnable()
        {
            if (_health == null)
                return;
            _health.Died += OnDied;
            _health.Damaged += OnDamaged;
        }

        void OnDisable()
        {
            if (_health == null)
                return;
            _health.Died -= OnDied;
            _health.Damaged -= OnDamaged;
        }

        public void SetPlayer(Transform player)
        {
            _player = player;
        }

        void Update()
        {
            if (!IsAlive)
            {
                if (_agent.enabled)
                    _agent.isStopped = true;
                _anim?.SetLocomotion(0f, true);
                return;
            }

            if (_player == null)
            {
                var motor = FindAnyObjectByType<SaloonWalkPreview>();
                if (motor != null && motor.isActiveAndEnabled)
                    _player = motor.transform;
            }

            if (_player == null)
            {
                _anim?.SetLocomotion(0f, true);
                return;
            }

            float dist = Vector3.Distance(transform.position, _player.position);
            if (!_aggro && dist <= detectRange)
                _aggro = true;
            if (_aggro && dist > loseRange)
                _aggro = false;

            if (!_aggro || Time.time < _flinchUntil)
            {
                _agent.isStopped = true;
                _anim?.SetLocomotion(0f, true);
                return;
            }

            Vector3 toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude > 0.001f)
            {
                Quaternion facing = Quaternion.LookRotation(toPlayer, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, turnSpeed * Time.deltaTime);
            }

            if (dist > attackRange)
            {
                _agent.isStopped = false;
                _agent.SetDestination(_player.position);
            }
            else
            {
                _agent.isStopped = true;
                TryShoot();
            }

            _anim?.SetLocomotion(_agent.velocity.magnitude, true);
        }

        void TryShoot()
        {
            if (_weapon == null || Time.time < _nextFire)
                return;

            Vector3 origin = transform.position + Vector3.up * 1.35f;
            Vector3 target = _player.position + Vector3.up * 1.4f;
            Vector3 dir = (target - origin).normalized;
            if (_weapon.TryFire(origin, dir, gameObject))
            {
                _anim?.TriggerShoot();
                _nextFire = Time.time + fireInterval;
            }
        }

        void OnDamaged(Health health, float amount, GameObject source)
        {
            _aggro = true;
            _flinchUntil = Time.time + 0.18f;
        }

        void OnDied(Health health, GameObject source)
        {
            _aggro = false;
            if (_agent.enabled)
            {
                _agent.isStopped = true;
                _agent.enabled = false;
            }

            var col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
            _anim?.SetDead();
        }
    }
}
