using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{

    /// <summary>
    /// Enter ability description here
    /// </summary>
    public class Sword : Ability
    {
        private GameObject _swordInstance;
        private Collider _collider;
        private HitColliderBehaviour _hitCollider;

        //Called when ability is created
        protected override void OnInit(CombatBehaviour newOwner, AbilityData_SO data)
        {
            _swordInstance = CombatBehaviour.Instantiate(AbilityData.VisualPrefab);

            _hitCollider = _swordInstance.GetComponent<HitColliderBehaviour>();
            _hitCollider.Owner = Owner.gameObject;
            _hitCollider.ColliderInfo = AbilityData.GetCollliderInfo(0);

            Owner.HoldItemInRight(_swordInstance);

            _collider = _swordInstance.GetComponentInChildren<Collider>();
            _collider.enabled = false;
        }

        //Called when ability is used
        protected override void OnActivate(params object[] args)
        {
            _collider.enabled = true;
        }

        protected override void OnRecover(params object[] args)
        {
            base.OnRecover(args);
            _collider.enabled = false;
        }
    }
}