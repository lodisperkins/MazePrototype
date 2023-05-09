using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DelayedActions
{
    public delegate bool Condition(params object[] args);

    /// <summary>
    /// An action that starts after some custom boolean condition has been met.
    /// </summary>
    public class ConditionAction : DelayedAction
    {
        private Condition _actionCheck;

        /// <summary>
        /// The condition that will be evaluated to check if the action can be performed.
        /// </summary>
        public Condition ActionCheck { get => _actionCheck; private set => _actionCheck = value; }

        public ConditionAction(DelayedEvent action, Condition condition)
        {
            OnDelayComplete = action;

            _actionCheck = condition;
        }

        /// <summary>
        /// Calls the condition to check if the action can be performed.
        /// </summary>
        /// <returns>True if the condition is true and the action can be performed.</returns>
        protected override IEnumerator PerformAction(params object[] args)
        {
            yield return new WaitUntil(() => _actionCheck?.Invoke(args) == true);
            IsActive = false;
            OnDelayComplete(args);
        }

    }
}