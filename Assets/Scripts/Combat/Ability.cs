
using DelayedActions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

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
        private List<HitColliderData> _colliderInfo = new List<HitColliderData>();
        private TimedAction _currentTimer;
        private HealthBehaviour _ownerHealth;

        private CombatBehaviour _owner;
        private AbilityData_SO _abilityData;

        private UnityEvent _onStart;
        private UnityEvent _onActivate;
        private UnityEvent _onRecover;
        private UnityEvent _onEnd;

        private CollisionEvent _onHit;
        private CollisionEvent _onHitTemp;
        private AbilityPhase _currentPhase;

        private bool _hitEnemy;

        public bool InUse { get => _inUse; private set => _inUse = value; }
        public HealthBehaviour OwnerHealth { get => _ownerHealth; private set => _ownerHealth = value; }
        public CombatBehaviour Owner { get => _owner; private set => _owner = value; }
        public AbilityPhase CurrentPhase { get => _currentPhase; private set => _currentPhase = value; }
        public AbilityData_SO AbilityData { get => _abilityData; private set => _abilityData = value; }

        public void AddOnStartAction(UnityAction action)
        {
            _onStart.AddListener(action);
        }

        public void AddOnActivateAction(UnityAction action)
        {
            _onActivate.AddListener(action);
        }

        public void AddOnRecoverAction(UnityAction action)
        {
            _onRecover.AddListener(action);
        }

        public void AddOnEndAction(UnityAction action)
        {
            _onEnd.AddListener(action);
        }

        public void AddOnHitAction(CollisionEvent action)
        {
            _onHit += action;
        }

        public void AddOnHitTempAction(CollisionEvent action)
        {
            _onHitTemp += action;
        }

        public void Init(CombatBehaviour owner, AbilityData_SO data)
        {
            Owner = owner;
            AbilityData = data;
            OwnerHealth = owner.GetComponent<HealthBehaviour>();

            for (int i = 0; i < AbilityData.ColliderInfoCount; i++)
            {
                HitColliderData info = AbilityData.GetCollliderInfo(i);
                _colliderInfo.Add(info);
            }

            OnInit(owner, data);
        }

        private void StartUpPhase(params object[] args)
        {
            CurrentPhase = AbilityPhase.STARTUP;
            _onStart?.Invoke();

            _currentTimer = CoroutineManager.Instance.StartNewTimedAction(ActivePhase, TimeUnit.SCALEDTIME, AbilityData.StartUpTime, args);

            for (int i = 0; i < _colliderInfo.Count; i++)
            {
                HitColliderData data = _colliderInfo[i];
                data.OnHit += _onHit;
                data.OnHit += context => { _onHitTemp?.Invoke(); _onHitTemp = null; };

                _colliderInfo[i] = data;
            }

            OnStart(args);
        }

        private void ActivePhase(params object[] args)
        {
            CurrentPhase = AbilityPhase.ACTIVE;
            _onActivate?.Invoke();
            OnActivate(args);

            _currentTimer = CoroutineManager.Instance.StartNewTimedAction(RecoverPhase, TimeUnit.SCALEDTIME, AbilityData.StartUpTime, args);
        }

        private void RecoverPhase(params object[] args)
        {
            CurrentPhase = AbilityPhase.RECOVER;
            _onRecover?.Invoke();
            OnRecover(args);

            _currentTimer = CoroutineManager.Instance.StartNewTimedAction(arguments => EndAbility(), TimeUnit.SCALEDTIME, AbilityData.RecoverTime);
        }

        public void EndAbility()
        {
            CoroutineManager.Instance.StopAction(_currentTimer);
            _onEnd?.Invoke();
            _onEnd = null;

            _inUse = false;
        }

        protected virtual void OnInit(CombatBehaviour owner, AbilityData_SO data) { }
        protected virtual void OnStart(params object[] args) { }
        protected virtual void OnActivate(params object[] args) { }
        protected virtual void OnRecover(params object[] args) { }

        public void UseAbility(params object[] args)
        {
            _inUse = true;
            StartUpPhase(args);
        }
    }

#if UNITY_EDITOR
    public class CustomAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        private static string _nameOfClass;

        static void OnWillCreateAsset(string assetName)
        {
            //If the file wasn't created in the abilities folder, return
            if (!assetName.Contains("Assets/Scripts/Combat/Abilities/"))
                return;

            //Break apart the string to get the name of the class 
            string[] substrings = assetName.Split('/', '.');
            _nameOfClass = substrings[substrings.Length - 3];

            //Write the name of the class to a text file for later use
            Stream stream = File.Open("Assets/Resources/LastAssetCreated.txt", FileMode.OpenOrCreate);
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(_nameOfClass);

            //Close the file
            writer.Close();
        }

        /// <summary>
        /// Creates data for a recently created ability if none exists already
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        public static void TryCreateAbilityData()
        {
            //Return if there is no text file contatining the ability class name
            if (!File.Exists("Assets/Resources/LastAssetCreated.txt"))
                return;

            //Initialize stream reader
            Stream stream = File.Open("Assets/Resources/LastAssetCreated.txt", FileMode.Open);
            StreamReader reader = new StreamReader(stream);

            //Stores the ability class name
            _nameOfClass = reader.ReadToEnd();

            //Close reader and delete unneeded file
            reader.Close();
            File.Delete("Assets/Resources/LastAssetCreated.txt");

            //Return if no name was found in the text file
            if (_nameOfClass == "")
                return;

            //Add the namespace to get the full class name for the ability
            string className = "Combat." + _nameOfClass;
            //Find the new ability type using the full class name
            Type assetType = Type.GetType(className);

            //Get a reference to the base types
            Type baseType = Type.GetType("Combat.Ability");

            //Check if there is already an ability data asset for this ability
            string[] results = AssetDatabase.FindAssets(_nameOfClass + "_Data", new[] { "Assets/Resources/AbilityData" });
            if (results.Length > 0)
            {
                return;
            }

            //If there is no ability data, create one based on it's base type
            AbilityData_SO newData = null;
            if (assetType.BaseType == baseType)
                newData = ScriptableObject.CreateInstance<AbilityData_SO>();

            //If the instance was created successfully, create a new asset using the instance 
            if (newData != null)
            {
                AssetDatabase.CreateAsset(newData, "Assets/Resources/AbilityData/" + _nameOfClass + "_Data.Asset");
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/Resources/AbilityData/" + _nameOfClass + "_Data.Asset");
                Debug.Log("Generated ability data for " + _nameOfClass);
            }

        }


    }
#endif
}
