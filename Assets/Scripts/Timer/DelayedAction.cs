using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DelayedActions
{
    /// <summary>
    /// Stores some action to perform after some condition has been met.
    /// </summary>
    public abstract class DelayedAction 
    {
        private bool _isActive;
        private DelayedEvent _onDelayUp;
        private DelayedEvent _onDelayCancel;
        private Coroutine _routine;

        public bool IsActive { get => _isActive; protected set => _isActive = value; }
        public DelayedEvent OnDelayComplete { get => _onDelayUp; set => _onDelayUp = value; }
        public DelayedEvent OnDelayCancel { get => _onDelayCancel; set => _onDelayCancel = value; }
        public Coroutine Routine { get => _routine; protected set => _routine = value; }

        public void Start(CoroutineManager routineBehaviour, params object[] args)
        {
            IsActive = true;
            _routine = routineBehaviour.StartCoroutine(PerformAction(args));
        }

        public void Cancel(CoroutineManager routineBehaviour)
        {
            if (_routine == null)
                return;

            routineBehaviour.StopCoroutine(_routine);
            IsActive = false;
            _onDelayCancel?.Invoke();
        }

        protected abstract IEnumerator PerformAction(params object[] args);
    }
}