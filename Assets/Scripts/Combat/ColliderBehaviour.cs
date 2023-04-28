using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{ 
    /// <summary>
    /// Event used when collisions occur. 
    /// arg[0] = The game object collided with.
    /// arg[1] = The collision script for the object that was collided with
    /// arg[2] = The collider object that was collided with. Is type Collider if trigger and type Collision if not.
    /// arg[3] = The collider behaviour of the object that raised this event. 
    /// arg[4] = The health script of the object that was collided with.
    /// </summary>
    /// <param name="args"></param>
    public delegate void CollisionEvent(params object[] args);

    public class ColliderBehaviour : MonoBehaviour
    {
        private Dictionary<GameObject, float> _collisions;

        private CollisionEvent _onHit;
        [SerializeField]
        private GameObject _owner;
        [SerializeField]
        private LayerMask _collisionLayers;

        private Rigidbody _rigidbody;

        public LayerMask CollisionLayers { get => _collisionLayers; set => _collisionLayers = value; }
        public Rigidbody RB { get => _rigidbody; private set => _rigidbody = value; }
        public Dictionary<GameObject, float> Collisions { get => _collisions; protected set => _collisions = value; }
        public GameObject Owner { get => _owner; set => _owner = value; }

        protected virtual void Awake()
        {
            Collisions = new Dictionary<GameObject, float>();
            RB = transform.root.GetComponentInChildren<Rigidbody>();
        }

        /// <summary>
        /// Copies the values in collider 1 to collider 2.
        /// </summary>
        /// <param name="collider1">The collider that will have its values copied.</param>
        /// <param name="collider2">The collider that will have its values overwritten.</param>
        public static void Copy(ColliderBehaviour collider1, ColliderBehaviour collider2)
        {
            collider2._onHit = collider1._onHit;
            collider2.CollisionLayers = collider1.CollisionLayers;
            collider2.Collisions = collider1.Collisions;
        }

        // <summary>
        /// Checks if the layer is in the collider's layer mask of acceptable collisions.
        /// </summary>
        /// <param name="layer">The unity physics collision layer of the game object.</param>
        public bool CheckIfCollisionAllowed(int layer)
        {
            if (CollisionLayers == 0)
                return false;

            int mask = CollisionLayers;
            if (mask == (mask | 1 << layer))
                return true;

            return false;
        }

        public virtual void AddCollisionEvent(CollisionEvent collisionEvent)
        {
            _onHit += collisionEvent;
        }

        public virtual void RemoveCollisionEvent(CollisionEvent collisionEvent)
        {
            _onHit -= collisionEvent;
        }

        public virtual void RemoveAllCollisionEventListeners()
        {
            _onHit = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == Owner)
                return;


        }
    }
}