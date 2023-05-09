using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DelayedActions
{
    public enum TimeUnit
    {
        SCALEDTIME,
        UNSCALEDTIME,
        FRAME
    }

    public delegate void DelayedEvent(params object[] args);

    public class CoroutineManager : MonoBehaviour
    {
        private List<DelayedAction> _delayedActions = new List<DelayedAction>();
        private static CoroutineManager _instance;

        public static CoroutineManager Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindObjectOfType<CoroutineManager>();

                if (!_instance)
                {
                    GameObject timer = new GameObject("RoutineObject");
                    _instance = timer.AddComponent<CoroutineManager>();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Starts a new coroutine that will perform the given action after the given time has passed.
        /// </summary>
        /// <param name="action">The action to perform once the timer reaches 0.</param>
        /// <param name="unitOfMeasurement">The unit used to count down the time.</param>
        /// <param name="duration">The amount of time to wait before performing the action</param>
        /// <param name="args">Additional arguments to be sent to the action when performed.</param>
        /// <returns>The instance of the timed action created when started.</returns>
        public TimedAction StartNewTimedAction(DelayedEvent action, TimeUnit unitOfMeasurement, float duration, params object[] args)
        {
            TimedAction timedAction = new TimedAction(action, duration, unitOfMeasurement);
            timedAction.Start(this, args);

            timedAction.OnDelayComplete += arguments => _delayedActions.Remove(timedAction);
            timedAction.OnDelayCancel += argguments => _delayedActions.Remove(timedAction);
            _delayedActions.Add(timedAction);

            return timedAction;
        }

        /// <summary>
        /// Starts a new coroutine that will perform the given action when the given condition is true.
        /// </summary>
        /// <param name="action">The action to perform once the condition is true.</param>
        /// <param name="condition">The condition to check to see if the action can be performed.</param>
        /// <param name="args">Additional argument to give to the action when performed.</param>
        /// <returns>The instance of the condition action thats created after starting.</returns>
        public ConditionAction StartNewConditionAction(DelayedEvent action, Condition condition, params object[] args)
        {
            ConditionAction conditionAction = new ConditionAction(action, condition);
            conditionAction.Start(this, args);

            conditionAction.OnDelayComplete += arguments => _delayedActions.Remove(conditionAction);
            conditionAction.OnDelayCancel += arguments => _delayedActions.Remove(conditionAction);
            _delayedActions.Add(conditionAction);

            return conditionAction;
        }


        /// <summary>
        /// Stops the given action and calls the cancel event.
        /// </summary>
        /// <param name="action">The active action to try to cancel.</param>
        /// <returns>Returns true if the action is currently active and can be canceled.</returns>
        public bool StopAction(DelayedAction action)
        {
            if (action == null)
                return false;

            action.Cancel(this);
            return _delayedActions.Remove(action);
        }
    }
}