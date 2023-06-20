using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Combat;

namespace Animation
{
    enum AnimationPhase
    {
        STARTUP,
        ACTIVE,
        RECOVER
    }

    [RequireComponent(typeof(Animator))]
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
        [SerializeField]
        private CombatBehaviour _combat;
        [SerializeField]
        private MovementBehaviour _movement;
        private AnimatorOverrideController _overrideController;
        private int _animationPhase;
        private bool _animatingMotion;

        private float _currentClipStartUpTime;
        private float _currentClipActiveTime;
        private float _currentClipRecoverTime;

        [SerializeField]
        private AnimationClip _defaultCastAnimation;
        [SerializeField]
        private AnimationClip _defaultDefendAnimation;
        [SerializeField]
        private AnimationClip _defaultSwingAnimation;

        // Start is called before the first frame update
        void Awake()
        {
            _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
            _animator.runtimeAnimatorController = _overrideController;

            _combat?.AddOnUseAbilityAction(PlayAbilityAnimation);
        }

        private void IncrementAnimationPhase()
        {
            _animationPhase++;

        }

        private int GetNextPhaseEventIndex(float currentAnimationTime = 0)
        {
            int eventIndex = 0;

            for (int i = 0; i < _currentClip.events.Length; i++)
            {
                if (_currentClip.events[i].functionName == "IncrementAnimationPhase" && currentAnimationTime == 0)
                    break;
                else if (_currentClip.events[i].functionName != "IncrementAnimationPhase" || Mathf.Abs(currentAnimationTime - _currentClip.events[i].time) > 0.05f)
                    eventIndex++;
                else
                    break;
            }

            return eventIndex;
        }

        private int GetNextPhaseEventIndex(int eventIndex)
        {
            eventIndex++;

            for (int i = eventIndex; i < _currentClip.events.Length; i++)
            {
                if (_currentClip.events[i].functionName != "IncrementAnimationPhase")
                    eventIndex++;
                else
                    break;
            }

            return eventIndex;
        }

        public void CalculateAnimationSpeed()
        {
            AnimatorStateInfo stateInfo;

            AnimationPhase phase = (AnimationPhase)_animationPhase;
            float newSpeed = 1;
            int eventIndex = 0;


            switch (phase)
            {
                case AnimationPhase.STARTUP:

                    if (_animator.GetNextAnimatorClipInfo(0).Length <= 0)
                        return;

                    _currentClip = _animator.GetNextAnimatorClipInfo(0)[0].clip;

                    if (!_currentClip || _currentClip.events.Length <= 0)
                        return;

                    stateInfo = _animator.GetNextAnimatorStateInfo(0);

                    if (_currentClipStartUpTime <= 0 && _combat.AbilityInUse)
                    {
                        _animator.Play(stateInfo.shortNameHash, 0, _currentClip.events[0].time);
                        break;
                    }

                    eventIndex = GetNextPhaseEventIndex(_currentClip.length * (stateInfo.normalizedTime % 1));

                    if (eventIndex < 0 || eventIndex >= _currentClip.events.Length)
                        break;

                    newSpeed = (_currentClip.events[eventIndex].time / _currentClipStartUpTime);

                    break;

                case AnimationPhase.ACTIVE:
                    if (!_currentClip)
                        _currentClip = _animator.GetNextAnimatorClipInfo(0)[0].clip;

                    if ((_currentClipActiveTime <= 0 || !_combat.AbilityInUse && (int)_combat.GetActiveAbility().CurrentPhase > 1)
                        && _currentClip.events.Length >= 2)
                    {
                        _animator.playbackTime = _currentClip.events[0].time;
                        break;
                    }

                    stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

                    float nextTimeStamp = _currentClip.length;
                    eventIndex = 0;

                    if (_currentClip.events.Length > 1)
                    {
                        eventIndex = GetNextPhaseEventIndex(_currentClip.length * (stateInfo.normalizedTime % 1));
                        int nextEventIndex = GetNextPhaseEventIndex(eventIndex);

                        if (nextEventIndex < 0 || nextEventIndex >= _currentClip.events.Length)
                            break;

                        nextTimeStamp = _currentClip.events[nextEventIndex].time;
                    }

                    if (eventIndex < 0 || eventIndex >= _currentClip.events.Length)
                        break;

                    newSpeed = (nextTimeStamp - _currentClip.events[eventIndex].time / _currentClipActiveTime);

                    break;
                case AnimationPhase.RECOVER:
                    if (_currentClip.events.Length < 2)
                        break;
                    else if (_currentClipRecoverTime <= 0)
                    {
                        _animator.playbackTime = _currentClip.length;
                        break;
                    }

                    stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                    eventIndex = GetNextPhaseEventIndex(eventIndex);

                    newSpeed = (_currentClip.length - _currentClip.events[eventIndex].time) / _currentClipRecoverTime;
                    break;
            }

            _animator.SetFloat("AnimationSpeedScale", newSpeed);
        }

        public void StopCurrentAnimation()
        {
            _animator.Rebind();
            _animator.StopPlayback();
            _overrideController["ActivateAbility"] = _controller.animationClips[0];
        }

        public void PlayAbilityAnimation()
        {
            Ability ability = _combat.GetActiveAbility();

            _currentAbilityAnimating = ability;
            _animator.SetFloat("AnimationSpeedScale", 1);
            _animationPhase = 0;

            StopCurrentAnimation();

            switch (ability.AbilityData.TypeOfAnimation)
            {
                case AnimationType.CAST:

                    if (!_defaultCastAnimation)
                    {
                        Debug.LogError("Couldn't play Cast animation. Couldn't find the Cast clip for " + ability.AbilityData.AbilityName);
                        return;
                    }


                    _currentClip = _defaultCastAnimation;
                    _overrideController["ActivateAbility"] = _defaultCastAnimation;
                    _animatingMotion = false;
                    _animationPhase = 0;

                    break;
                case AnimationType.SWING:

                    if (!_defaultSwingAnimation)
                    {
                        Debug.LogError("Couldn't play Swing animation. Couldn't find the Swing clip for " + ability.AbilityData.AbilityName);
                        return;
                    }


                    _currentClip = _defaultSwingAnimation;
                    _overrideController["ActivateAbility"] = _defaultSwingAnimation;
                    _animatingMotion = false;
                    _animationPhase = 0;

                    break;
                case AnimationType.DEFEND:

                    if (!_defaultDefendAnimation)
                    {
                        Debug.LogError("Couldn't play Defend animation. Couldn't find the Defend clip for " + ability.AbilityData.AbilityName);
                        return;
                    }


                    _currentClip = _defaultDefendAnimation;
                    _overrideController["ActivateAbility"] = _defaultDefendAnimation;
                    _animatingMotion = false;
                    _animationPhase = 0;

                    break;
                case AnimationType.CUSTOM:

                    if (!ability.AbilityData.GetCustomAnimation(out _currentClip))
                    {
                        Debug.LogError("Can't play custom clip. No custom clip found for " + ability.AbilityData.AbilityName);
                        return;
                    }

                    _overrideController["ActivateAbility"] = _currentClip;

                    _animatingMotion = false;

                    _animationPhase = 0;
                    break;
            }

            if (ability.AbilityData.UseAbilityTimingForAnimation)
            {
                _currentClipStartUpTime = ability.AbilityData.StartUpTime;
                _currentClipActiveTime = ability.AbilityData.ActiveTime;
                _currentClipRecoverTime = ability.AbilityData.RecoverTime;
            }
            else
                _animationPhase = 3;

            _animator.ResetTrigger("Attack");
            _animator.Update(Time.deltaTime);
            _animator.SetTrigger("Attack");
        }

        // Update is called once per frame
        void Update()
        {
            _animator.SetFloat("MovementSpeed", _movement.CurrentSpeed);
        }
    }
}