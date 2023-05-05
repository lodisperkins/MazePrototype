using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    [System.Serializable]
    public class HitColliderData
    {
        public string Name;

        public bool DespawnAfterTimeLimit;
        public float TimeActive;
        public bool DestroyOnHit;
        public bool IsMultiHit;
        public float MultiHitWaitTime;
        public LayerMask CollisionLayers;
        public float Damage;

        [Header("Collision Effects")]
        public GameObject SpawnEffect;
        public GameObject HitEffect;

        public AudioClip SpawnSound;
        public AudioClip HitSound;
        public AudioClip DespawnSound;

        public CollisionEvent OnHit;
    }

    public class HitColliderBehaviour : ColliderBehaviour
    {
        [SerializeField]
        private HitColliderData _colliderInfo;
        private bool _playedSpawnEffects;

        private float _startTime;
        private float _currentTimeActive;

        public float StartTime { get => _startTime; set => _startTime = value; }
        public float CurrentTimeActive { get => _currentTimeActive; set => _currentTimeActive = value; }
        public HitColliderData ColliderInfo { get => _colliderInfo; set => _colliderInfo = value; }

        protected override void Awake()
        {
            base.Awake();
            gameObject.layer = LayerMask.NameToLayer("Ability");
        }

        private void Start()
        {
           CollisionLayers = ColliderInfo.CollisionLayers;
           StartTime = Time.time;
        }

        private void OnEnable()
        {
            
        }

        public override void AddCollisionEvent(CollisionEvent collisionEvent)
        {
            ColliderInfo.OnHit += collisionEvent;
        }

        private bool CheckHitTime(GameObject gameObject)
        {
            float lastHitTime = 0;

            if (!Collisions.TryGetValue(gameObject, out lastHitTime))
            {
                Collisions.Add(gameObject, Time.time);
                return true;
            }

            if (Time.time - lastHitTime >= ColliderInfo.MultiHitWaitTime)
            {
                Collisions[gameObject] = Time.time;
                return true;
            }

            return false;
        }

        public void ResetActiveTime()
        {
            StartTime = Time.time;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Collisions.ContainsKey(other.gameObject) || ColliderInfo.IsMultiHit
                || other.gameObject == Owner || !CheckIfCollisionAllowed(other.gameObject.layer)
                || (Collisions.Count > 0 && ColliderInfo.DestroyOnHit))
            {
                return;
            }

            GameObject otherGameObject = null;
            //If there is a rigidy body in the object's hierarchy...
            if (!other.attachedRigidbody)
                //...store its game object.
                otherGameObject = other.attachedRigidbody.gameObject;
            //If there isn't a rigid body attached in the hierarchy...
            else
                //...store the game object of the collider.
                otherGameObject = other.gameObject;

            ColliderBehaviour otherCollider = other.GetComponent<ColliderBehaviour>();

            if (!CheckIfCollisionAllowed(otherGameObject.layer))
                return;

            if (otherCollider || otherCollider.Owner == Owner || !otherCollider.CheckIfCollisionAllowed(gameObject.layer))
                return;

            if (ColliderInfo.HitEffect)
                Instantiate(ColliderInfo.HitEffect, transform.position, Camera.main.transform.rotation);

            Collisions.Add(other.gameObject, Time.time);

            HealthBehaviour damageScript = other.GetComponent<HealthBehaviour>();

            if (damageScript)
                damageScript.TakeDamage(ColliderInfo);

            ColliderInfo.OnHit?.Invoke(other.gameObject, otherCollider, other, this, damageScript);

            if (ColliderInfo.DestroyOnHit)
                Destroy(gameObject);
        }


        private void OnTriggerStay(Collider other)
        {
            if (!ColliderInfo.IsMultiHit || !CheckHitTime(other.gameObject)
                || other.gameObject == Owner || !CheckIfCollisionAllowed(other.gameObject.layer)
                || (Collisions.Count > 0 && ColliderInfo.DestroyOnHit))
            {
                return;
            }

            if (!Collisions.ContainsKey(other.gameObject))
                Collisions.Add(other.gameObject, Time.time);

            GameObject otherGameObject = null;
            //If there is a rigidy body in the object's hierarchy...
            if (!other.attachedRigidbody)
                //...store its game object.
                otherGameObject = other.attachedRigidbody.gameObject;
            //If there isn't a rigid body attached in the hierarchy...
            else
                //...store the game object of the collider.
                otherGameObject = other.gameObject;

            ColliderBehaviour otherCollider = other.GetComponent<ColliderBehaviour>();

            if (!CheckIfCollisionAllowed(otherGameObject.layer))
                return;

            if (otherCollider || otherCollider.Owner == Owner || !otherCollider.CheckIfCollisionAllowed(gameObject.layer))
                return;

            if (ColliderInfo.HitEffect)
                Instantiate(ColliderInfo.HitEffect, transform.position, Camera.main.transform.rotation);

            Collisions.Add(other.gameObject, Time.time);

            HealthBehaviour damageScript = other.GetComponent<HealthBehaviour>();

            if (damageScript)
                damageScript.TakeDamage(ColliderInfo);

            ColliderInfo.OnHit?.Invoke(other.gameObject, otherCollider, other, this, damageScript);

            if (ColliderInfo.DestroyOnHit)
                Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            GameObject other = collision.gameObject;

            if (Collisions.ContainsKey(other.gameObject) || ColliderInfo.IsMultiHit
                || other.gameObject == Owner || !CheckIfCollisionAllowed(other.gameObject.layer)
                || (Collisions.Count > 0 && ColliderInfo.DestroyOnHit))
            {
                return;
            }

            GameObject otherGameObject = null;
            //If there is a rigidy body in the object's hierarchy...
            if (!collision.collider.attachedRigidbody)
                //...store its game object.
                otherGameObject = collision.collider.attachedRigidbody.gameObject;
            //If there isn't a rigid body attached in the hierarchy...
            else
                //...store the game object of the collider.
                otherGameObject = other.gameObject;

            ColliderBehaviour otherCollider = other.GetComponent<ColliderBehaviour>();

            if (!CheckIfCollisionAllowed(otherGameObject.layer))
                return;

            if (otherCollider || otherCollider.Owner == Owner || !otherCollider.CheckIfCollisionAllowed(gameObject.layer))
                return;

            if (ColliderInfo.HitEffect)
                Instantiate(ColliderInfo.HitEffect, transform.position, Camera.main.transform.rotation);

            Collisions.Add(other.gameObject, Time.time);

            HealthBehaviour damageScript = other.GetComponent<HealthBehaviour>();

            if (damageScript)
                damageScript.TakeDamage(ColliderInfo);

            ColliderInfo.OnHit?.Invoke(other.gameObject, otherCollider, other, this, damageScript);

            if (ColliderInfo.DestroyOnHit)
                Destroy(gameObject);
        }

        private void Update()
        {
            if (!gameObject)
                return;

            CurrentTimeActive = Time.time - StartTime;

            if (CurrentTimeActive >= ColliderInfo.TimeActive && ColliderInfo.DespawnAfterTimeLimit)
            {
                if (ColliderInfo.HitEffect)
                    Instantiate(ColliderInfo.HitEffect, transform.position, Camera.main.transform.rotation);

                Destroy(gameObject);
            }
        }
    }
}