
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public enum AbilityPhase
    {
        STARTUP,
        ACTIVE,
        RECOVER
    }

    [System.Serializable]
    public abstract class Ability
    {
        private bool _inUse;
        private bool _canPlayAnimation;
        private List<HitColliderData> _colliderInfo;

    }
}