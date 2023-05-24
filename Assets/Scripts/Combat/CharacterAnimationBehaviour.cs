using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public class CharacterAnimationBehaviour : MonoBehaviour
    {
        [SerializeField]
        private HealthBehaviour _healthBehaviour;
        [SerializeField]
        private Animator _animator;
        private Ability _currentAbilityAnimating;
        private AnimationClip _currentClip;
        [SerializeField]
        private RuntimeAnimatorController _controller;
        private AnimatorOverrideController _overrideController;
        private int _animationPhase;
        private bool _animatingMotion;

        [SerializeField]
        private AnimationClip _defaultCastAnimation;
        [SerializeField]
        private AnimationClip _defaultSummonAnimation;
        [SerializeField]
        private AnimationClip _defaultMeleeAnimation;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}