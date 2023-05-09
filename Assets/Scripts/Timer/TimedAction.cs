using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DelayedActions
{
    public class TimedAction : DelayedAction
    {
        private float _timeStarted;
        private float _duration;
        private TimeUnit _unit;


        public TimedAction(DelayedEvent action, float duration, TimeUnit unit = TimeUnit.SCALEDTIME)
        {
            OnDelayComplete = action;
            _duration = duration;
            _unit = unit;
        }

        /// <summary>
        /// The time that this action began. Value varies based on specified unit at start.
        /// </summary>
        public float TimeStarted { get => _timeStarted; private set => _timeStarted = value; }

        /// <summary>
        /// The amount of time this action has left. Value varies based on specified unit at start.
        /// </summary>
        public float Duration { get => _duration; private set => _duration = value; }

        /// <summary>
        /// The unit of time to use to measure the duration of this action.
        /// </summary>
        public TimeUnit Unit { get => _unit; private set => _unit = value; }

        protected override IEnumerator PerformAction(params object[] args)
        {
            if (_unit == TimeUnit.SCALEDTIME)
            {
                _timeStarted = Time.time;
                yield return new WaitForSeconds(_duration);
            }
            else if (_unit == TimeUnit.UNSCALEDTIME)
            {
                _timeStarted = Time.unscaledTime;
                yield return new WaitForSecondsRealtime(_duration);
            }

            IsActive = false;
            OnDelayComplete(args);
        }
    }
}