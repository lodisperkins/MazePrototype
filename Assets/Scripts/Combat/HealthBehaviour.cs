using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Combat
{
    public class HealthBehaviour : MonoBehaviour
    {
        [Tooltip("The measurement of the amount of damage this object can take or has taken")]
        [SerializeField]
        private float _health;
        [Tooltip("The starting amount of damage this object can take or has taken. Set to -1 to start with max health.")]
        [SerializeField]
        private float _startingHealth = -1;
        [Tooltip("The maximum amount of damage this object can take or has taken")]
        [SerializeField]
        private float _maxHealth;
        [Tooltip("Whether or not this object should be deleted if the health is 0")]
        [SerializeField]
        private bool _destroyOnDeath;

        [Tooltip("Event raised when health is set to a lower value.")]
        [SerializeField]
        private UnityEvent _onDamageTaken;
        [Tooltip("Event raised when health is less than or equal to zero.")]
        [SerializeField]
        private UnityEvent _onDeath;

        private void Awake()
        {
            //Set up starting health. If the starting health is negative use max health instead.
            if (_startingHealth < 0)
                _health = MaxHealth;
            else
                _health = _startingHealth;

            //Add destroying the game object to the death event if allowed.
            if (_destroyOnDeath)
                _onDeath.AddListener(() => Destroy(gameObject));
        }

        /// <summary>
        /// The measurement of the amount of damage this object can take or has taken
        /// </summary>
        public float Health 
        {
            get => _health;

            private set
            {
                //If the new value is less than what is allowed...
                if (value <= 0)
                {
                    //Raise the death event if alive and clamp the health.

                    if (_health > 0)
                        _onDeath?.Invoke();

                    _health = 0;
                    return;
                }

                if (value > MaxHealth)
                    value = MaxHealth;

                _health = value;
            }
        }

        //The maximum amount of damage this object can take.
        public float MaxHealth { get => _maxHealth; set => _maxHealth = value; }

        /// <summary>
        /// Subtracts from the main health value and calls the OnDamageTaken event.
        /// </summary>
        /// <param name="damage">The amount to subtract from this objects health.</param>
        /// <returns>The amount of damage done to the object.</returns>
        public float TakeDamage(float damage)
        {
            float damageTaken = Health;

            Health -= damage;

            damageTaken -= Health;

            _onDamageTaken?.Invoke();
            return damageTaken;
        }

        /// <summary>
        /// Subtracts from the main health value and calls the OnDamageTaken event.
        /// </summary>
        /// <param name="damage">The amount to subtract from this objects health.</param>
        /// <returns>The amount of damage done to the object.</returns>
        public float TakeDamage(HitColliderData info)
        {
            float damageTaken = Health;

            Health -= info.Damage;

            damageTaken -= Health;

            _onDamageTaken?.Invoke();
            return damageTaken;
        }

        /// <summary>
        /// Resets the health to it's starting point.
        /// Will set it to the maximum health if the starting health is negative.
        /// </summary>
        public void ResetHealth()
        {
            Health = _startingHealth == -1 ? MaxHealth : _startingHealth;
        }

        /// <summary>
        /// Adds a new listener to the OnDeath event. Invoked when health is less than or equal to zero.
        /// </summary>
        /// <param name="action">The action to perform when this object has died.</param>
        public void AddOnDeathAction(UnityAction action)
        {
            _onDeath.AddListener(action);
        }

        /// <summary>
        /// Adds a new listener to the OnDamageTaken event. Invoked when health is set to a lower value.
        /// </summary>
        /// <param name="action">The action to perform when this object has been hit.</param>
        public void AddOnDamageTakenAction(UnityAction action)
        {
            _onDamageTaken.AddListener(action);
        }
    }
}