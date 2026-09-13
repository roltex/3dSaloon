using System;
using UnityEngine;

namespace TheLastBet.Saloon
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;

        public float MaxHealth => maxHealth;
        public float Current { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<Health, float, GameObject> Damaged;
        public event Action<Health, GameObject> Died;

        void Awake()
        {
            ResetHealth();
        }

        public void ResetHealth()
        {
            Current = maxHealth;
            IsDead = false;
        }

        public void ApplyDamage(float amount, GameObject source)
        {
            if (IsDead || amount <= 0f)
                return;

            Current = Mathf.Max(0f, Current - amount);
            Damaged?.Invoke(this, amount, source);
            if (Current > 0f)
                return;

            IsDead = true;
            Died?.Invoke(this, source);
        }
    }
}
