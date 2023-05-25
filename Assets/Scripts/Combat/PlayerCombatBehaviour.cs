using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Combat
{
    public class PlayerCombatBehaviour : MonoBehaviour
    {
        [SerializeField]
        private AbilityData_SO _abilitySlot1;
        [SerializeField]
        private AbilityData_SO _abilitySlot2;

        private Ability _ability1;
        private Ability _ability2;
        private UnityEvent _onUseAbility;

        public bool AbilityInUse
        {
            get
            {
                return _ability1.InUse || _ability2.InUse;
            }
        }

        // Start is called before the first frame update
        void Awake()
        {
            InitAbilities();
        }

        private void InitAbilities()
        {
            string ability1Name = _abilitySlot1.name.Substring(0, _abilitySlot1.name.Length - 5);
            string ability2Name = _abilitySlot2.name.Substring(0, _abilitySlot2.name.Length - 5);

            Type ability1Type = Type.GetType("Combat." + ability1Name);
            Type ability2Type = Type.GetType("Combat." + ability2Name);

            _ability1 = (Ability)Activator.CreateInstance(ability1Type);
            _ability2 = (Ability)Activator.CreateInstance(ability2Type);
        }

        public Ability GetActiveAbility()
        {
            if (!AbilityInUse)
                return null;

            if (_ability1.InUse)
                return _ability1;

            return _ability2;
        }

        public void AddOnUseAbilityAction(UnityAction action)
        {
            _onUseAbility.AddListener(action);
        }

        public void RemoveOnUseAbilityAction(UnityAction action)
        {
            _onUseAbility.RemoveListener(action);
        }

        public void UseAbility1(params object[] args)
        {
            _ability1.UseAbility(args);
        }

        public void UseAbility2(params object[] args)
        {
            _ability2.UseAbility(args);
        }
    }
}