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
        public bool DestroyHit;
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

        public 
    }
    public class HitColliderBehaviour : ColliderBehaviour
    {

    }
}